namespace PersonalFinance.Models;

public class Expense
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; set; }
    public int CategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = null!;
    public string? Description { get; set; }
}
