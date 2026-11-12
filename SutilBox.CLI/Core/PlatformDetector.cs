namespace SutilBox.CLI.Core;

public static class PlatformDetector
{
    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsMacOS => OperatingSystem.IsBrowser() == false && OperatingSystem.IsMacOS();

    public static string GetPlatformName()
    {
        if (IsWindows) return "Windows";
        if (IsLinux) return "Linux";
        if (IsMacOS) return "macOS";
        return "Desconocido";
    }
}
