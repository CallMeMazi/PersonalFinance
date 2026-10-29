namespace PersonalFinance.Models;

public class MonthlyGoal
{
    public int Id { get; set; }
    public int PersianYear { get; set; }
    public int PersianMonth { get; set; }
    public decimal TargetHours { get; set; }
}
