public class VisitLog
{
    public int Id { get; set; }
    public string? UserId { get; set; } // Nullable for non-logged in users
    public bool IsLoggedIn { get; set; }
    public DateTime VisitTime { get; set; }
    public string? IpAddress { get; set; }  // Store visitor's IP address
    public DateTime SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; } // Nullable until session ends
}
