using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimulatedBattery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Battery.Initialize();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Battery.EnableBattery();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Battery.DisableBattery();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Battery.SetBatteryToAC();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Battery.SetBatteryToDC();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Battery.SetBatteryPercentage(trackBar1.Value);
        }
    }
}
