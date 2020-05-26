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

            ConsoleKey keypressed = GetKeyPressed(true);

            while (keypressed != ConsoleKey.Q)
            {
                bool redisplay = true;

                switch (keypressed)
                {
                    case ConsoleKey.F:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [CONFIGURATION]");
                        await application.Command(LinkDeviceActionType.FeatureEnablementToken).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.K:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [UNLOCK]");
                        await application.Command(LinkDeviceActionType.UnlockDeviceConfig).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.L:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [LOCK]");
                        await application.Command(LinkDeviceActionType.LockDeviceConfig).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.R:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [REBOOT]");
                        await application.Command(LinkDeviceActionType.RebootDevice).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [STATUS]");
                        await application.Command(LinkDeviceActionType.GetSecurityConfiguration).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.T:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [TEST]");
                        await application.Command(LinkDeviceActionType.GenerateHMAC).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.U:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [UPDATE]");
                        await application.Command(LinkDeviceActionType.UpdateHMACKeys).ConfigureAwait(false);
                        break;
                    }
                    case ConsoleKey.V:
                    {
                        //Console.WriteLine("\r\nCOMMAND: [SLOT]");
                        await application.Command(LinkDeviceActionType.GetActiveKeySlot).ConfigureAwait(false);
                        break;
                    }

                    default:
                    {
                        redisplay = false;
                        break;
                    }
                }

                await Task.Delay(COMMAND_WAIT_DELAY).ConfigureAwait(false);
                keypressed = GetKeyPressed(redisplay);
            }

            Console.WriteLine("\r\nCOMMAND: [QUIT]\r\n");

            application.Shutdown();
        }

        static private ConsoleKey GetKeyPressed(bool redisplay)
        {
            if (redisplay)
            { 
                Console.WriteLine("\nCOMMANDS: [f=FET, k=UNLOCK, l=LOCK, r=REBOOT, s=STATUS, t=TEST, u=UPDATE, v=SLOT, q=QUIT]\r\n");
            }
            return Console.ReadKey(true).Key;
        }
    }
}
