using System;
using System.Collections.Generic;

namespace LendingApp.CollectorMobile.ViewModels
{
    public class LedgerViewModel
    {
        public Guid LoanId { get; set; }
        public Guid BorrowerId { get; set; }

        public double PrincipalAmount { get; set; } 
        public double TotalAmount { get; set; }
        public double RemainingBalance { get; set; }
        public double InstallmentAmount { get; set; }
        public int PaymentsMade { get; set; }
        public int ExpectedPayments { get; set; }
        public DateTime? StartDate { get; set; }

        public int PaymentFrequency { get; set; }
        public string PaymentMode => PaymentFrequency switch
        {
            0 => "Daily",
            1 => "Weekly",
            2 => "Monthly",
            _ => "Unknown"
        };

        public string PaymentProgress => $"{PaymentsMade}/{ExpectedPayments}";

        public string BorrowerName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public List<LedgerRow> Schedule { get; set; } = new();
    }

    public class LedgerRow
    {
        public int PaymentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double AmountDue { get; set; }
        public double AmountPaid { get; set; }
        public double RunningBalance { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public string Status => PaymentDate.HasValue ? "Paid" : "Pay";

        public string DisplayPaymentDate => PaymentDate.HasValue ? PaymentDate.Value.ToString("MM/dd/yyyy") : "-";
        public string DisplayAmountPaid => AmountPaid > 0 ? AmountPaid.ToString("N2") : "-";

    }
}
