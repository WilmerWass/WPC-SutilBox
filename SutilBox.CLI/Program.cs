using SutilBox.CLI.Core;
using SutilBox.CLI.Models;

namespace SutilBox.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var command = args.Length > 0 ? args[0].ToLower() : "help";
        var isExpert = args.Contains("--expert") || args.Contains("-e");

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║          SutilBox CLI - Sistema de              ║");
        Console.WriteLine("║          Diagnóstico y Optimización              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            switch (command)
            {
                case "scan":
                    await RunScanAsync(isExpert);
                    break;
                case "info":
                    await RunInfoAsync();
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"Comando desconocido: {command}");
                    Console.WriteLine();
                    ShowHelp();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    static async Task RunScanAsync(bool isExpert)
    {
        var engine = DiagnosticEngineFactory.Create();
        Console.WriteLine($"🔍 Iniciando escaneo completo en {engine.PlatformName}...");
        Console.WriteLine();

        var result = await engine.RunScanAsync();

        ScanOutput.DisplayResult(result);

        if (isExpert || !result.Issues.Any(i => i.IsSolvable))
        {
            if (isExpert && result.Issues.Any(i => i.IsSolvable))
            {
                Console.WriteLine();
                await ExpertMode.InteractiveLoop(result, engine);
            }
        }
        else if (result.Issues.Any(i => i.IsSolvable))
        {
            Console.WriteLine();
            Console.Write("¿Deseas entrar en modo experto para solucionar problemas? (s/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            if (response == "s" || response == "si" || response == "y" || response == "yes")
            {
                Console.WriteLine();
                await ExpertMode.InteractiveLoop(result, engine);
            }
        }
    }

    static async Task RunInfoAsync()
    {
        var engine = DiagnosticEngineFactory.Create();
        var info = await engine.GetSystemInfoAsync();
        InfoOutput.Display(info);
    }

    static void ShowHelp()
    {
        Console.WriteLine("Uso: sbox <comando> [opciones]");
        Console.WriteLine();
        Console.WriteLine("Comandos:");
        Console.WriteLine("  scan        Ejecuta un escaneo completo del sistema");
        Console.WriteLine("  info        Muestra información del sistema (SO, CPU, RAM, discos)");
        Console.WriteLine("  help        Muestra esta ayuda");
        Console.WriteLine();
        Console.WriteLine("Opciones:");
        Console.WriteLine("  --expert, -e  Activa el modo experto interactivo (con scan)");
        Console.WriteLine();
        Console.WriteLine("Ejemplos:");
        Console.WriteLine("  sbox scan              Escaneo básico con informe");
        Console.WriteLine("  sbox scan --expert     Escaneo con modo experto interactivo");
        Console.WriteLine("  sbox info              Información del sistema");
        Console.WriteLine();
        Console.WriteLine("Nota: La interfaz gráfica (WPF) solo está disponible en Windows.");
        Console.WriteLine("Este CLI funciona en Windows, Linux y macOS.");
    }
}
