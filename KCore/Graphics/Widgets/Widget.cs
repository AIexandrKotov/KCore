using KCore.CoreForms;
using KCore.TerminalCore;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    public abstract class Widget : BoundedObject
    {
        public Widget(
            int left = 0, int top = 0, IContainer container = null, Alignment? alignment = null, bool fillWidth = true, bool fillHeight = true) 
            : base(left, top, container, alignment, fillWidth, fillHeight)
        {

        }

        #region Requests
        public interface IWidgetRequest
        {
            Widget Widget { get; }
        }

        [DebuggerDisplay("[{Widget.GetType().Name,nq}] {GetType().Name.Replace(\"Widget\", \"\"),nq}")]
        public abstract class WidgetRequest<T> : Request, IWidgetRequest where T: Widget
        {
            public T Widget;
            public WidgetRequest(Form form, T widget) : base(form)
            {
                Widget = widget;
            }

            Widget IWidgetRequest.Widget => Widget;
        }
        private sealed class WidgetRedraw : WidgetRequest<Widget>
        {
            public WidgetRedraw(Form form, Widget widget) : base(form, widget) { }

            public override bool AllRedraw => true;
            public override bool Condition() => Widget.RedrawTrigger;
            public override void Cancel() => Widget.RedrawTrigger = false;
            public override void Invoke() => Widget.MainRedraw();
        }
        private sealed class WidgetReclear : WidgetRequest<Widget>
        {
            public WidgetReclear(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.ReclearTrigger;
            public override void Cancel() => Widget.ReclearTrigger = false;
            public override void Invoke() => Widget.MainReclear();
        }
        private sealed class WidgetHide : WidgetRequest<Widget>
        {
            public WidgetHide(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.HideTrigger;
            public override void Cancel() => Widget.HideTrigger = false;
            public override void Invoke() => Widget.MainHide();
        }
        private sealed class WidgetShow : WidgetRequest<Widget>
        {
            public WidgetShow(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.ShowTrigger;
            public override void Cancel() => Widget.ShowTrigger = false;
            public override void Invoke() => Widget.MainShow();
        }
        private sealed class WidgetEnable : WidgetRequest<Widget>
        {
            public WidgetEnable(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.EnableTrigger;
            public override void Cancel() => Widget.EnableTrigger = false;
            public override void Invoke() => Widget.MainEnable();
        }
        private sealed class WidgetDisable : WidgetRequest<Widget>
        {
            public WidgetDisable(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.DisableTrigger;
            public override void Cancel() => Widget.DisableTrigger = false;
            public override void Invoke() => Widget.MainDisable();
        }
        private sealed class WidgetSelect : WidgetRequest<Widget>
        {
            public WidgetSelect(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.SelectTrigger;
            public override void Cancel() => Widget.SelectTrigger = false;
            public override void Invoke() => Widget.MainSelect();
        }
        private sealed class WidgetDeselect : WidgetRequest<Widget>
        {
            public WidgetDeselect(Form form, Widget widget) : base(form, widget) { }
            public override bool Condition() => Widget.DeselectTrigger;
            public override void Cancel() => Widget.DeselectTrigger = false;
            public override void Invoke() => Widget.MainDeselect();
        }

        #endregion

        #region Binding
        internal IList<Request> InternalGetBinds(Form form)
        {
            var ret = new List<Request>();

            ret.AddRange(GetBinds(form));
            ret.Add(new WidgetRedraw(form, this));
            ret.Add(new WidgetReclear(form, this));
            if (AllowedVisible)
            {
                ret.Add(new WidgetShow(form, this));
                ret.Add(new WidgetHide(form, this));
            }
            if (AllowedEnabling)
            {
                ret.Add(new WidgetEnable(form, this));
                ret.Add(new WidgetDisable(form, this));
            }
            if (AllowedSelecting)
            {
                ret.Add(new WidgetSelect(form, this));
                ret.Add(new WidgetDeselect(form, this));
            }
            return ret;
        }
        public virtual IList<Request> GetBinds(Form form) => Array.Empty<Request>();
        #endregion

        #region Events
        public event Action<Widget> OnResizing;
        public event Action<Widget> OnResized;

        public event Action<Widget> OnReclearing;
        public event Action<Widget> OnRedrawing;

        public event Action<Widget> OnHiding;
        public event Action<Widget> OnShowing;

        public event Action<Widget> OnEnabling;
        public event Action<Widget> OnDisabling;

        public event Action<Widget> OnSelecting;
        public event Action<Widget> OnDeselecting;
        #endregion

        #region Triggers
        private bool RedrawTrigger;
        private bool ReclearTrigger;

        private bool HideTrigger;
        private bool ShowTrigger;

        private bool EnableTrigger;
        private bool DisableTrigger;

        private bool SelectTrigger;
        private bool DeselectTrigger;
        #endregion

        #region Atomic requests
        public void Redraw()
        {
            RedrawTrigger = true;
        }
        public void Reclear()
        {
            ReclearTrigger = true;
        }
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
        private void Select()
        {
            SelectTrigger = true;
        }
        private void Deselect()
        {
            DeselectTrigger = true;
        }
        #endregion

        #region Redraw + Reclear
        public virtual bool AlwaysClear => false;
        private void MainRedraw()
        {
            if (AllowedVisible && !Visible) return;
            OnRedrawing?.Invoke(this);
            if (AlwaysClear) Clear();
            Draw();
            RedrawTrigger = false;
        }
        private void MainReclear()
        {
            if (AllowedVisible && !Visible) return;
            OnReclearing?.Invoke(this);
            Clear();
            ReclearTrigger = false;
        }
        #endregion

        #region Resizing
        internal void InternalResize()
        {
            OnResizing(this);
            Resize();
            OnResized(this);
        }
        public abstract void Resize();
        #endregion

        #region Visible (Hide + Show)
        public bool Visible { get => visible; set { if (value) Show(); else Hide(); } }
        public virtual bool AllowedVisible => true;
        public IList<Widget> AreBelow { get; set; } = new Widget[0];

        private bool visible = true;
        private void MainHide()
        {
            HideTrigger = false;
            if (!visible) return;
            OnHiding?.Invoke(this);
            Clear();
            visible = false;
            foreach (var x in AreBelow) x.Redraw();
        }
        private void MainShow()
        {
            ShowTrigger = false;
            if (visible) return;
            OnShowing?.Invoke(this);
            visible = true;
            Redraw();
        }
        #endregion

        #region Enabling
        public bool Enabled { get => enabled; set { if (value) Enable(); else Disable(); } }
        public virtual bool AllowedEnabling => false;

        private bool enabled = true;
        private void MainEnable()
        {
            EnableTrigger = false;
            if (enabled) return;
            OnEnabling?.Invoke(this);
            enabled = true;
            Redraw();
        }
        private void MainDisable()
        {
            DisableTrigger = false;
            if (!enabled) return;
            OnDisabling?.Invoke(this);
            enabled = false;
            Redraw();
        }
        #endregion

        #region Selecting
        public bool Selected { get => selected; set { if (value) Select(); else Deselect(); } }
        public virtual bool SelectNeedsRedraw => false;
        public virtual bool AllowedSelecting => false;
        private bool selected = false;
        private void MainSelect()
        {
            SelectTrigger = false;
            if (selected) return;
            selected = true;
            OnSelecting?.Invoke(this);
            if (SelectNeedsRedraw) Redraw();
        }
        private void MainDeselect()
        {
            DeselectTrigger = false;
            if (!selected) return;
            selected = false;
            OnDeselecting?.Invoke(this);
            if (SelectNeedsRedraw) Redraw();
        }
        #endregion
    }
}
