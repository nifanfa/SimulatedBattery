using Microsoft.Win32;
using SimulatedBattery.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SimulatedBattery
{
    internal static class Battery
    {
        public static bool IsCharging = false;

        private static bool IsWDTFInstalled
        {
            get { return Type.GetTypeFromProgID("WDTF2.WDTF") != null; }
        }
        private static dynamic SimulatedBatterySystemSystemAction;

        public static void SetRunAsStartup()
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            Process process = Process.GetCurrentProcess();
            registryKey.SetValue(process.ProcessName, process.MainModule.FileName);
        }

        public static void SetupCharge()
        {
            Timer timer = new Timer(1000);
            int Phase = 0;
            timer.Elapsed += (sender, e) =>
            {
                if (IsCharging)
                {
                    switch (Phase)
                    {
                        case 0:
                            SetBatteryPercentage(25);
                            break;
                        case 1:
                            SetBatteryPercentage(50);
                            break;
                        case 2:
                            SetBatteryPercentage(75);
                            break;
                        case 3:
                            SetBatteryPercentage(100);
                            break;
                    }
                    Phase++;
                    if (Phase > 3)
                    {
                        Phase = 0;
                    }
                }
            };
            timer.Start();
        }

        public static void Initialize()
        {
            if (!IsWDTFInstalled)
            {
                Console.WriteLine("Installing WDTF");

                SetInstaller(true);
                SetInstaller(false);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "msiexec";
                processStartInfo.Arguments = $"/i \"{MSIPath}\" /quiet";
                processStartInfo.Verb = "RunAs";
                Process process = Process.Start(processStartInfo);

                Console.CursorVisible = false;

                while (!process.HasExited) ;

                if (!IsWDTFInstalled)
                {
                    Console.WriteLine("Faild To Install");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                else
                {
                    Console.WriteLine("WDTF Installed");
                }
            }

            Type WDTFType = Type.GetTypeFromProgID("WDTF2.WDTF");
            dynamic WDTF = Activator.CreateInstance(WDTFType);
            SimulatedBatterySystemSystemAction = WDTF.SystemDepot.ThisSystem.GetInterface("SimulatedBatterySystem");
        }

        private static string MSIPath = Path.Combine(Path.GetTempPath(), "Windows Driver Testing Framework (WDTF) Runtime Libraries-x64_en-us.msi");
        private static string CabPath = Path.Combine(Path.GetTempPath(), "52909056ae20065680e3c9283d5a4a21.cab");

        private static void SetInstaller(bool Remove)
        {
            if (Remove) 
            {
                if (File.Exists(MSIPath))
                {
                    File.Delete(MSIPath);
                }
                if (File.Exists(CabPath))
                {
                    File.Delete(CabPath);
                }
            }
            else
            {
                File.WriteAllBytes(MSIPath, Resources.msi);
                File.WriteAllBytes(CabPath, Resources.cab);
            }
        }

        public static void EnableBattery()
        {
            SimulatedBatterySystemSystemAction.DisableRealBatteries();
            SimulatedBatterySystemSystemAction.EnableSimulatedBattery();
        }

        public static void DisableBattery()
        {
            SimulatedBatterySystemSystemAction.EnableRealBatteries();
            SimulatedBatterySystemSystemAction.DisableSimulatedBattery();
        }

        public static void SetBatteryPercentage(int Value)
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryChargePercentage(Value);
        }

        public static void SetBatteryToDC()
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryToDC();
        }

        public static void SetBatteryToAC()
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryToAC();
        }
    }
}
