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
        private Form bindedTo;
        public Form BindedTo => bindedTo;
        public virtual bool AllRedraw => false;
        public abstract void Cancel();
        public abstract bool Condition();
        public abstract void Invoke();
        public Request(Form form)
        {
            bindedTo = form;
        }
    }

    public class Trigger : Request
    {
        private bool Request;
        public Action<Form> InvokeDelegate;

        public Trigger(Form form, Action<Form> invoke = null) : base(form)
        {
            InvokeDelegate = invoke ?? (f => { });
        }

        public override bool Condition() => Request;
        public override void Invoke()
        {
            InvokeDelegate(BindedTo);
            Request = false;
        }
        public void Do() => Request = true;

        public override void Cancel() => Request = false;
    }

    public class TriggerRedrawer : Trigger
    {
        public TriggerRedrawer(Form form, Action<Form> invoke = null)
            : base(form, invoke)
        {

        }
        public override bool AllRedraw => true;
    }

    public class Redrawer : DelegateRequest
    {
        public Redrawer(Form form, Func<Form, bool> condition = null, Action<Form> invoke = null, Action<Form> cancel = null) 
            : base(form, condition, invoke, cancel)
        {

        }
        public override bool AllRedraw => true;
    }

    public class DelegateRequest : Request
    {
        public Action<Form> CancelDelegate = form => { };
        public Func<Form, bool> ConditionDelegate = form => false;
        public Action<Form> InvokeDelegate = form => { };

        public DelegateRequest(Form form, Func<Form, bool> condition = null, Action<Form> invoke = null, Action<Form> cancel = null) : base(form)
        {
            ConditionDelegate = condition ?? (f => true);
            InvokeDelegate = invoke ?? (f => { });
            CancelDelegate = cancel ?? (f => { });
        }

        public override bool Condition() => ConditionDelegate(BindedTo);
        public override void Invoke() => InvokeDelegate(BindedTo);
        public override void Cancel() => CancelDelegate(BindedTo);
    }
}
