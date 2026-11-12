using System.Net.Mail;
using System.Security.Claims;
using GameExplorer.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Auth;

public record Credentials(string? Email, string? Password);
public record ChangePasswordRequest(string? OldPassword, string? NewPassword);

public static class AuthEndpoints
{
	static readonly string DummyHash = Passwords.Hash("timing-equalizer");

	public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
	{
		var account = app.MapGroup("/api/account");
		account.MapPost("/register", Register).RequireRateLimiting("auth");
		account.MapPost("/login", Login).RequireRateLimiting("auth");
		account.MapPost("/logout", async (HttpContext http) =>
		{
			await http.SignOutAsync();
			return TypedResults.NoContent();
		}).RequireAuthorization();
		account.MapGet("/manage/info", Info).RequireAuthorization();
		account.MapPost("/manage/info", ChangePassword).RequireAuthorization().RequireRateLimiting("auth");
	}

	public static int GetUserId(this ClaimsPrincipal principal) =>
		int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

	static IResult? ValidatePassword(string password)
	{
		if (password.Length is < 8 or > 128)
			return Error("PasswordLength", "From 8 to 128 characters.");
		if (!password.Any(char.IsUpper))
			return Error("PasswordRequiresUpper", "At least one uppercase letter.");
		if (!password.Any(c => !char.IsLetterOrDigit(c)))
			return Error("PasswordRequiresNonAlphanumeric", "At least one special character.");
		return null;
	}

	static IResult Error(string code, string message) =>
		TypedResults.ValidationProblem(new Dictionary<string, string[]> { [code] = [message] });

	static async Task<IResult> Register(Credentials request, AppDbContext db)
	{
		string email = request.Email?.Trim() ?? "";
		string password = request.Password ?? "";
		if (email.Length > 256 || !MailAddress.TryCreate(email, out MailAddress? parsed) || parsed.Address != email)
			return Error("InvalidEmail", "Email not valid.");
		if (ValidatePassword(password) is { } passwordError)
			return passwordError;

		db.Users.Add(new User { Email = email, PasswordHash = Passwords.Hash(password) });
		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
		{
			return Error("DuplicateUserName", "Email already registered.");
		}
		return TypedResults.Ok();
	}

	static async Task<IResult> Login(Credentials request, AppDbContext db, HttpContext http)
	{
		string email = request.Email?.Trim() ?? "";
		User? user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
		bool ok = Passwords.Verify(request.Password ?? "", user?.PasswordHash ?? DummyHash);
		if (user is null || !ok)
			return TypedResults.Problem("Failed", statusCode: StatusCodes.Status401Unauthorized);

		await SignInAsync(http, user);
		return TypedResults.Ok();
	}

	static async Task<IResult> Info(ClaimsPrincipal principal, AppDbContext db)
	{
		User? user = await db.Users.FindAsync(principal.GetUserId());
		return user is null ? TypedResults.NotFound() : TypedResults.Ok(new { user.Email });
	}

	static async Task<IResult> ChangePassword(ChangePasswordRequest request, ClaimsPrincipal principal, AppDbContext db, HttpContext http)
	{
		string oldPassword = request.OldPassword ?? "";
		string newPassword = request.NewPassword ?? "";
		User? user = await db.Users.FindAsync(principal.GetUserId());
		if (user is null)
			return TypedResults.Unauthorized();
		if (!Passwords.Verify(oldPassword, user.PasswordHash))
			return Error("PasswordMismatch", "Old password is wrong.");
		if (ValidatePassword(newPassword) is { } passwordError)
			return passwordError;
		if (newPassword == oldPassword)
			return Error("PasswordSameAsOld", "New password must be different.");

		user.PasswordHash = Passwords.Hash(newPassword);
		user.SessionStamp = Guid.NewGuid();
		await db.SaveChangesAsync();
		await SignInAsync(http, user);
		return TypedResults.Ok(new { user.Email });
	}

	static Task SignInAsync(HttpContext http, User user)
	{
		Claim[] claims = [new(ClaimTypes.NameIdentifier, user.Id.ToString()), new("stamp", user.SessionStamp.ToString())];
		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		return http.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });
	}

	public static async Task ValidateSessionAsync(CookieValidatePrincipalContext context)
	{
		AppDbContext db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
		int id = context.Principal!.GetUserId();
		bool valid = Guid.TryParse(context.Principal!.FindFirst("stamp")?.Value, out Guid stamp)
			&& await db.Users.AnyAsync(u => u.Id == id && u.SessionStamp == stamp);
		if (!valid)
		{
			context.RejectPrincipal();
			await context.HttpContext.SignOutAsync();
		}
	}
}
