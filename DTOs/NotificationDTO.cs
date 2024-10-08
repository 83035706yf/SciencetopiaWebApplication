public class NotificationDTO
{
    public string? Id { get; set; }  // Change this to string if needed
    public string? Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }  // Change to DateTimeOffset
    public bool IsRead { get; set; }
    public string? Type { get; set; }
}
