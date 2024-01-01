using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCore.Graphics.Widget;

namespace KCore.Graphics.Widgets
{
    public abstract class BasePanel
    {
        internal bool optimized = false;
        internal int redrawersstart;

        public void Bind(Request request)
        {
            requestList.Add(request);
            optimized = false;
        }
        public void Bind(params Request[] requests)
        {
            requestList.AddRange(requests);
            optimized = false;
        }
        public void Bind(Widget widget)
        {
            requestList.AddRange(widget.InternalGetBinds(GetBindingForm()));
            optimized = false;
        }
        public void Bind(params Widget[] widgets)
        {
            for (var i = 0; i < widgets.Length; i++)
                requestList.AddRange(widgets[i].InternalGetBinds(GetBindingForm()));
            optimized = false;
        }
        public void Bind(BasePanel panel)
        {
            requestList.AddRange(panel.GetPanelBinds());
            optimized = false;
        }
        public void Unbind(Request request)
        {
            requestList.Remove(request);
            optimized = false;
        }
        public void Unbind(Widget widget)
        {
            requestList.RemoveAll(x => x is Widget.IWidgetRequest wr && wr.Widget == widget);
            optimized = false;
        }
        public void Unbind(BasePanel panel)
        {
            if (panel is Form form)
            {
                foreach (var bind in form.GetPanelBinds())
                    requestList.Remove(bind);
            }
            else requestList.RemoveAll(x => x is Panel.IPanelRequest pr && pr.Panel == panel);
            optimized = false;
        }

        internal List<Request> requestList = new List<Request>();
        internal Request[] requests;
        public Request[] Requests => requestList.ToArray();
        protected abstract IList<Request> GetPanelBinds();
        public abstract Form GetBindingForm();
        public abstract bool AllRedraw { get; }

        public void Optimize()
        {
            if (requestList?.Count > 0)
            {
                requests = new Request[requestList.Count]; var index = 0;

                for (var i = 0; i < requestList.Count; i++)
                    if (!requestList[i].AllRedraw) requests[index++] = requestList[i];
                for (var i = 0; i < requestList.Count; i++)
                    if (requestList[i].AllRedraw) requests[index++] = requestList[i];

                redrawersstart = Array.FindIndex(requests, x => x.AllRedraw);
                if (redrawersstart == -1) redrawersstart = requests.Length;
            }
            else
            {
                requests = new Request[0];
            }
            optimized = true;
        }
    }

    public class Panel : BasePanel, INestedWidgets, IResizeble
    {
        #region Visible (Hide + Show)
        public bool Visible { get => visible; set { if (value) Show(); else Hide(); } }
        public IList<Widget> AreBelow { get; set; } = new Widget[0];

        private bool visible = true;
        private void MainHide()
        {
            HideTrigger = false;
            if (!visible) return;
            OnHiding?.Invoke(this);
            visible = false;
            foreach (var x in AreBelow) x.Redraw();
        }
        private void MainShow()
        {
            ShowTrigger = false;
            if (visible) return;
            OnShowing?.Invoke(this);
            visible = true;
        }
        #endregion

        #region Enabling
        public bool Enabled { get => enabled; set { if (value) Enable(); else Disable(); } }

        private bool enabled = true;
        private void MainEnable()
        {
            EnableTrigger = false;
            if (enabled) return;
            OnEnabling?.Invoke(this);
            enabled = true;
        }
        private void MainDisable()
        {
            DisableTrigger = false;
            if (!enabled) return;
            OnDisabling?.Invoke(this);
            enabled = false;
        }
        #endregion

        #region Requests
        public interface IPanelRequest
        {
            Panel Panel { get; }
        }

        [DebuggerDisplay("[{Panel.GetType().Name,nq}] {GetType().Name.Replace(\"Panel\", \"\"),nq}")]
        public abstract class PanelRequest<T> : Request, IPanelRequest where T : Panel
        {
            public T Panel;
            public PanelRequest(T panel) : base(panel.BindedTo)
            {
                Panel = panel;
            }

            Panel IPanelRequest.Panel => Panel;
        }

