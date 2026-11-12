using SutilBox.CLI.Models;

namespace SutilBox.CLI.Core;

public static class ScanOutput
{
    public static void DisplayResult(ScanResult result)
    {
        // Health score
        Console.WriteLine("┌────────────────────────────────────────────────────┐");
        Console.WriteLine("│              RESULTADO DEL ESCANEO                 │");
        Console.WriteLine("└────────────────────────────────────────────────────┘");
        Console.WriteLine();

        DisplayHealthScore(result.HealthScore);
        Console.WriteLine();

        // Timestamp
        Console.WriteLine($"📅 Escaneo completado: {result.Timestamp:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine();

        // Category summary
        if (result.CategorySummary.Count > 0)
        {
            Console.WriteLine("📊 Problemas por categoría:");
            foreach (var (category, count) in result.CategorySummary)
            {
                var icon = CategoryToIcon(category);
                Console.WriteLine($"   {icon} {category,-15} {count} problema(s)");
            }
            Console.WriteLine();
        }

        // Group by severity
        var critical = result.Issues.Where(i => i.Severity == IssueSeverity.Critical).ToList();
        var warnings = result.Issues.Where(i => i.Severity == IssueSeverity.Warning).ToList();
        var infos = result.Issues.Where(i => i.Severity == IssueSeverity.Info).ToList();

        if (critical.Count > 0)
        {
            DisplayIssues("🔴 CRÍTICOS", critical, ConsoleColor.Red);
            Console.WriteLine();
        }

        if (warnings.Count > 0)
        {
            DisplayIssues("🟡 ADVERTENCIAS", warnings, ConsoleColor.Yellow);
            Console.WriteLine();
        }

        if (infos.Count > 0)
        {
            DisplayIssues("ℹ️  INFORMACIÓN", infos, ConsoleColor.Cyan);
            Console.WriteLine();
        }

        if (result.Issues.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ ¡No se detectaron problemas! Tu sistema está saludable.");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Summary
        var solvable = result.Issues.Count(i => i.IsSolvable);
        Console.WriteLine($"Total: {result.Issues.Count} problema(s) detectado(s), {solvable} solucionable(s).");

        if (solvable > 0)
        {
            Console.WriteLine();
            Console.WriteLine("💡 Usa 'sbox scan --expert' para solucionar problemas interactivamente.");
        }
    }

    static void DisplayHealthScore(int score)
    {
        var color = score >= 80 ? ConsoleColor.Green : score >= 50 ? ConsoleColor.Yellow : ConsoleColor.Red;
        var emoji = score >= 80 ? "🟢" : score >= 50 ? "🟡" : "🔴";
        var label = score >= 80 ? "Saludable" : score >= 50 ? "Necesita atención" : "Crítico";

        Console.ForegroundColor = color;
        Console.Write($"  {emoji} Score de salud: {score}/100 ");
        Console.Write($"({label})");
        Console.ResetColor();
        Console.WriteLine();

        // Visual bar
        var barLength = 30;
        var filled = (int)(score / 100.0 * barLength);
        Console.Write("  [");
        Console.ForegroundColor = color;
        for (var i = 0; i < filled; i++) Console.Write("█");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        for (var i = filled; i < barLength; i++) Console.Write("░");
        Console.ResetColor();
        Console.WriteLine("]");
    }

    static void DisplayIssues(string header, List<ScanIssue> issues, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"  {header} ({issues.Count})");
        Console.ResetColor();
        Console.WriteLine();

        for (var i = 0; i < issues.Count; i++)
        {
            var issue = issues[i];
            Console.WriteLine($"    {i + 1}. {issue.Title}");
            Console.WriteLine($"       {issue.Description}");

            if (!string.IsNullOrEmpty(issue.Solution))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"       💊 Solución: {issue.Solution}");
                Console.ResetColor();
            }

            if (i < issues.Count - 1)
                Console.WriteLine();
        }
    }

    static string CategoryToIcon(ScanCategory category)
    {
        return category switch
        {
            ScanCategory.Hardware => "🖥️",
            ScanCategory.Drivers => "🔧",
            ScanCategory.Disco => "💾",
            ScanCategory.Rendimiento => "⚡",
            ScanCategory.Seguridad => "🔒",
            ScanCategory.Privacidad => "👁️",
            ScanCategory.Inicio => "🚀",
            ScanCategory.Sistema => "⚙️",
            _ => "📋"
        };
    }
}
