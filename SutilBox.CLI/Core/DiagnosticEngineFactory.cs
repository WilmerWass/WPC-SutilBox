namespace SutilBox.CLI.Core;

public static class DiagnosticEngineFactory
{
    public static IDiagnosticEngine Create()
    {
        if (PlatformDetector.IsWindows)
        {
            return new WindowsDiagnosticEngine();
        }
        else
        {
            return new UnixDiagnosticEngine();
        }
    }
}
