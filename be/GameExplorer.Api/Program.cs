using GameExplorer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

var app = builder.Build();

app.UseCors("Frontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