        private sealed class PanelRequests : PanelRequest<Panel>
        {
            public PanelRequests(Panel panel) : base(panel) { }
            public override void Send() { }
            public override void Cancel() { }
            public override bool Condition() => Panel.Enabled;
            public override void Invoke()
            {
                if (!Panel.Enabled) return;
                for (var i = 0; i < Panel.redrawersstart; i++)
                {
                    if (Panel.requests[i].Condition())
                        Panel.requests[i].Invoke();
                }
            }
        }
        private sealed class PanelRedrawers : PanelRequest<Panel>
        {
            public PanelRedrawers(Panel panel) : base(panel) { }
            public override bool AllRedraw => true;
            public override void Send() { }
            public override void Cancel() { }
            public override bool Condition() => Panel.Visible;
            public override void Invoke()
            {
                if (!Panel.Visible) return;
                for (var i = Panel.redrawersstart; i < Panel.requests.Length; i++)
                {
                    if (Panel.BindedTo.allredraw || Panel.requests[i].Condition())
                        Panel.requests[i].Invoke();
                }
            }
        }
        private sealed class PanelHide : PanelRequest<Panel>
        {
            public PanelHide(Panel panel) : base(panel) { }
            public override bool Condition() => Panel.HideTrigger;
            public override void Send() => Panel.HideTrigger = true;
            public override void Cancel() => Panel.HideTrigger = false;
            public override void Invoke() => Panel.MainHide();
        }
        private sealed class PanelShow : PanelRequest<Panel>
        {
            public PanelShow(Panel panel) : base(panel) { }
            public override bool Condition() => Panel.ShowTrigger;
            public override void Send() => Panel.ShowTrigger = true;
            public override void Cancel() => Panel.ShowTrigger = false;
            public override void Invoke() => Panel.MainShow();
        }
        private sealed class PanelEnable : PanelRequest<Panel>
        {
            public PanelEnable(Panel panel) : base(panel) { }
            public override bool Condition() => Panel.EnableTrigger;
            public override void Send() => Panel.EnableTrigger = true;
            public override void Cancel() => Panel.EnableTrigger = false;
            public override void Invoke() => Panel.MainEnable();
        }
        private sealed class PanelDisable : PanelRequest<Panel>
        {
            public PanelDisable(Panel panel) : base(panel) { }
            public override bool Condition() => Panel.DisableTrigger;
            public override void Send() => Panel.DisableTrigger = true;
            public override void Cancel() => Panel.DisableTrigger = false;
            public override void Invoke() => Panel.MainDisable();
        }
        #endregion

        #region Triggers
        private bool HideTrigger;
        private bool ShowTrigger;
        private bool EnableTrigger;
        private bool DisableTrigger;
        #endregion

        #region Events
        public event Action<Panel> OnHiding;
        public event Action<Panel> OnShowing;
        public event Action<Panel> OnEnabling;
        public event Action<Panel> OnDisabling;
        public event Action<Panel> OnResizing;
        #endregion

        #region Atomic requests
        private void Hide()
        {
            HideTrigger = true;
        }
        private void Show()
        {
            ShowTrigger = true;
        }
        private void Enable()
        {
            EnableTrigger = true;
        }
        private void Disable()
        {
            DisableTrigger = true;
        }
        #endregion

        protected override IList<Request> GetPanelBinds()
        {
            return new Request[]
            {
                new PanelRequests(this),
                new PanelRedrawers(this),
                new PanelHide(this),
                new PanelShow(this),
                new PanelEnable(this),
                new PanelDisable(this),
            };
        }

        public Panel(Form form)
        {
            BindedTo = form;
        }
        public Form BindedTo { get; set; }
        public override bool AllRedraw => BindedTo.AllRedraw;
        protected List<Widget> Childs { get; set; } = new List<Widget>();
        public Widget[] GetChilds() => Childs.ToArray();

        public void AddWidget(Widget widget)
        {
            Childs.Add(widget);
            Bind(widget);
            Optimize();
        }
        public void RemoveWidget(Widget widget)
        {
            Childs.Remove(widget);
            Unbind(widget);
            Optimize();
        }
        public void ClearWidgets(Predicate<Widget> predicate)
        {
            var i = 0;
            while (i < Childs.Count)
            {
                if (predicate(Childs[i]))
                {
                    Unbind(Childs[i]);
                    Childs.RemoveAt(i);
                }
                else i++;
            }
            Optimize();
        }
        public void ClearWidgets()
        {
            for (var i = 0; i < Childs.Count; i++)
                Unbind(Childs[i]);
            Childs.Clear();
            Optimize();
        }

        public override Form GetBindingForm() => BindedTo;

        public void Resize()
        {
            var count = Childs.Count;
            for (var i = 0; i < count; i++)
                Childs[i].Resize();
            OnResizing?.Invoke(this);
        }
    }
}
