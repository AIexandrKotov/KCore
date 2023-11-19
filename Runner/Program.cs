using KCore;
using KCore.Forms;
using KCore.Graphics;

namespace Runner
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var f = new SimpleEventForm();
            f.AllRedraw += () => f.StartAnimation(new Form2());
            f.Start();
        }
    }
}
