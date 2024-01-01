using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics
{
    [DebuggerDisplay("[{GetType().Name}]")]
    public abstract class Request
    {
        private BasePanel bindedTo;
        public BasePanel BindedTo => bindedTo;
        public virtual bool AllRedraw => false;
        public abstract void Send();
        public abstract void Cancel();
        public abstract bool Condition();
        public abstract void Invoke();
        public Request(BasePanel form)
        {
            bindedTo = form;
        }
    }

    public class Trigger : Request
    {
        private bool Request;
        public Action<BasePanel> InvokeDelegate;

        public Trigger(BasePanel form, Action<BasePanel> invoke = null) : base(form)
        {
            InvokeDelegate = invoke ?? (f => { });
        }

        public override bool Condition() => Request;
        public override void Invoke()
        {
            InvokeDelegate(BindedTo);
            Request = false;
        }
        public void Pull() => Request = true;

        public override void Cancel() => Request = false;
        public override void Send() => Request = true;
    }

    public class TriggerRedrawer : Trigger
    {
        public TriggerRedrawer(Form form, Action<BasePanel> invoke = null)
            : base(form, invoke)
        {

        }
        public override bool AllRedraw => true;
    }

    public class Redrawer : DelegateRequest
    {
        public Redrawer(Form form, Func<BasePanel, bool> condition = null, Action<BasePanel> invoke = null, Action<BasePanel> cancel = null) 
            : base(form, condition, invoke, cancel)
        {

        }
        public override bool AllRedraw => true;
    }

    public class DelegateRequest : Request
    {
        public Action<BasePanel> CancelDelegate = form => { };
        public Action<BasePanel> RegisterDelegate = form => { };
        public Func<BasePanel, bool> ConditionDelegate = form => false;
        public Action<BasePanel> InvokeDelegate = form => { };

        public DelegateRequest(Form form, Func<BasePanel, bool> condition = null, Action<BasePanel> invoke = null, Action<BasePanel> cancel = null) : base(form)
        {
            ConditionDelegate = condition ?? (f => true);
            InvokeDelegate = invoke ?? (f => { });
            CancelDelegate = cancel ?? (f => { });
        }

        public override bool Condition() => ConditionDelegate(BindedTo);
        public override void Invoke() => InvokeDelegate(BindedTo);
        public override void Cancel() => CancelDelegate(BindedTo);
        public override void Send() => RegisterDelegate(BindedTo);
    }
}
