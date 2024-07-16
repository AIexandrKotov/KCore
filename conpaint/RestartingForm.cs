using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace conpaint
{
    public partial class RestartingForm : Form
    {
        bool Ticked = false;

        public RestartingForm()
        {
            InitializeComponent();
#if NETFRAMEWORK
            label2.Text = "The program will restart automatically";
#else
            label2.Text = "Please, restart program (close form, not console)";
#endif
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Ticked)
            {
#if NETFRAMEWORK
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Environment.Exit(0);
#endif
            }
            else Ticked = true;
        }
    }
}
