using LendingApp.Shared.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace LendingApp.CollectorMobile.Pages;

public partial class LoanListPage : ContentPage
{
    private readonly HttpClient _http = new();
    private readonly string _baseUrl = "https://localhost:7141/api"; // Adjust as needed

    public ObservableCollection<Loan> Loans { get; set; } = new();
    public ObservableCollection<Loan> FilteredLoans { get; set; } = new();
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
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));

            // Load collectors
            var collectors = await _http.GetFromJsonAsync<List<Collector>>($"{_baseUrl}/collectors");
            CollectorIds.Clear();
            foreach (var collector in collectors)
                CollectorIds.Add(collector.Id);

            // Load all loans
            var loans = await _http.GetFromJsonAsync<List<Loan>>($"{_baseUrl}/loans");
            Loans.Clear();
            foreach (var loan in loans)
                Loans.Add(loan);

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

    private void OnCollectorChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker?.SelectedItem is Guid selected)
        {
            SelectedCollectorId = selected;
            ApplyFilter();
        }
    }

    private void ApplyFilter()
    {
        FilteredLoans.Clear();

        var filtered = SelectedCollectorId == null
            ? Loans
            : Loans.Where(l => l.CollectorId == SelectedCollectorId);

        foreach (var loan in filtered)
            FilteredLoans.Add(loan);
    }
}
