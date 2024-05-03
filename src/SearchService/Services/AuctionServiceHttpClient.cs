using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Services;

public class AuctionServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuctionServiceHttpClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration.GetValue<string>("AuctionServiceUrl");
        Console.WriteLine("Armin {0}",_baseUrl);
    }

    public async Task<List<Item>> GetItemsForSearchDatabaseAsync()
    {
        var lastUpdated = await DB.Find<Item, DateTime>()
            .Sort(s => s.Descending(i => i.UpdateAt))
            .Project(i => i.UpdateAt)
            .ExecuteFirstAsync();

        var items = await _httpClient.GetFromJsonAsync<List<Item>>($"{_baseUrl}/Api/Auctions?fromDate={lastUpdated}");

        return items;
    }
}
