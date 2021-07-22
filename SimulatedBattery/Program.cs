using Microsoft.Win32;
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
            public double Voltage = 0;
            public double Current = 0;
            public double Power = 0;
            public double Electricity = 0;
            public double PowerFactor = 0;
            public double C02 = 0;
            public double Temperature = 0;
            public double Frequency = 0;
        }
        static byte[] Req = new byte[] { 0x01, 0x03, 0x00, 0x48, 0x00, 0x08, 0xC4, 0x1A };

        static Info BatteryInfo = new Info();

        static int BatteryPercentage = 100;
        static bool IsCharging = false;

        private static bool IsWDTFInstalled
        {
            get { return Type.GetTypeFromProgID("WDTF2.WDTF") != null; }
        }
        private static dynamic SimulatedBatterySystemSystemAction;


        //MODIFY HERE
        private static string PORT = "COM3";
        static double MaxVoltage = 12.6;
        static double MinVoltage = 11.1;

        static unsafe void Main(string[] args)
        {
            //Test
            //IsCharging = true;

            INIT_IO();

            INSTALL_WDTF();

            INIT_WDTF();

            RUN_WHEN_BOOT();

            EnableBattery();
            IF_CHARGE();

            for (; ; )
            {
            }
        }

        private static void RUN_WHEN_BOOT() 
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            Process process = Process.GetCurrentProcess();
            registryKey.SetValue(process.ProcessName, process.MainModule.FileName);
        }

        private static void IF_CHARGE() 
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

        private static unsafe void INIT_IO()
        {
            SerialPort serialPort = new SerialPort(PORT, 4800);
            serialPort.Open();
            serialPort.DataReceived += (s, e) =>
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);

                byte[] buffer = new byte[64];
                int len = serialPort.Read(buffer, 0, buffer.Length);
                for(int i = 0; i < len; i++) 
                {
                    Console.Write(buffer[i].ToString("x2") + " ");
                }
                Console.WriteLine();

                byte[] data = new byte[32];
                for (int i = 0; i < data.Length; i++)
                {
                    //               Header Size
                    data[i] = buffer[i + 3];
                }

                READ_DATA(data);

                double M = MaxVoltage - MinVoltage;
                double C = BatteryInfo.Voltage - MinVoltage;

                if (!IsCharging)
                {
                    BatteryPercentage = (int)((C / M) * 100);
                    SetBatteryPercentage(BatteryPercentage);

                    Console.WriteLine("BatteryPercentage:" + BatteryPercentage);
                }
            };

            Timer timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                serialPort.Write(Req, 0, Req.Length);
            };
            System.Threading.Thread.Sleep(1000);
            timer.Start();
        }

        private static void INSTALL_WDTF()
        {
            if (!IsWDTFInstalled)
            {
                Console.Write("Installing WDTF");
                int CT = Console.CursorTop;
                int CL = Console.CursorLeft;


                SET_INSTALLER(Mode.Remove);
                SET_INSTALLER(Mode.Create);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "msiexec";
                processStartInfo.Arguments = $"/i \"{MSIPath}\" /quiet";
                processStartInfo.Verb = "RunAs";
                Process process = Process.Start(processStartInfo);

                int C = 0;
                int MAX = 5;

                Console.CursorVisible = false;

                while (!process.HasExited)
                {
                    Console.SetCursorPosition(CL, CT);
                    if (C > MAX) 
                    {
                        C = 0;
                    }
                    for(int i = 0; i < C; i++) 
                    {
                        Console.Write(".");
                    }
                    for (int i = 0; i < MAX-C; i++)
                    {
                        Console.Write(" ");
                    }
                    C++;
                    System.Threading.Thread.Sleep(100);
                }

                Console.CursorVisible = true;
                Console.WriteLine();

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
            BatteryInfo.Voltage = (data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]) * 0.0001d;
            BatteryInfo.Current = (data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]) * 0.0001d;
            BatteryInfo.Power = (data[8] << 24 | data[9] << 16 | data[10] << 8 | data[11]) * 0.0001d;
            BatteryInfo.Electricity = (data[12] << 24 | data[13] << 16 | data[14] << 8 | data[15]) * 0.0001d;
            BatteryInfo.PowerFactor = (data[16] << 24 | data[17] << 16 | data[18] << 8 | data[19]) * 0.001d;
            BatteryInfo.C02 = (data[20] << 24 | data[21] << 16 | data[22] << 8 | data[23]) * 0.0001d;
            BatteryInfo.Temperature = (data[24] << 24 | data[25] << 16 | data[26] << 8 | data[27]) * 0.01d;
            BatteryInfo.Frequency = (data[28] << 24 | data[29] << 16 | data[30] << 8 | data[31]) * 0.01d;

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

        private static void SET_INSTALLER(Mode mode)
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
