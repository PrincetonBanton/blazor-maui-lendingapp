using LendingApp.Shared.Models;
using LendingApp.CollectorMobile.ViewModels;
using LendingApp.CollectorMobile.Models; // Required for PaymentEntryModel
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

        var borrower = await http.GetFromJsonAsync<Borrower>($"api/borrowers/{loan.BorrowerId}");
        var ledgerEntries = await http.GetFromJsonAsync<List<Ledger>>($"api/ledgers/loan/{loan.Id}") ?? new();

        ViewModel = new LedgerViewModel
        {
            LoanId = loan.Id,
            BorrowerId = loan.BorrowerId,
            CollectorId = loan.CollectorId,
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

        ViewModel.Schedule = GenerateSchedule(ViewModel, ledgerEntries);
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

    private void OnPayTapped(object sender, EventArgs e)
    {
        if ((sender as Label)?.BindingContext is LedgerRow row)
        {
            ViewModel.CurrentInstallmentLabel = $"Installment #{row.PaymentNumber}";

            // Reinitialize the PaymentEntryModel with default values
            ViewModel.PaymentEntry = new PaymentEntryModel
            {
                LoanId = ViewModel.LoanId,
                CollectorId = ViewModel.CollectorId,
                PaymentDate = DateTime.Now,
                PaymentAmount = row.AmountDue,
                Remarks = ""
            };

            // Force refresh of the binding
            BindingContext = null;
            BindingContext = ViewModel;

            PayModal.IsVisible = true;

        }
    }

    private void OnCancelPayment(object sender, EventArgs e)
    {
        PayModal.IsVisible = false;
    }

    private async void OnSubmitPayment(object sender, EventArgs e)
    {
        var entry = ViewModel.PaymentEntry;
        if (entry.PaymentAmount <= 0)
        {
            await DisplayAlert("Validation", "Please enter a valid payment amount.", "OK");
            return;
        }
        var nextPaymentNumber = ViewModel.Schedule.FirstOrDefault(s => s.DisplayAmountPaid == "-")?.PaymentNumber ?? 1;

        // Create the new ledger record
        var newLedger = new Ledger
        {
            LoanId = entry.LoanId,
            CollectorId = entry.CollectorId,
            PaymentNumber = nextPaymentNumber,
            PaymentDate = entry.PaymentDate,
            PaymentAmount = entry.PaymentAmount,
            Remarks = entry.Remarks
        };

        string json = System.Text.Json.JsonSerializer.Serialize(newLedger, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await DisplayAlert("Preview Ledger Payload", json, "OK");

        // TODO: Send `newLedger` to your backend API via POST here

        PayModal.IsVisible = false;
    }



}
