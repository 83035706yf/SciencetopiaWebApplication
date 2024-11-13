using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Sciencetopia.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Sciencetopia.Data;

namespace Sciencetopia.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string conversationId, string senderId, string receiverId, string content)
        {
            // Ensure the conversation exists
            var conversation = await _context.Conversations
                                     .Include(c => c.Messages)
                                     .FirstOrDefaultAsync(c => c.Id == conversationId);

            // If the conversation does not exist, optionally create a new one (or handle as needed)
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Messages = new List<Message>()
                };
                _context.Conversations.Add(conversation);
            }

            // Create and save the new message
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                SentTime = DateTimeOffset.UtcNow,
                SenderId = senderId,
                ReceiverId = receiverId,
                ConversationId = conversation.Id,
                IsRead = false // Assuming the message is unread when first sent
            };

            conversation.Messages.Add(message);
            await _context.SaveChangesAsync();

            // After saving the message, convert it to DTO before sending
            var messageDto = new MessageWithUserDetailsDTO
            {
                Id = message.Id,
                Content = message.Content,
                SentTime = message.SentTime,
                Sender = new UserDetailsDTO
                {
                    Id = message.SenderId,
                    UserName = await _context.Users
                        .Where(u => u.Id == message.SenderId)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync(),
                    AvatarUrl = await _context.Users
                        .Where(u => u.Id == message.SenderId)
                        .Select(u => u.AvatarUrl)
                        .FirstOrDefaultAsync()
                }
            };

            var lastMessageSentTime = message.SentTime;

            // Send the message to the receiver
            await Clients.User(receiverId).SendAsync("ReceiveMessage", conversationId, messageDto);

            // Update unread message count for the receiver
            var unreadMessageCount = await _context.Messages
                .Where(m => m.ReceiverId == receiverId && !m.IsRead)
                .CountAsync();

            // Notify client of updated message count per conversation
            var conversationMessageCount = await _context.Messages
                .Where(m => m.ReceiverId == receiverId && m.ConversationId == conversationId && !m.IsRead)
                .CountAsync();

            // Send notification to the receiver
            await Clients.User(receiverId).SendAsync("updateMessages", unreadMessageCount);
            await Clients.User(receiverId).SendAsync("updateConversationMessages", conversationId, conversationMessageCount);
        }

        public async Task<GroupedMessageDTO> GetOrStartConversation(string userId1, string userId2)
        {
            // Check if a conversation already exists between these users
            var existingConversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Messages.Any(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                                                               (m.SenderId == userId2 && m.ReceiverId == userId1)));

            if (existingConversation != null)
            {
                // Identify the partner ID (the other user in the conversation)
                var partnerMessage = existingConversation.Messages.FirstOrDefault();
                var partnerId = partnerMessage.SenderId == userId1 ? partnerMessage.ReceiverId : partnerMessage.SenderId;

                // Return minimal information since details will be fetched on the frontend
                return new GroupedMessageDTO
                {
                    ConversationId = existingConversation.Id,
                    PartnerId = partnerId
                };
            }

            // If no existing conversation, create a new conversation entry
            var newConversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                Messages = new List<Message>()
            };
            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            // Fetch partner information directly from the Identity system
            var partner = await _context.Users
                .Where(u => u.Id == userId2)
                .Select(u => new { u.UserName, u.AvatarUrl })
                .FirstOrDefaultAsync();

            if (partner == null)
            {
                throw new Exception("User not found.");
            }

            // Return a new GroupedMessageDTO for the newly created conversation
            return new GroupedMessageDTO
            {
                ConversationId = newConversation.Id,
                PartnerId = userId2,
                PartnerName = partner.UserName,
                PartnerAvatarUrl = partner.AvatarUrl,
                UnreadMessageCount = 0,
                Messages = new List<MessageWithUserDetailsDTO>()  // Empty list for a new conversation
            };
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentTime)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentTime = m.SentTime,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("LoadHistory", messages);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (userId != null)
            {
                // Retrieve all conversations where the user is involved
                var conversations = await _context.Conversations
                    .Where(c => c.Messages.Any(m => m.SenderId == userId || m.ReceiverId == userId))
                    .ToListAsync();

                // Loop through each conversation and get the count of unread messages for that conversation
                foreach (var conversation in conversations)
                {
                    var unreadMessageCount = await _context.Messages
                        .Where(m => m.ConversationId == conversation.Id && m.ReceiverId == userId && !m.IsRead)
                        .CountAsync();

                    // Send the unread message count for each conversation to the user
                    await Clients.Caller.SendAsync("updateConversationMessages", conversation.Id, unreadMessageCount);
                }

                // Send the total unread message count to the connected user
                var totalUnreadMessageCount = await _context.Messages
                    .Where(m => m.ReceiverId == userId && !m.IsRead)
                    .CountAsync();

                await Clients.Caller.SendAsync("updateMessages", totalUnreadMessageCount);

                // Similiarly, retrieve all notifications for the user and send them to the client
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new
                    {
                        n.Id,
                        n.Content,
                        n.CreatedAt,
                        n.IsRead,
                        n.Type,
                        n.Data
                    })
                    .ToListAsync();

                // Send the unread notification count to the user
                var unreadNotificationCount = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .CountAsync();

                await Clients.Caller.SendAsync("updateNotifications", unreadNotificationCount);
            }

            await base.OnConnectedAsync();
        }

        public async Task MarkMessagesAsRead(string conversationId, string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            // Notify client of updated message count
            var receiverMessageCount = await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();

            // Notify client of updated message count per conversation
            var conversationMessageCount = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.ConversationId == conversationId && !m.IsRead)
                .CountAsync();

            await Clients.User(userId).SendAsync("updateMessages", receiverMessageCount);
            await Clients.User(userId).SendAsync("updateConversationMessages", conversationId, conversationMessageCount);
        }

        // Sends a notification to a specific user
        public async Task SendNotificationToUsers(List<string> userIds, string content, string type, string data)
        {
            var notifications = new List<Notification>();

            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    UserId = userId,
                    Type = type,
                    Data = data
                };

                notifications.Add(notification);

                // Send the notification to the specific user via SignalR
                await Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Content,
                    notification.CreatedAt,
                    notification.IsRead,
                    notification.Type,
                    notification.Data
                });
            }

            // Save all notifications to the database in one go
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Update unread notifications count for the receiver
            foreach (var userId in userIds)
            {
                await Clients.User(userId).SendAsync("UpdateNotifications");
            }
        }

        // Method to mark a notification as read, could be called from the client
        public async Task MarkNotificationAsRead(string notificationId)
        {
            // Fetch the notification from the database using notificationId
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            // Notify the client (optional, based on your app's needs)
            await Clients.Caller.SendAsync("UpdateNotifications", notificationId);
        }

        // Additional methods as needed...
        public async Task MarkAllNotificationsAsRead(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            await Clients.User(userId).SendAsync("UpdateNotifications");
        }
    }
}
