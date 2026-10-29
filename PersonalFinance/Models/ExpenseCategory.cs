namespace PersonalFinance.Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = "#7c6cff";
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public decimal AllocationPercent { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
