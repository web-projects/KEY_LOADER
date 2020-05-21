using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XO.Requests;

namespace DEVICE_CORE
{
    class Program
    {
        static readonly DeviceActivator activator = new DeviceActivator();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            string pluginPath = Path.Combine(Environment.CurrentDirectory, "DevicePlugins");

            IDeviceApplication application = activator.Start(pluginPath);
            await application.Run().ConfigureAwait(false);
            await Task.Delay(5120);

            // GET STATUS
            await application.Command(LinkDeviceActionType.GetStatus).ConfigureAwait(false);
            await Task.Delay(5120);

            Console.WriteLine("\nCOMMANDS: [l=LOAD, t=TEST, s=STATUS, q=QUIT]\r\n");

            ConsoleKey keypressed = Console.ReadKey(true).Key;

            while (keypressed != ConsoleKey.Q)
            {
                switch (keypressed)
                {
                    case ConsoleKey.L:
                    {
                        Console.WriteLine("\r\nCOMMAND: [LOAD]");
                        break;
                    }
                    case ConsoleKey.T:
                    {
                        Console.WriteLine("\r\nCOMMAND: [TEST]");
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        Console.WriteLine("\r\nCOMMAND: [STATUS]");
                        await application.Command(LinkDeviceActionType.GetSecurityConfiguration).ConfigureAwait(false);
                        break;
                    }
                }

                await Task.Delay(50).ConfigureAwait(false);

                keypressed = Console.ReadKey(true).Key;
            }

            Console.WriteLine("\r\nCOMMAND: [QUIT]\r\n");

            application.Shutdown();
        }
    }
}
