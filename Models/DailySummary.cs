public class DailySummary
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalUsers { get; set; }
    public int LoggedInUsers { get; set; }
    public int TotalVisits { get; set; }
    public int DailyVisits { get; set; }
    public int NewUsers { get; set; }
    public int ReturningUsers { get; set; }
    public int TotalStudyGroups { get; set; }
    public int WeeklyActiveStudyGroups { get; set; }
    public int TotalKnowledgeNodes { get; set; }
    public int TotalKnowledgeNodeViews { get; set; }
}
