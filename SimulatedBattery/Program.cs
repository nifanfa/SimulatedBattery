using SimulatedBattery.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Timers;

namespace SimulatedBattery
{
    class Program
    {
        public class Info
        {
            public float Voltage = 0;
            public float Current = 0;
            public float Power = 0;
            public float Electricity = 0;
            public float PowerFactor = 0;
            public float C02 = 0;
            public float Temperature = 0;
            public float Frequency = 0;
        }

        static Info BatteryInfo = new Info();

        static int MaxVoltage = 300;
        static int MinVoltage = 100;
        static int BatteryPercentage = 100;

        static byte[] Req = new byte[] { 0x01, 0x03, 0x00, 0x48, 0x00, 0x08, 0xC4, 0x1A };

        private static bool IsWDTFInstalled
        {
            get { return Type.GetTypeFromProgID("WDTF2.WDTF") != null; }
        }
        private static dynamic SimulatedBatterySystemSystemAction;

        private static string PORT = "COM4";

        static unsafe void Main(string[] args)
        {
            INIT_IO();

            INSTALL_WDTF();
            INIT_WDTF();

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

        private static unsafe void INIT_IO()
        {
            SerialPort serialPort = new SerialPort(PORT, 4800);
            serialPort.Open();
            serialPort.DataReceived += (s, e) =>
            {
                byte[] buffer = new byte[64];
                serialPort.Read(buffer, 0, buffer.Length);

                byte[] data = new byte[32];
                for (int i = 0; i < data.Length; i++)
                {
                    //               Header Size
                    data[i] = buffer[i + 3];
                }

                READ_DATA(data);

                float M = MaxVoltage - MinVoltage;
                float C = BatteryInfo.Voltage - MinVoltage;
                BatteryPercentage = (int)((C / M) * 100);
                SetBatteryPercentage(BatteryPercentage);

                Console.WriteLine("BatteryPercentage:" + BatteryPercentage);
            };

            Timer timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                serialPort.Write(Req, 0, Req.Length);
            };
            timer.Start();
        }

        private static void INSTALL_WDTF()
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
        }

        private static unsafe void READ_DATA(byte[] data)
        {
            BatteryInfo.Voltage = (data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]) * 0.0001f;
            BatteryInfo.Current = (data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]) * 0.0001f;
            BatteryInfo.Power = (data[8] << 24 | data[9] << 16 | data[10] << 8 | data[11]) * 0.0001f;
            BatteryInfo.Electricity = (data[12] << 24 | data[13] << 16 | data[14] << 8 | data[15]) * 0.0001f;
            BatteryInfo.PowerFactor = (data[16] << 24 | data[17] << 16 | data[18] << 8 | data[19]) * 0.001f;
            BatteryInfo.C02 = (data[20] << 24 | data[21] << 16 | data[22] << 8 | data[23]) * 0.0001f;
            BatteryInfo.Temperature = (data[24] << 24 | data[25] << 16 | data[26] << 8 | data[27]) * 0.01f;
            BatteryInfo.Frequency = (data[28] << 24 | data[29] << 16 | data[30] << 8 | data[31]) * 0.01f;

            Console.WriteLine("Voltage:" + BatteryInfo.Voltage + "V");
            Console.WriteLine("Current:" + BatteryInfo.Current + "A");
            Console.WriteLine("Power:" + BatteryInfo.Power + "W");
            Console.WriteLine("Electricity:" + BatteryInfo.Electricity + "KWh");
            Console.WriteLine("PowerFactor:" + BatteryInfo.PowerFactor);
            Console.WriteLine("C02:" + BatteryInfo.C02 + "Kg");
            Console.WriteLine("Temperature:" + BatteryInfo.Temperature + "℃");
            Console.WriteLine("Frequency:" + BatteryInfo.Frequency + "Hz");
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

        private static void INIT_WDTF()
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
