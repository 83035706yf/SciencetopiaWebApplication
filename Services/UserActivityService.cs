using Sciencetopia.Data;
using Microsoft.EntityFrameworkCore;

public class UserActivityService
{
    private readonly ApplicationDbContext _context;

    public UserActivityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogVisit(string? userId, string ipAddress)
    {
        // Define a time limit (e.g., 10 minutes)
        var timeLimit = DateTime.UtcNow.AddMinutes(-60);

        // Check if there is a recent visit from the same IP within the time limit
        var recentVisit = await _context.VisitLogs
            .Where(v => v.IpAddress == ipAddress && v.VisitTime >= timeLimit)
            .FirstOrDefaultAsync();

        if (recentVisit == null)
        {
            // Log the visit only if no recent visit is found
            var visit = new VisitLog
            {
                UserId = userId,
                IsLoggedIn = !string.IsNullOrEmpty(userId),
                VisitTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                SessionStartTime = DateTime.UtcNow
            };

            _context.VisitLogs.Add(visit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task LogSessionEnd(int visitId)
    {
        var visit = await _context.VisitLogs.FindAsync(visitId);
        if (visit != null)
        {
            visit.SessionEndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // // Get total number of visits
    // public async Task<int> GetTotalVisits()
    // {
    //     return await _context.VisitLogs.CountAsync();
    // }

    // // Get number of visits from logged-in users
    // public async Task<int> GetLoggedInUserVisits()
    // {
    //     return await _context.VisitLogs.CountAsync(v => v.IsLoggedIn);
    // }
}
