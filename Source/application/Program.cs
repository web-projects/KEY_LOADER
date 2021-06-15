﻿using Config.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XO.Requests;

namespace DEVICE_CORE
{
    class Program
    {
        const int STARTUP_WAIT_DELAY = 2048;
        const int COMMAND_WAIT_DELAY = 4096;
        const int CONFIGURATION_UPDATE_DELAY = 6144;
        static readonly DeviceActivator activator = new DeviceActivator();

        static readonly string[] MENU = new string[]
        {
            " ",
            "============ [ MENU ] ============",
            " c => UPDATE EMV CONFIGURATION",
            " i => UPDATE RAPTOR IDLE SCREEN",
            " k => EMV-KERNEL",
            " r => REBOOT",
            " s => STATUS",
            " t => TEST HMAC SECRETS",
            " u => UPDATE HMAC SECRETS",
            " v => ACTIVE ADE SLOT",
            " 0 => LOCK ADE SLOT-0",
            " 8 => LOCK ADE SLOT-8",
            " O => UNLOCK",
            " m => menu",
            " q => QUIT",
            "  "
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            DirectoryInfo di = null;

            // create working directory
            if (!Directory.Exists(Constants.TargetDirectory))
            {
                di = Directory.CreateDirectory(Constants.TargetDirectory);
            }

            string pluginPath = Path.Combine(Environment.CurrentDirectory, "DevicePlugins");

            IDeviceApplication application = activator.Start(pluginPath);
            await application.Run().ConfigureAwait(false);

            // GET STATUS
            //await application.Command(LinkDeviceActionType.GetStatus).ConfigureAwait(false);

            DisplayMenu();
            ConsoleKeyInfo keypressed = GetKeyPressed(false);

            while (keypressed.Key != ConsoleKey.Q)
            {
                bool redisplay = false;

                // Check for <ALT> key combinations
                if ((keypressed.Modifiers & ConsoleModifiers.Alt) != 0)
                {
#if DEBUG
                    switch (keypressed.Key)
                    {
                        case ConsoleKey.A:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [DISPLAY_CUSTOM_SCREEN]");
                            await application.Command(LinkDeviceActionType.DisplayCustomScreen).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.D:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [DATETIME]");
                            await application.Command(LinkDeviceActionType.SetTerminalDateTime).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.F:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [FEATUREENABLEMENTTOKEN]");
                            await application.Command(LinkDeviceActionType.FeatureEnablementToken).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.V:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [VERSION]");
                            await application.Command(LinkDeviceActionType.VIPAVersions).ConfigureAwait(false);
                            break;
                        }
                    }
#endif
                }
                else
                {
                    switch (keypressed.Key)
                    {
                        case ConsoleKey.M:
                        {
                            Console.WriteLine("");
                            DisplayMenu();
                            break;
                        }
                        case ConsoleKey.C:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [CONFIGURATION]");
                            await application.Command(LinkDeviceActionType.Configuration).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.H:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [24_HOUR_REBOOT]");
                            await application.Command(LinkDeviceActionType.Reboot24Hour).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.I:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [UPDATE_IDLE_SCREEN]");
                            await application.Command(LinkDeviceActionType.UpdateIdleScreen).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.O:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [UNLOCK]");
                            await application.Command(LinkDeviceActionType.UnlockDeviceConfig).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.K:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [EMV-KERNEL]");
                            await application.Command(LinkDeviceActionType.GetEMVKernelChecksum).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.R:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [REBOOT]");
                            //await application.Command(LinkDeviceActionType.RebootDevice).ConfigureAwait(false);
                            await application.Command(LinkDeviceActionType.VIPARestart).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.S:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [STATUS]");
                            await application.Command(LinkDeviceActionType.GetSecurityConfiguration).ConfigureAwait(false);
                            //Task.Run(async () => await application.Command(LinkDeviceActionType.GetSecurityConfiguration)).GetAwaiter().GetResult();
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
                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [LOCK]");
                            await application.Command(LinkDeviceActionType.LockDeviceConfig0).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.D8:
                        case ConsoleKey.NumPad8:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [LOCK]");
                            await application.Command(LinkDeviceActionType.LockDeviceConfig8).ConfigureAwait(false);
                            break;
                        }
                        case ConsoleKey.X:
                        {
                            //Console.WriteLine("\r\nCOMMAND: [DEVICE-EXTENDED-RESET]");
                            await application.Command(LinkDeviceActionType.DeviceExtendedReset).ConfigureAwait(false);
                            break;
                        }
                        default:
                        {
                            redisplay = false;
                            break;
                        }
                    }
                }

                keypressed = GetKeyPressed(redisplay);
            }

            Console.WriteLine("\r\nCOMMAND: [QUIT]\r\n");

            application.Shutdown();

            // delete working directory
            DeleteWorkingDirectory(di);
        }

        static private void DeleteWorkingDirectory(DirectoryInfo di)
        {
            if (di == null)
            {
                di = new DirectoryInfo(Constants.TargetDirectory);
            }

            if (di != null)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                di.Delete();
            }
            else if (Directory.Exists(Constants.TargetDirectory))
            {
                di = new DirectoryInfo(Constants.TargetDirectory);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Directory.Delete(Constants.TargetDirectory);
            }
        }

        static private ConsoleKeyInfo GetKeyPressed(bool redisplay)
        {
            if (redisplay)
            {
                Console.Write("SELECT COMMAND: ");
            }
            return Console.ReadKey(true);
        }

        static private void DisplayMenu()
        {
            foreach (string value in MENU)
            {
                Console.WriteLine(value);
            }

            Console.Write("SELECT COMMAND: ");
        }
    }
}
