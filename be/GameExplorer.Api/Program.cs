using GameExplorer.Api.Data;
using GameExplorer.Api.Services;
using GameExplorer.Api.Errors;
using GameExplorer.Api.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddExceptionHandler<IgdbExceptionHandler>();
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

builder.Services.AddHybridCache();

builder.Services.AddOptions<IgdbOptions>()
	.Bind(builder.Configuration.GetSection("Igdb"))
	.ValidateDataAnnotations()
	.ValidateOnStart();

builder.Services.AddHttpClient<TwitchTokenProvider>(c =>
	c.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));

builder.Services.AddTransient<IgdbAuthHandler>();

// One limiter for the whole app: IGDB accepts 4 requests per second
builder.Services.AddKeyedSingleton<RateLimiter>(IgdbRateLimitHandler.LimiterKey, (_, _) =>
	new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
	{
		TokenLimit = 4,
		TokensPerPeriod = 4,
		ReplenishmentPeriod = TimeSpan.FromSeconds(1),
		QueueLimit = 100,
		QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
		AutoReplenishment = true,
	}));
builder.Services.AddTransient<IgdbRateLimitHandler>();

builder.Services.AddScoped<GameService>();

var igdbHttp = builder.Services.AddHttpClient<IgdbClient>(c =>
	{
		c.BaseAddress = new Uri("https://api.igdb.com/v4/");
		c.Timeout = Timeout.InfiniteTimeSpan; // TotalRequestTimeout below handles timeouts now
	})
	.AddHttpMessageHandler<IgdbAuthHandler>();

igdbHttp.AddStandardResilienceHandler(options =>
{
	options.Retry.MaxRetryAttempts = 1;
	options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
	options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(12);
});

igdbHttp.AddHttpMessageHandler<IgdbRateLimitHandler>(); // last = innermost, so retries are limited too

// --- Database (Elmond0) ---
// The connection string comes from config, not from code, so the password never ends up in git; without it the API stops at startup.
// Snake case because PostgreSQL folds unquoted names to lowercase. Logging of query values only in development: they can contain emails and password hashes.

var connectionString = builder.Configuration.GetConnectionString("Default")
	?? throw new InvalidOperationException("Missing ConnectionStrings:Default (start the API with `make dev`, or export the variables described in db/README.md#backend-secrets)");

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

	if (builder.Environment.IsDevelopment())
		options.EnableSensitiveDataLogging().EnableDetailedErrors();
});

// --- Login (Elmond0) ---
// Each request checks the session stamp in the database, so changing the stamp logs the user out everywhere.

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(o =>
	{
		o.Cookie.Name = "terranova.auth";
		o.Events.OnValidatePrincipal = AuthEndpoints.ValidateSessionAsync;
		o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
		o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
	});
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(o =>
{
	o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	o.AddPolicy("auth", http => RateLimitPartition.GetFixedWindowLimiter(
		http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await db.Users.AnyAsync();

	var games = scope.ServiceProvider.GetRequiredService<GameService>();
	try
	{
		await Task.WhenAll(
			games.GetHomeTopRatedAsync(default),
			games.GetHomeRecentAsync(default),
			games.GetHomeUpcomingAsync(default),
			games.GetPlatformHighlightsAsync(167, default), // PlayStation
			games.GetPlatformHighlightsAsync(169, default), // Xbox
			games.GetPlatformHighlightsAsync(130, default), // Nintendo
			games.GetPlatformHighlightsAsync(6, default));  // PC
	}
	catch (IgdbException e)
	{
		// IGDB down at startup is not a reason to stop the API: the home will load on first request
		app.Logger.LogWarning(e, "Home cache warm-up failed");
	}
}

app.UseExceptionHandler();

app.MapHealthChecks("/health");

app.UseCors("Frontend");

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// (Elmond0) Authentication before authorization: the cookie must be read before checking who can access what.
// Rate limiter after routing, so it can read the "auth" policy of each endpoint. Login and favorites endpoints
// live in Auth/ and Data/ and are mapped with one call each, so this file stays short.

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.MapAuthEndpoints();
app.MapMeEndpoints();
app.Run();

