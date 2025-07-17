using LendingApp.CollectorMobile.ViewModels;
using LendingApp.Shared.Models;

namespace LendingApp.CollectorMobile.Helpers;

public static class ScheduleGenerator
{
    public static List<LedgerRow> Generate(LedgerViewModel vm, List<Ledger> ledgerEntries)
    {
        var schedule = new List<LedgerRow>();
        DateTime startDate = vm.StartDate ?? DateTime.Today;

        TimeSpan interval = vm.PaymentFrequency switch
        {
            0 => TimeSpan.FromDays(1),   // Daily
            1 => TimeSpan.FromDays(7),   // Weekly
            2 => TimeSpan.FromDays(30),  // Monthly
            _ => TimeSpan.FromDays(30)   // Default to Monthly if unknown
        };

        double runningBalance = vm.TotalAmount;

        for (int i = 1; i <= vm.ExpectedPayments; i++)
        {
            double daysBetweenPayments = interval.TotalDays;                                   // Number of days between payments
            DateTime dueDate = startDate.AddDays(daysBetweenPayments * (i - 1));               // Compute due date

            Ledger? matchingEntry = ledgerEntries.FirstOrDefault(e => e.PaymentNumber == i);   // Get ledger entry for this payment number

            double amountPaid = matchingEntry?.PaymentAmount ?? 0;                             // Get amount paid if exists
            DateTime? paymentDate = matchingEntry?.PaymentDate;                                // Get payment date if exists
            string remarks = matchingEntry?.Remarks ?? "";                                     // Get remarks if exists

            if (matchingEntry != null)
                runningBalance -= amountPaid;                                                  // Subtract paid amount from balance

            schedule.Add(new LedgerRow
            {
                PaymentNumber = i,
                DueDate = dueDate,
                PaymentDate = paymentDate,
                AmountDue = vm.InstallmentAmount,
                AmountPaid = amountPaid,
                RunningBalance = runningBalance,
                Remarks = remarks
            });
        }

        return schedule;
    }
}
