using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
	.AddPolicyHandler(GetRetryPolicy());

// Automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// MassTransit
builder.Services.AddMassTransit(x => {
	x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host("localhost", "/", h =>
		{
			h.Username("rabbit");
			h.Password("rabbitpw");
		});

		cfg.ReceiveEndpoint("search-auction-created", e => {
			cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
			e.ConfigureConsumer<AuctionCreatedConsumer>(context);
		});

		cfg.ConfigureEndpoints(context);
	});
});

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