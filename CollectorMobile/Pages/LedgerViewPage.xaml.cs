using System.Net.Http.Json;
using LendingApp.Shared.Models;
using LendingApp.CollectorMobile.ViewModels;
using LendingApp.CollectorMobile.Models; // Required for PaymentEntryModel
using LendingApp.CollectorMobile.Helpers;

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
        var http = new HttpClient{ BaseAddress = new Uri("https://localhost:7141") };
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

        ViewModel.Schedule = ScheduleGenerator.Generate(ViewModel, ledgerEntries);
        BindingContext = ViewModel;
    }

    private void OnPayTapped(object sender, EventArgs e)
    {
        if ((sender as Label)?.BindingContext is LedgerRow row)
        {
            ViewModel.CurrentInstallmentLabel = $"Installment #{row.PaymentNumber}";
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

        var nextUnpaid = ViewModel.Schedule.FirstOrDefault(s => s.AmountPaid == 0);
        var nextPaymentNumber = nextUnpaid?.PaymentNumber ?? -1;
        if (nextPaymentNumber == -1)
        {
            await DisplayAlert("Notice", "All payments have been completed.", "OK");
            return;
        }

        var newLedger = new Ledger
        {
            LoanId = entry.LoanId,
            CollectorId = entry.CollectorId,
            PaymentNumber = nextPaymentNumber,
            PaymentDate = entry.PaymentDate,
            PaymentAmount = entry.PaymentAmount,
            Remarks = entry.Remarks
        };

        var http = new HttpClient { BaseAddress = new Uri("https://localhost:7141") };

        var response = await http.PostAsJsonAsync("api/ledgers", newLedger);
        if (!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Error", "Failed to post ledger entry.", "OK");
            return;
        }

        // Update loan stats
        var loan = ViewModel.ToLoan(); // convert ViewModel to Loan object
        loan.RemainingBalance -= entry.PaymentAmount;
        loan.PaymentsMade += 1;

        var updateResponse = await http.PutAsJsonAsync($"api/loans/{loan.Id}/payment-update", loan);
        if (!updateResponse.IsSuccessStatusCode)
        {
            await DisplayAlert("Error", "Failed to update loan stats.", "OK");
            return;
        }

        await DisplayAlert("Success", "Payment recorded successfully.", "OK");

        PayModal.IsVisible = false;

        var updatedLoan = await http.GetFromJsonAsync<Loan>($"api/loans/{loan.Id}");
        if (updatedLoan != null)
        {
            LoadDataAsync(updatedLoan); 
        }
        else
        {
            await DisplayAlert("Error", "Failed to refresh loan data.", "OK");
        }
    }

}
