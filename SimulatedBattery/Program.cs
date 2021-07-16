using SimulatedBattery.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SimulatedBattery
{
    class Program
    {
        private static bool IsWDTFInstalled 
        {
            get { return Type.GetTypeFromProgID("WDTF2.WDTF") != null; }
        }
        private static dynamic SimulatedBatterySystemSystemAction;

        static void Main(string[] args)
        {
            if (!IsWDTFInstalled) 
            {
                Console.WriteLine("Installing WDTF");

                SetInstaller(Mode.Remove);
                SetInstaller(Mode.Create);

                Process process = Process.Start("msiexec", $"/i \"{MSIPath}\" /quiet");

                while (!process.HasExited) 
                {
                }
                if (!IsWDTFInstalled) 
                {
                    Console.WriteLine("Faild To Install");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    Console.WriteLine("WDTF Installed");
                }
            }

            InitWDTF();

            ConsoleColor DefaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Commands:"); 
            Console.WriteLine("Enable: Enable Simulated Battery"); 
            Console.WriteLine("Disable: Disable Simulated Battery"); 
            Console.WriteLine("AC: AC Mode"); 
            Console.WriteLine("DC: DC Mode");
            Console.WriteLine("Input Any Number To Set Battery Percentage");
            Console.ForegroundColor = DefaultColor;

            for (; ; )
            {
                string s = Console.ReadLine().ToUpper();
                switch (s)
                {
                    case "DC":
                        SetBatteryToDC();
                        break;
                    case "AC":
                        SetBatteryToAC();
                        break;
                    case "ENABLE":
                        EnableBattery();
                        break;
                    case "DISABLE":
                        DisableBattery();
                        break;
                    default:
                        SetBatteryPercentage(Convert.ToInt32(s));
                        break;
                }
            }
        }

        enum Mode 
        {
            Create,
            Remove
        }

        private static string MSIPath = Path.Combine(Path.GetTempPath(), "Windows Driver Testing Framework (WDTF) Runtime Libraries-x64_en-us.msi");
        private static string CabPath = Path.Combine(Path.GetTempPath(), "52909056ae20065680e3c9283d5a4a21.cab");

        private static void SetInstaller(Mode mode) 
        {
            switch (mode) 
            {
                case Mode.Create:
                    File.WriteAllBytes(MSIPath, Resources.MSI);
                    File.WriteAllBytes(CabPath, Resources.CAB);
                    break;
                case Mode.Remove:
                    if (File.Exists(MSIPath))
                    {
                        File.Delete(MSIPath);
                    }
                    if (File.Exists(CabPath))
                    {
                        File.Delete(CabPath);
                    }
                    break;
            }
        }

        private static void InitWDTF()
        {
            Type WDTFType = Type.GetTypeFromProgID("WDTF2.WDTF");
            dynamic WDTF = Activator.CreateInstance(WDTFType);
            SimulatedBatterySystemSystemAction = WDTF.SystemDepot.ThisSystem.GetInterface("SimulatedBatterySystem");
        }

        private static void EnableBattery()
        {
            SimulatedBatterySystemSystemAction.DisableRealBatteries();
            SimulatedBatterySystemSystemAction.EnableSimulatedBattery();
        }

        private static void DisableBattery()
        {
            SimulatedBatterySystemSystemAction.EnableRealBatteries();
            SimulatedBatterySystemSystemAction.DisableSimulatedBattery();
        }

        private static void SetBatteryPercentage(int Value)
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryChargePercentage(Value);
        }

        private static void SetBatteryToDC()
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryToDC();
        }

        private static void SetBatteryToAC()
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryToAC();
        }
    }
}
