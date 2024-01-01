using KCore;
using KCore.Extensions;
using KCore.Graphics.Widgets;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Form3 : Form
    {
        public Window Window;
        public ScrollableText Text;
        public VerticalScroll VerticalScroll;
        public Trigger CloseTrigger;

        public Form3()
        {
            var nu_dawai = @"Ну давай разберем по частям, тобою написанное ))
Складывается впечатление что ты реально контуженный, обиженный жизнью имбицил ))
Могу тебе и в глаза сказать, готов приехать послушать?)
Вся та хуйня тобою написанное это простое пиздабольство, рембо ты комнатный))
от того что ты много написал, жизнь твоя лучше не станет))
пиздеть не мешки ворочить, много вас таких по весне оттаяло ))
Про таких как ты говорят: Мама не хотела, папа не старался)
Вникай в моё послание тебе постарайся проанализировать и сделать выводы для себя)";

            Text = new ScrollableText(nu_dawai);
            Text.OnShowing += widget => ActiveWidget = Text;
            Text.OnHiding += widget => ActiveWidget = null;
            ActiveWidget = Text;

            var scroll = new WithVerticalScroll(Text, scroll: VerticalScroll = new VerticalScroll(Text, pixel: ' ', scrollPixel: '▐'), expanding: (1, 1));
            Window = new Window(width: 40, height: 12, padding: (2, 2, 1, 2), alignment: Alignment.CenterWidth | Alignment.CenterHeight,
                title: "%=>Red%тестируем %=>Blue%текст%=>reset%", child: scroll, fillHeight: false, fillWidth: false);
            Window.Resize();

            Root.AddWidget(scroll.Scroll);
            Root.AddWidget(Window);
            Root.AddWidget(Text);

            Bind(CloseTrigger = new Trigger(this, form => (form as Form).Close()));
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.Tab || key == Key.Escape) CloseTrigger.Pull();
            else if (key == Key.Spacebar)
            {
                var nv = !Text.Visible;
                Text.Visible = nv;
                Window.Visible = nv;
                VerticalScroll.Visible = nv;
            }
            else if (key == Key.A)
            {
                Text.Autoscroll = !Text.Autoscroll;
            }
        }
    }
}
