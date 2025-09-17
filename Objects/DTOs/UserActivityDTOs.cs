namespace Vanigam.CRM.Objects.DTOs
{
    public class UserActivityAnalytics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public double AverageSessionDurationHours { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHoursLogged { get; set; }
        public List<DailyActivitySummary> DailyActivity { get; set; } = new();
    }

    public class DailyActivitySummary
    {
        public DateTime Date { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHours { get; set; }
        public double AverageSessionDuration { get; set; }
    }

    public class UserActivitySummary
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHoursLogged { get; set; }
        public double AverageSessionDurationHours { get; set; }
        public DateTimeOffset? LastLoginTime { get; set; }
        public DateTimeOffset? FirstLoginTime { get; set; }
        public int DaysActive { get; set; }
        public string? MostUsedDevice { get; set; }
        public string? MostUsedLocation { get; set; }
    }
}
