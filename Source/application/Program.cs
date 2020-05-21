using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XO.Requests;

namespace DEVICE_CORE
{
    class Program
    {
        const int COMMAND_WAIT_DELAY = 4096;
        static readonly DeviceActivator activator = new DeviceActivator();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            string pluginPath = Path.Combine(Environment.CurrentDirectory, "DevicePlugins");

            IDeviceApplication application = activator.Start(pluginPath);
            await application.Run().ConfigureAwait(false);
            await Task.Delay(COMMAND_WAIT_DELAY);

            // GET STATUS
            //await application.Command(LinkDeviceActionType.GetStatus).ConfigureAwait(false);
            //await Task.Delay(4096);

            ConsoleKey keypressed = GetKeyPressed();

            while (keypressed != ConsoleKey.Q)
            {
                switch (keypressed)
                {
                    case ConsoleKey.L:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [LOAD]");
                        await application.Command(LinkDeviceActionType.LoadHMACKeys).ConfigureAwait(false);
                        await Task.Delay(COMMAND_WAIT_DELAY);
                        break;
                    }
                    case ConsoleKey.T:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [TEST]");
                        await application.Command(LinkDeviceActionType.GenerateHMAC).ConfigureAwait(false);
                        await Task.Delay(COMMAND_WAIT_DELAY);
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [STATUS]");
                        await application.Command(LinkDeviceActionType.GetSecurityConfiguration).ConfigureAwait(false);
                        await Task.Delay(COMMAND_WAIT_DELAY);
                        break;
                    }
                }

                await Task.Delay(50).ConfigureAwait(false);

                keypressed = GetKeyPressed();
            }

            Console.WriteLine("\r\nCOMMAND: [QUIT]\r\n");

            application.Shutdown();
        }

        static private ConsoleKey GetKeyPressed()
        {
            Console.WriteLine("\nCOMMANDS: [l=LOAD, t=TEST, s=STATUS, q=QUIT]\r\n");
            return Console.ReadKey(true).Key;
        }
    }
}
