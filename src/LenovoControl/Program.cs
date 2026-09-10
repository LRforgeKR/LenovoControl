using System.Threading;

namespace LenovoControl;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(
            initiallyOwned: true,
            name: @"Local\LenovoControl.SingleInstance",
            createdNew: out bool createdNew);

        if (!createdNew)
            return;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());

        GC.KeepAlive(mutex);
    }
}
