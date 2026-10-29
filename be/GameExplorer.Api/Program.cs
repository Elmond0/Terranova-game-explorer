using GameExplorer.Api.Data;
using GameExplorer.Api.Services;
using GameExplorer.Api.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddExceptionHandler<RawgExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"]
	?? throw new InvalidOperationException("CORS configuration is missing.");

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
		policy.WithOrigins(frontendOrigin)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<RawgService>(c =>
{
	c.BaseAddress = new Uri("https://api.rawg.io/api/");
	c.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddOptions<RawgOptions>()
	.Bind(builder.Configuration.GetSection("Rawg"))
	.ValidateDataAnnotations()
	.ValidateOnStart();

// --- Database (Elmond0) ---
// The backend connects as terranova_api (rows only); migrations run with db/scripts/migrate.sh
var connectionString = builder.Configuration.GetConnectionString("Default")
	?? throw new InvalidOperationException("Missing ConnectionStrings:Default (set it with dotnet user-secrets, see db/README.md)");

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

	// Parameter values in logs: development only
	if (builder.Environment.IsDevelopment())
		options.EnableSensitiveDataLogging().EnableDetailedErrors();
});
builder.Services.AddScoped<IGameCacheStore, GameCacheStore>();

// --- Login (ASP.NET Core Identity, cookies: ADR-04) ---
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<AppDbContext>();



var app = builder.Build();

app.UseExceptionHandler();

app.MapHealthChecks("/health");

app.UseCors("Frontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// --- Login endpoints ---
var account = app.MapGroup("/api/account");
account.MapIdentityApi<IdentityUser>();
account.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
{
	await signInManager.SignOutAsync();
	return TypedResults.NoContent();
}).RequireAuthorization();

app.Run();
