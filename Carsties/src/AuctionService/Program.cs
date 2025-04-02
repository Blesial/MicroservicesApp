using AuctionService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// GET ASSEMBLIES -> PROVIDES THE LOCATION OF THE ASSEMBLY THAT THIS APP IS RUNNING IN
// gonna look for the classes that derives from the Profile class, and register the mappings in memory.

var app = builder.Build();

// Configure the HTTP request pipeline.

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
