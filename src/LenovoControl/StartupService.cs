using Microsoft.Win32;

namespace LenovoControl;

internal static class StartupService
{
    private const string RunKeyPath =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    private const string ValueName = "LenovoControl";

    public static bool IsEnabled()
    {
        string? exePath = Environment.ProcessPath;

        if (string.IsNullOrWhiteSpace(exePath))
            return false;

        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(RunKeyPath);

        string? storedValue =
            key?.GetValue(ValueName) as string;

        if (string.IsNullOrWhiteSpace(storedValue))
            return false;

        return string.Equals(
            NormalizePath(storedValue),
            exePath,
            StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey key =
            Registry.CurrentUser.CreateSubKey(
                RunKeyPath,
                writable: true)
            ?? throw new InvalidOperationException(
                "Impossibile accedere alle impostazioni di avvio automatico.");

        if (!enabled)
        {
            key.DeleteValue(
                ValueName,
                throwOnMissingValue: false);

            return;
        }

        string exePath =
            Environment.ProcessPath
            ?? throw new InvalidOperationException(
                "Impossibile determinare il percorso di Lenovo Control.");

        if (exePath.Contains(
                @"\bin\",
                StringComparison.OrdinalIgnoreCase) ||
            exePath.Contains(
                @"\obj\",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "L'avvio automatico può essere abilitato solo dalla versione pubblicata di Lenovo Control.");
        }

        key.SetValue(
            ValueName,
            $"\"{exePath}\"",
            RegistryValueKind.String);
    }

    private static string NormalizePath(string value)
    {
        value = value.Trim();

        if (value.Length >= 2 &&
            value.StartsWith('"') &&
            value.EndsWith('"'))
        {
            value = value[1..^1];
        }

        return value;
    }
}
