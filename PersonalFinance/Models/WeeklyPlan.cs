namespace PersonalFinance.Models;

public class WeeklyPlan
{
    public int Id { get; set; }
    public DateTime WeekStart { get; set; }
    public decimal TargetHours { get; set; }
}
