using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace HideriDotNet
{
    public partial class Form1 : Form
    {
        public Program botProgram;
        public Form1()
        {
            InitializeComponent();
            
        }
        
        public void Update()
        {
            this.Text = "Hideri Bot [Running " + Program.botSettings.botName + "] ["+ Program.modules.Count.ToString()+" modules loaded]";
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            Console.WriteLine("UI Closed.");
        }
    }
}
