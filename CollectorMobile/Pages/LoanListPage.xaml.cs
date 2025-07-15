using LendingApp.Shared.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using LendingApp.CollectorMobile.ViewModels;

namespace LendingApp.CollectorMobile.Pages;

public partial class LoanListPage : ContentPage
{
    private readonly HttpClient _http = new();
    private readonly string _baseUrl = "https://localhost:7141/api";

    public ObservableCollection<LoanListModel> FilteredLoans { get; set; } = new();
    public ObservableCollection<Guid> CollectorIds { get; set; } = new();

    private List<Loan>? AllLoans;
    private List<Borrower>? AllBorrowers;

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
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));

            var collectors = await _http.GetFromJsonAsync<List<Collector>>($"{_baseUrl}/collectors");
            CollectorIds.Clear();
            foreach (var c in collectors)
                CollectorIds.Add(c.Id);

            AllLoans = await _http.GetFromJsonAsync<List<Loan>>($"{_baseUrl}/loans");
            AllBorrowers = await _http.GetFromJsonAsync<List<Borrower>>($"{_baseUrl}/borrowers");

            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    private void ApplyFilter()
    {
        FilteredLoans.Clear();
        if (AllLoans == null || AllBorrowers == null) return;

        var filtered = SelectedCollectorId == null
            ? AllLoans
            : AllLoans.Where(l => l.CollectorId == SelectedCollectorId);

        foreach (var loan in filtered)
        {
            var borrower = AllBorrowers.FirstOrDefault(b => b.Id == loan.BorrowerId);

            FilteredLoans.Add(new LoanListModel
            {
                Id = loan.Id,
                BorrowerName = borrower?.FullName ?? "Unknown",
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
        var picker = sender as Picker;
        if (picker?.SelectedItem is Guid selected)
        {
            SelectedCollectorId = selected;
            ApplyFilter();
        }
    }

    private async void OnViewLedgerTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.BindingContext is LoanListModel selectedLoan)
        {
            var fullLoan = AllLoans.FirstOrDefault(l => l.Id == selectedLoan.Id);
            if (fullLoan != null)
            {
                await Navigation.PushAsync(new LedgerViewPage(fullLoan));
            }
        }
    }

}
