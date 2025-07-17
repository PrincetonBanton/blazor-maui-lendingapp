using LendingApp.Shared.Models;
using System.Net.Http.Json;

namespace LendingApp.CollectorMobile.Services;

public class LoanService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "https://localhost:7141/api";

    public LoanService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<List<Collector>> GetCollectorsAsync()
    {
        return await _http.GetFromJsonAsync<List<Collector>>($"{_baseUrl}/collectors") ?? new();
    }

    public async Task<List<Loan>> GetLoansAsync()
    {
        return await _http.GetFromJsonAsync<List<Loan>>($"{_baseUrl}/loans") ?? new();
    }
    public async Task<bool> UpdateLoanPaymentStatsAsync(Loan loan)
    {
        var response = await _http.PutAsJsonAsync($"api/loans/{loan.Id}/payment-update", loan);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Borrower>> GetBorrowersAsync()
    {
        return await _http.GetFromJsonAsync<List<Borrower>>($"{_baseUrl}/borrowers") ?? new();
    }
}
