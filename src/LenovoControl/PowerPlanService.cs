using System.Runtime.InteropServices;
using System.Text;

namespace LenovoControl;

internal enum PowerProfile
{
    Battery,
    Balanced,
    Performance
}

internal sealed record PowerPlanInfo(Guid Guid, string Name);

internal static class PowerPlanService
{
    // GUID standard Windows: non dipende dalla lingua del sistema.
    private static readonly Guid BalancedGuid =
        new("381b4222-f694-41f0-9685-ff5bb260df2e");

    private const uint AccessScheme = 16;

    private const uint ErrorSuccess = 0;
    private const uint ErrorNoMoreItems = 259;

    [DllImport("powrprof.dll")]
    private static extern uint PowerEnumerate(
        IntPtr RootPowerKey,
        IntPtr SchemeGuid,
        IntPtr SubGroupOfPowerSettingsGuid,
        uint AccessFlags,
        uint Index,
        byte[] Buffer,
        ref uint BufferSize);

    [DllImport("powrprof.dll")]
    private static extern uint PowerReadFriendlyName(
        IntPtr RootPowerKey,
        ref Guid SchemeGuid,
        IntPtr SubGroupOfPowerSettingsGuid,
        IntPtr PowerSettingGuid,
        byte[]? Buffer,
        ref uint BufferSize);

    [DllImport("powrprof.dll")]
    private static extern uint PowerGetActiveScheme(
        IntPtr UserRootPowerKey,
        out IntPtr ActivePolicyGuid);

    [DllImport("powrprof.dll")]
    private static extern uint PowerSetActiveScheme(
        IntPtr UserRootPowerKey,
        ref Guid SchemeGuid);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public static IReadOnlyList<PowerPlanInfo> GetAllPlans()
    {
        var plans = new List<PowerPlanInfo>();

        for (uint index = 0; ; index++)
        {
            uint size = 16;
            byte[] buffer = new byte[size];

            uint result = PowerEnumerate(
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                AccessScheme,
                index,
                buffer,
                ref size);

            if (result == ErrorNoMoreItems)
                break;

            if (result != ErrorSuccess)
                continue;

            if (size < 16)
                continue;

            Guid guid = new(buffer.AsSpan(0, 16));

            string name = ReadFriendlyName(guid);

            plans.Add(new PowerPlanInfo(guid, name));
        }

        return plans;
    }

    public static PowerPlanInfo? FindProfile(PowerProfile profile)
    {
        IReadOnlyList<PowerPlanInfo> plans = GetAllPlans();

        return profile switch
        {
            PowerProfile.Balanced =>
                plans.FirstOrDefault(
                    p => p.Guid == BalancedGuid),

            PowerProfile.Battery =>
                plans.FirstOrDefault(
                    p => string.Equals(
                        p.Name,
                        "Lenovo Battery",
                        StringComparison.OrdinalIgnoreCase)),

            PowerProfile.Performance =>
                plans.FirstOrDefault(
                    p => string.Equals(
                        p.Name,
                        "Lenovo Performance",
                        StringComparison.OrdinalIgnoreCase)),

            _ => null
        };
    }

    public static bool TryGetActivePlan(
        out Guid activePlan,
        out uint errorCode)
    {
        activePlan = Guid.Empty;

        errorCode = PowerGetActiveScheme(
            IntPtr.Zero,
            out IntPtr guidPointer);

        if (errorCode != ErrorSuccess ||
            guidPointer == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            activePlan =
                Marshal.PtrToStructure<Guid>(
                    guidPointer);

            return true;
        }
        finally
        {
            LocalFree(guidPointer);
        }
    }

    public static uint SetActivePlan(Guid planGuid)
    {
        Guid guid = planGuid;

        return PowerSetActiveScheme(
            IntPtr.Zero,
            ref guid);
    }

    public static string GetPlanName(Guid guid)
    {
        PowerPlanInfo? plan =
            GetAllPlans()
                .FirstOrDefault(
                    p => p.Guid == guid);

        return plan?.Name ?? "Profilo sconosciuto";
    }

    private static string ReadFriendlyName(Guid schemeGuid)
    {
        uint size = 0;

        PowerReadFriendlyName(
            IntPtr.Zero,
            ref schemeGuid,
            IntPtr.Zero,
            IntPtr.Zero,
            null,
            ref size);

        if (size == 0)
            return schemeGuid.ToString();

        byte[] buffer = new byte[size];

        uint result = PowerReadFriendlyName(
            IntPtr.Zero,
            ref schemeGuid,
            IntPtr.Zero,
            IntPtr.Zero,
            buffer,
            ref size);

        if (result != ErrorSuccess)
            return schemeGuid.ToString();

        return Encoding.Unicode
            .GetString(buffer)
            .TrimEnd('\0');
    }
}
