using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sciencetopia.Data;
using Sciencetopia.Services;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;

    public MessageController(ApplicationDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }
    // POST: api/Message
    [HttpPost("SendMessage")]
    public async Task<ActionResult<Message>> PostMessage(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetMessage", new { id = message.Id }, message);
    }

    // Additional methods to retrieve messages...
    [HttpGet("GetMessages")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
    {
        return await _context.Messages.ToListAsync();
    }

    [HttpGet("GetGroupedMessagesByUser/{userId}")]
    public async Task<ActionResult<IEnumerable<GroupedMessageDTO>>> GetGroupedMessagesByUser(string userId)
    {
        // Step 1: Fetch the conversations with sorted messages
        var groupedMessages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.ConversationId)
            .Select(group => new
            {
                ConversationId = group.Key,
                PartnerId = group.Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId).FirstOrDefault(),
                PartnerName = group.Select(m => m.SenderId == userId ? m.Receiver.UserName : m.Sender.UserName).FirstOrDefault(),
                UnreadMessageCount = group.Count(m => !m.IsRead && m.ReceiverId == userId),
                Messages = group.OrderBy(m => m.SentTime)
                                .Select(m => new MessageWithUserDetailsDTO
                                {
                                    Id = m.Id,
                                    Content = m.Content,
                                    SentTime = m.SentTime,
                                    Sender = new UserDetailsDTO
                                    {
                                        Id = m.Sender.Id,
                                        UserName = m.Sender.UserName,
                                    },
                                    IsRead = m.IsRead
                                }).ToList(),
                LastMessageSentTime = group.Max(m => m.SentTime) // Fetch the latest sent time for sorting
            })
            .OrderByDescending(group => group.LastMessageSentTime) // Sort by the latest sent time
            .ToListAsync();

        // Step 2: Transform the anonymous type to GroupedMessageDTO and fetch avatar URLs
        var result = new List<GroupedMessageDTO>();
        foreach (var group in groupedMessages)
        {
            var groupedMessageDto = new GroupedMessageDTO
            {
                ConversationId = group.ConversationId,
                PartnerId = group.PartnerId,
                PartnerName = group.PartnerName,
                UnreadMessageCount = group.UnreadMessageCount,
                Messages = group.Messages,
            };

            groupedMessageDto.PartnerAvatarUrl = await _userService.FetchUserAvatarUrlByIdAsync(group.PartnerId);

            foreach (var message in groupedMessageDto.Messages)
            {
                if (message.Sender != null)
                {
                    var avatarUrl = await _userService.FetchUserAvatarUrlByIdAsync(message.Sender.Id!);
                    message.Sender.AvatarUrl = avatarUrl;
                }
            }

            result.Add(groupedMessageDto);
        }

        return Ok(result);
    }

    [HttpGet("GetConversation/{conversationId}")]
    public async Task<ActionResult<GroupedMessageDTO>> GetConversation(string conversationId)
    {
        // Step 1: Retrieve the conversation and related messages
        var conversationGroup = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentTime)
            .ToListAsync();

        // Step 2: If no messages are found, return an empty conversation structure
        if (!conversationGroup.Any())
        {
            return Ok(new GroupedMessageDTO
            {
                ConversationId = conversationId,
                PartnerId = null,
                PartnerName = "Unknown", // Placeholder, adjust if you have specific data requirements
                UnreadMessageCount = 0,
                Messages = new List<MessageWithUserDetailsDTO>()
            });
        }

        // Step 3: Identify the partner user (assuming it's either the sender or receiver)
        var partnerMessage = conversationGroup.FirstOrDefault();
        var partnerId = partnerMessage.SenderId == conversationGroup.First().ReceiverId
            ? partnerMessage.SenderId
            : partnerMessage.ReceiverId;

        // Fetch partner details
        var partnerName = partnerId != null ? await _userService.GetUserNameByIdAsync(partnerId) : "Unknown";
        var partnerAvatarUrl = partnerId != null ? await _userService.FetchUserAvatarUrlByIdAsync(partnerId) : null;

        // Step 4: Populate the DTO with message details
        var conversationDto = new GroupedMessageDTO
        {
            ConversationId = conversationId,
            PartnerId = partnerId,
            PartnerName = partnerName,
            PartnerAvatarUrl = partnerAvatarUrl,
            UnreadMessageCount = conversationGroup.Count(m => !m.IsRead && m.ReceiverId == partnerMessage.ReceiverId),
            Messages = conversationGroup.Select(m => new MessageWithUserDetailsDTO
            {
                Id = m.Id,
                Content = m.Content,
                SentTime = m.SentTime,
                Sender = new UserDetailsDTO
                {
                    Id = m.Sender.Id,
                    UserName = m.Sender.UserName,
                    AvatarUrl = m.Sender.AvatarUrl
                },
                IsRead = m.IsRead
            }).ToList()
        };

        return Ok(conversationDto);
    }

    [HttpPost("MarkAsRead")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == request.ConversationId && m.ReceiverId == request.UserId && !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Messages marked as read" });
    }
}