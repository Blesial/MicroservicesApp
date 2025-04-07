using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
	.AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

// By placing the initialization in the ApplicationStarted event, 
// you ensure that the database setup happens only after the application is ready, 
// and it doesn't block the startup process.
app.Lifetime.ApplicationStarted.Register(async () =>
{
	// Initialize the database
	try
	{
		await DbInitializer.InitializeDb(app);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error: {ex.Message}");
	}
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
	=> HttpPolicyExtensions
		.HandleTransientHttpError()
		.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));