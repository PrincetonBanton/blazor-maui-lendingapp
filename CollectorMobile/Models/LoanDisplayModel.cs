public class LoanDisplayModel
{
    public Guid Id { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public double InstallmentAmount { get; set; }
    public int PaymentsMade { get; set; }
    public int ExpectedPayments { get; set; }
    public double RemainingBalance { get; set; }

    public string PaymentProgress => $"{PaymentsMade}/{ExpectedPayments}";
}
