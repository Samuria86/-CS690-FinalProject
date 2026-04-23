namespace Library.Models
{
    public class ReadingGoal
    {
        public int Id { get; set; }
        public int? DailyPageGoal { get; set; }
        public int? WeeklyPageGoal { get; set; }
        public int? MonthlyBookGoal { get; set; }
        public int? YearlyBookGoal { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }
}
