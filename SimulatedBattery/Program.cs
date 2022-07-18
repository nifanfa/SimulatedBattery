using Microsoft.Win32;
using SimulatedBattery.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SimulatedBattery
{
    class Program
    {
        [STAThread]
        static unsafe void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }
    }
}
