using LendingApp.Shared.Models;
using System.Net.Http.Json;

namespace LendingApp.CollectorMobile.Services;

public class LedgerService
{
    private readonly HttpClient _http;

    public LedgerService()
    {
        _http = new HttpClient { BaseAddress = new Uri("https://localhost:7141") };
    }

    public async Task<bool> PostLedgerAsync(Ledger ledger)
    {
        var response = await _http.PostAsJsonAsync("api/ledgers", ledger);
        return response.IsSuccessStatusCode;
    }
}
