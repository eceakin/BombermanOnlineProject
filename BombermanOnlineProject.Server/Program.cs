

using BombermanOnlineProject.Server.Data.Context;
using BombermanOnlineProject.Server.Data.Repositories;
using BombermanOnlineProject.Server.Data.UnitOfWork;
using BombermanOnlineProject.Server.Hubs;
using BombermanOnlineProject.Server.Presenters;
using BombermanOnlineProject.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BombermanDbContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
		?? "Host=localhost;Port=5433;Database=BombermanDb;Username=postgres;Password=12345";

	options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameStatisticsRepository, GameStatisticsRepository>();

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<LeaderboardService>();

builder.Services.AddSingleton<LobbyPresenter>();
builder.Services.AddScoped<GamePresenter>();
builder.Services.AddScoped<LeaderboardPresenter>();

builder.Services.AddSignalR(options =>
{
	options.EnableDetailedErrors = true;
	options.MaximumReceiveMessageSize = 102400;
	options.StreamBufferCapacity = 10;
	options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
	options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins(
			"http://localhost:3000",
			"http://localhost:5173",
			"http://localhost:5174"
		)
		.AllowAnyMethod()
		.AllowAnyHeader()
		.AllowCredentials();
	});
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<BombermanDbContext>();
		context.Database.EnsureCreated();
		Console.WriteLine("[Program] Database initialized successfully");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[Program] Error initializing database: {ex.Message}");
	}
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/gamehub");

Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║     BOMBERMAN ONLINE - SERVER STARTED                    ║
║                                                           ║
║     SignalR Hub: /gamehub                                ║
║     Environment: " + app.Environment.EnvironmentName.PadRight(40) + @"║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
");

app.Run();