using LendingApp.Shared.Models;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BorrowerId { get; set; }
    public Guid? CollectorId { get; set; }
    public double PrincipalAmount { get; set; }
    public double TotalAmount { get; set; }
    public double RemainingBalance { get; set; }

    public DateTime ApplicationDate { get; set; } = DateTime.Now;
    public DateTime? ApprovalDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    public int TermInMonths { get; set; }
    public double MonthlyInterestRate { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;

    public int ExpectedPayments { get; set; }
    public double InstallmentAmount { get; set; }
    public int PaymentsMade { get; set; } = 0;
}
