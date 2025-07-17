using LendingApp.Shared.Models;
using LendingApp.CollectorMobile.Services;
using LendingApp.CollectorMobile.ViewModels;
using System.Collections.ObjectModel;

namespace LendingApp.CollectorMobile.Pages;

public partial class LoanListPage : ContentPage
{
    private readonly LoanService _loanService = new(new HttpClient()); // Manual instantiation
    private List<Loan>? AllLoans;
    private List<Borrower>? AllBorrowers;

    public ObservableCollection<LoanListModel> FilteredLoans { get; set; } = new();
    public ObservableCollection<Guid> CollectorIds { get; set; } = new();
    public Guid? SelectedCollectorId { get; set; } = null;
    public bool IsLoading { get; set; }

    public LoanListPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            SetLoading(true);

            CollectorIds.Clear();
            foreach (var c in await _loanService.GetCollectorsAsync())
                CollectorIds.Add(c.Id);

            AllLoans = await _loanService.GetLoansAsync();
            AllBorrowers = await _loanService.GetBorrowersAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
        OnPropertyChanged(nameof(IsLoading));
    }

    private void ApplyFilter()
    {
        if (AllLoans is null || AllBorrowers is null) return;

        FilteredLoans.Clear();

        var filtered = SelectedCollectorId == null
            ? AllLoans
            : AllLoans.Where(l => l.CollectorId == SelectedCollectorId);

        foreach (var loan in filtered)
        {
            var borrowerName = AllBorrowers.FirstOrDefault(b => b.Id == loan.BorrowerId)?.FullName ?? "Unknown";

            FilteredLoans.Add(new LoanListModel
            {
                Id = loan.Id,
                BorrowerName = borrowerName,
                TotalAmount = loan.TotalAmount,
                InstallmentAmount = loan.InstallmentAmount,
                PaymentsMade = loan.PaymentsMade,
                ExpectedPayments = loan.ExpectedPayments,
                RemainingBalance = loan.RemainingBalance
            });
        }
    }

    private void OnCollectorChanged(object sender, EventArgs e)
    {
        if (sender is Picker { SelectedItem: Guid selected })
        {
            SelectedCollectorId = selected;
            ApplyFilter();
        }
    }

    private async void OnViewLedgerTapped(object sender, EventArgs e)
    {
        if (sender is Label { BindingContext: LoanListModel selectedLoan })
        {
            var fullLoan = AllLoans?.FirstOrDefault(l => l.Id == selectedLoan.Id);
            if (fullLoan != null)
                await Navigation.PushAsync(new LedgerViewPage(fullLoan));
        }
    }
}
