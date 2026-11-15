// C# instead of bash scripts: dotnet is already required, so the same commands work on Linux, macOS and Windows without Git Bash.
// Usage: dotnet db/scripts/db.cs migrate | backup | restore <file>

using System.Diagnostics;

string command = args.Length > 0 ? args[0] : "";
// Resolved before moving to the repository root: the backup path is relative to where it was typed.
string file = args.Length > 1 ? Path.GetFullPath(args[1]) : "";

// From the repository root, so the script works from any folder.
string scripts = (string)AppContext.GetData("EntryPointFileDirectoryPath")!;
Directory.SetCurrentDirectory(Path.Combine(scripts, "..", ".."));

if (command == "migrate")
	Migrate();
else if (command == "backup")
	Backup();
else if (command == "restore" && file != "")
	Psql(file);
else
{
	Console.WriteLine("Usage: dotnet db/scripts/db.cs migrate | backup | restore <file>");
	return 1;
}
return 0;

// Not "dotnet ef database update": EF Core only writes the SQL, then psql runs it inside the container
// as the migrations user, so its password is never needed on your PC.
static void Migrate()
{
	// dotnet ef starts Program.cs, which stops without these. The script command never connects, so fake values are enough.
	Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Host=localhost");
	Environment.SetEnvironmentVariable("Cors__FrontendOrigin", "http://localhost");

	string sql = Path.GetTempFileName();
	Run("dotnet", "tool", "restore");
	Run("dotnet", "restore", "be/GameExplorer.Api");
	Run("dotnet", "ef", "migrations", "script", "--idempotent", "--project", "be/GameExplorer.Api", "--output", sql);
	// The API can read the migration history but must not change it.
	File.AppendAllText(sql, "\nREVOKE INSERT, UPDATE, DELETE ON \"__EFMigrationsHistory\" FROM :\"api_user\";\n");

	Psql(sql, "-v api_user=$API_DB_USER");
	File.Delete(sql);
	Console.WriteLine("Migrations applied.");
}

// pg_dump runs inside the container, so you don't need PostgreSQL on your PC. The file is copied out
// only if the dump worked: a failed backup leaves no broken file that looks like a good one.
static void Backup()
{
	string file = $"db/backups/terranova_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
	Directory.CreateDirectory("db/backups");
	Run("docker", "compose", "exec", "-T", "db", "sh", "-c", "pg_dump --clean --if-exists --no-owner -U $POSTGRES_USER -d $POSTGRES_DB -f /tmp/backup.sql");
	Run("docker", "compose", "cp", "db:/tmp/backup.sql", file);
	Console.WriteLine($"Backup created: {file}");
}

// As the migrations user, so the tables belong to it and stay readable by the API user.
// The $VARIABLES exist only in the container, so sh reads them there.
static void Psql(string file, string options = "")
{
	Run("docker", "compose", "cp", file, "db:/tmp/script.sql");
	Run("docker", "compose", "exec", "-T", "db", "sh", "-c", $"psql -v ON_ERROR_STOP=1 {options} -U $APP_DB_USER -d $POSTGRES_DB -f /tmp/script.sql");
}

// Same exit code as the failed command, so make and CI stop too.
static void Run(string program, params string[] arguments)
{
	using Process process = Process.Start(program, arguments);
	process.WaitForExit();
	if (process.ExitCode != 0)
	{
		Console.WriteLine($"Failed: {program} {string.Join(' ', arguments)}");
		Environment.Exit(process.ExitCode);
	}
}
