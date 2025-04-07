using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
	public static async Task InitializeDb(WebApplication webApplication)
	{
		// EVERY FUNCTIONALITY THAT COMES FROM MONGODB.ENTITIES IS "STATIC".
		// SO WE DONT NEED TO INSTANTIATE THE MONGODB CONTEXT.
		await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(webApplication.Configuration.GetConnectionString("MongoDbConn")));
		// THIS WILL CREATE A DATABASE CALLED "searchdb" AND A COLLECTION CALLED "items".

		// TO CREATE THE INDEXES FOR THE SEARCH FUNCTIONALITY
		await DB.Index<Item>()
			.Key(i => i.Make, KeyType.Text)
			.Key(i => i.Model, KeyType.Text)
			.Key(i => i.Color, KeyType.Text)
			.CreateAsync();
		
		using var scope = webApplication.Services.CreateScope();

		var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

		var items = await httpClient.GetItemsForSearchDb();

		Console.WriteLine($"Items count returned from Auctions Service: {items.Count}");

		if (items.Count > 0) await DB.SaveAsync(items);
	}
}
