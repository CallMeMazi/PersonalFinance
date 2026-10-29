using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinance.Models;

public class WorkLog
{
    public int Id { get; set; }
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public decimal HourlyRateUsed { get; set; }

    [NotMapped]
    public decimal Income => Hours * HourlyRateUsed;
}
