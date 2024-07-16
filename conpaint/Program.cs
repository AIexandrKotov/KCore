using System;
using System.Windows.Forms;

namespace conpaint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConpaintConsole.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConpaintForm());
        }
    }
}
