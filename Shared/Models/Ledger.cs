using LendingApp.Shared.Models;

public class Ledger
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanId { get; set; }
    public Guid CollectorId { get; set; } 
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public double PaymentAmount { get; set; }
    public int PaymentNumber { get; set; }      // 1 of 30, 2 of 30, etc.
    public string Remarks { get; set; } = string.Empty;

    //public static implicit operator Ledger(Ledger v)
    //{
    //    throw new NotImplementedException();
    //}
}


