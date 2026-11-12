namespace SutilBox.CLI.Core;

public static class InfoOutput
{
    public static void Display(Dictionary<string, string> info)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────┐");
        Console.WriteLine("│           INFORMACIÓN DEL SISTEMA                  │");
        Console.WriteLine("└────────────────────────────────────────────────────┘");
        Console.WriteLine();

        var maxKeyLength = info.Keys.Max(k => k.Length);
        var paddingSize = maxKeyLength + 2;

        foreach (var (key, value) in info)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var keyPadded = key.PadRight(paddingSize);
            Console.Write($"  {keyPadded}");
            Console.ResetColor();

            var lines = value.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                    Console.WriteLine(lines[i].Trim());
                else
                {
                    var pad = new string(' ', paddingSize);
                    Console.WriteLine($"  {pad}{lines[i]}");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"📅 {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
    }
}
