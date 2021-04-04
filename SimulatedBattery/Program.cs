using System;

namespace SimulatedBattery
{
    class Program
    {
        private static dynamic SimulatedBatterySystemSystemAction;

        static void Main(string[] args)
        {
            try
            {
                InitWDTF();
                for (; ; )
                {
                    string s = Console.ReadLine();
                    switch (s)
                    {
                        case "DC":
                            SetBatteryToDC();
                            break;
                        case "AC":
                            SetBatteryToAC();
                            break;
                        case "Enable":
                            EnableBattery();
                            break;
                        case "Disable":
                            DisableBattery();
                            break;
                        default:
                            SetBatteryPercentage(Convert.ToInt32(s));
                            break;
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                Console.ReadKey();
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
