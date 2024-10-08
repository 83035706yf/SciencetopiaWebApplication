using Sciencetopia.Data;
using Microsoft.EntityFrameworkCore;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDTO>> GetNotificationsForUserAsync(string userId)
    {
        return await _context.Notifications
                             .Where(n => n.UserId == userId)
                             .OrderByDescending(n => n.CreatedAt)
                             .Select(n => new NotificationDTO
                             {
                                 Id = n.Id.ToString(),
                                 Content = n.Content,
                                 CreatedAt = n.CreatedAt.UtcDateTime,  // Convert DateTimeOffset to DateTime
                                 IsRead = n.IsRead,
                                 Type = n.Type
                             })
                             .ToListAsync();
    }
}
