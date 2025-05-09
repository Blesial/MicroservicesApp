using AuctionService.Consumers;
using AuctionService.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// GET ASSEMBLIES -> PROVIDES THE LOCATION OF THE ASSEMBLY THAT THIS APP IS RUNNING IN
// gonna look for the classes that derives from the Profile class, and register the mappings in memory.

// MassTransit
builder.Services.AddMassTransit(x => {
	x.AddEntityFrameworkOutbox<AuctionDbContext>(o => {
		o.QueryDelay = TimeSpan.FromSeconds(10);

		o.UsePostgres();

		o.UseBusOutbox(); // Enables the outbox pattern for the bus
	});

	x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host("localhost", "/", h =>
		{
			h.Username("rabbit");
			h.Password("rabbitpw");
		});
		cfg.ConfigureEndpoints(context);
	});
});

// Adding the JwtBearer authentication scheme
// This is used to authenticate users via JWT tokens.
// We are telling how to validate the token.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options => 
		{
			options.Authority = builder.Configuration["IdentityServerUrl"]; // Tells who has issued the token
			options.RequireHttpsMetadata = false; // For development purposes, we are not using HTTPS
			options.TokenValidationParameters.ValidateAudience = false; // We are not validating the audience
			options.TokenValidationParameters.NameClaimType = "username"; // The claim type that contains the name of the user
		}
	);
// Scanning for Consumers/Sagas:
// MassTransit inspects the DI container to discover all your message handlers (consumers and sagas) automatically.
// Auto-Configuring Receive Endpoints:
// It then uses naming conventions to automatically create and configure the messaging endpoints 
// (queues and bindings) for each of these handlers, reducing manual configuration and potential errors.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication(); // is before UseAuthorization 

app.UseAuthorization();

app.MapControllers();

try
{
	DbInitializer.InitDb(app);	
}
catch (Exception e)
{
	Console.WriteLine(e.Message);
}

app.Run();
