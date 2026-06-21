using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AccountingProject.Tests;

public sealed class MigrationScriptIntegrityTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string MigrationsDir = Path.Combine(RepoRoot, "server", "Migrations");
    private static readonly string ServerProject = Path.Combine(RepoRoot, "server", "AccountingProject.csproj");

    private static readonly string[] RequiredSchemaMarkers =
    [
        "סוג_מוסד",
        "מקטע_הורה_שעות_נוספות",
        "IX_עובדים_מזהה_מעסיק_תז",
        "20260621115616_RepairMissingProductionSchema",
        "WHERE [IsDeleted] = 0",
    ];

    [Fact]
    public void MigrationFiles_AllHaveDesignerCompanion()
    {
        var orphans = Directory
            .GetFiles(MigrationsDir, "*.cs")
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !Path.GetFileName(path).Equals("PayrollDbContextModelSnapshot.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !File.Exists(path.Replace(".cs", ".Designer.cs", StringComparison.OrdinalIgnoreCase)))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(orphans);
    }

    [Fact]
    public void GeneratedIdempotentScript_IncludesAllDesignerMigrationsAndRequiredSchema()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"migration-{Guid.NewGuid():N}.sql");
        try
        {
            RunDotNetEfScript(scriptPath);
            var sql = File.ReadAllText(scriptPath);

            foreach (var marker in RequiredSchemaMarkers)
            {
                Assert.Contains(marker, sql);
            }

            var migrationIds = Directory
                .GetFiles(MigrationsDir, "*.Designer.cs")
                .Select(ReadMigrationId)
                .Where(id => id != null)
                .Cast<string>()
                .Distinct()
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            Assert.NotEmpty(migrationIds);
            foreach (var id in migrationIds)
            {
                Assert.Contains($"MigrationId] = N'{id}'", sql);
            }
        }
        finally
        {
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    private static void RunDotNetEfScript(string outputPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments =
                $"ef migrations script --project \"{ServerProject}\" --startup-project \"{ServerProject}\" " +
                $"--configuration Release --idempotent -o \"{outputPath}\"",
            WorkingDirectory = Path.Combine(RepoRoot, "server"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet ef migrations script.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.True(process.ExitCode == 0,
            $"dotnet ef migrations script failed.{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
        Assert.True(File.Exists(outputPath), "Expected migration script output file was not created.");
    }

    private static string? ReadMigrationId(string designerPath)
    {
        var content = File.ReadAllText(designerPath);
        var match = Regex.Match(content, @"\[Migration\(""([^""]+)""\)\]");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AccountingProject.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
