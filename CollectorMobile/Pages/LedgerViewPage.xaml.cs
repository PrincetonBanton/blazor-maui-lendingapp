using LendingApp.Shared.Models;
using LendingApp.CollectorMobile.ViewModels;
using System.Net.Http.Json;

namespace LendingApp.CollectorMobile.Pages;

public partial class LedgerViewPage : ContentPage
{
    public LedgerViewModel ViewModel { get; set; }

    public LedgerViewPage(Loan loan)
    {
        InitializeComponent();
        LoadDataAsync(loan);
    }

    private async void LoadDataAsync(Loan loan)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7141") // Replace with your actual backend base URL
        };

        // Fetch borrower info
        var borrower = await http.GetFromJsonAsync<Borrower>($"api/borrowers/{loan.BorrowerId}");

        // Fetch ledger entries
        var ledgerEntries = await http.GetFromJsonAsync<List<Ledger>>($"api/ledgers/loan/{loan.Id}") ?? new();

        // Initialize ViewModel
        ViewModel = new LedgerViewModel
        {
            LoanId = loan.Id,
            BorrowerId = loan.BorrowerId,
            PrincipalAmount = loan.PrincipalAmount,
            TotalAmount = loan.TotalAmount,
            RemainingBalance = loan.RemainingBalance,
            InstallmentAmount = loan.InstallmentAmount,
            PaymentsMade = loan.PaymentsMade,
            ExpectedPayments = loan.ExpectedPayments,
            BorrowerName = borrower?.FullName ?? "Unknown",
            ContactNumber = borrower?.ContactNumber ?? "N/A",
            Address = borrower?.Address ?? "N/A",
            PaymentFrequency = (int)loan.PaymentFrequency,
            StartDate = loan.StartDate
        };

        // Generate Ledger Schedule
        ViewModel.Schedule = GenerateSchedule(ViewModel, ledgerEntries);

        // Bind to UI
        BindingContext = ViewModel;
    }

    private List<LedgerRow> GenerateSchedule(LedgerViewModel vm, List<Ledger> ledgerEntries)
    {
        var schedule = new List<LedgerRow>();
        DateTime startDate = vm.StartDate ?? DateTime.Today;
        TimeSpan interval = vm.PaymentFrequency switch
        {
            0 => TimeSpan.FromDays(1),
            1 => TimeSpan.FromDays(7),
            2 => TimeSpan.FromDays(30),
            _ => TimeSpan.FromDays(30)
        };

        double runningBalance = vm.TotalAmount;

        for (int i = 1; i <= vm.ExpectedPayments; i++)
        {
            var dueDate = startDate.AddDays(interval.TotalDays * (i - 1));
            var entry = ledgerEntries.FirstOrDefault(e => e.PaymentNumber == i);

            double amountPaid = entry?.PaymentAmount ?? 0;
            DateTime? paymentDate = entry?.PaymentDate;

            if (entry != null)
                runningBalance -= amountPaid;

            schedule.Add(new LedgerRow
            {
                PaymentNumber = i,
                DueDate = dueDate,
                PaymentDate = paymentDate,
                AmountDue = vm.InstallmentAmount,
                AmountPaid = amountPaid,
                RunningBalance = runningBalance,
                Remarks = entry?.Remarks ?? ""
            });
        }

        return schedule;
    }

}
