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
                EnableBattery();
                for(; ; ) 
                {
                    Console.Write("Set Percentage:");
                    int i = Convert.ToInt32(Console.ReadLine());
                    SetBatteryPercentage(i);
                    Console.WriteLine($"Battery Percentage:{i}");
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
            //hello
        }

        private static void SetBatteryPercentage(int Value) 
        {
            SimulatedBatterySystemSystemAction.SetSimulatedBatteryChargePercentage(Value);
        }
    }
}
