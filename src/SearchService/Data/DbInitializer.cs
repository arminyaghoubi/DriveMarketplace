using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;
using System.Data.Common;
using System.Text.Json;

namespace SearchService.Data;

public static class DbInitializer
{
    public static async Task InitDatabase(string connectionString, AuctionServiceHttpClient auctionService)
    {
        await DB.InitAsync("Search", MongoClientSettings.FromConnectionString(connectionString));

        await DB.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        await SeedData(auctionService);
    }

    private static async Task SeedData(AuctionServiceHttpClient auctionService)
    {
        var items = await auctionService.GetItemsForSearchDatabaseAsync();

        if (items.Count > 0)
        {
            await DB.SaveAsync(items);
        }
    }
}
