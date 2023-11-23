using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using KCore;
using KCore.TerminalCore;
using KCore.CoreForms;
using KCore.Graphics;
using KCore.Graphics.Widgets;

namespace KCore
{
    /// <summary>
    /// Базовый тип консольных окон
    /// </summary>
    public abstract class Form
    {
        #region fields and vmethods 
        private bool status, allredraw, resize;

        private bool clear;

        private int redrawersstart;

        private bool optimized = false;

        private bool needshowing = false;

        private bool onetimeresized = false;

        protected virtual ConsoleColor Background { get => Theme.Back; }

        /// <summary>
        /// Обработчик клавиш
        /// </summary>
        protected virtual void OnKeyDown(byte key)
        {
            ActiveWidget?.OnKeyDown(key);
        }

        /// <summary>
        /// Обработчик клавиш
        /// </summary>
        protected virtual void OnKeyUp(byte key)
        {
            ActiveWidget?.OnKeyUp(key);
        }

        /// <summary>
        /// Срабатывает при каждой перерисовке перед реакциями
        /// </summary>
        protected virtual void OnAllRedraw() { }

        /// <summary>
        /// Срабатывает при каждой перерисовке после реакций
        /// </summary>
        protected virtual void OnTopAllRedraw() { }

        /// <summary>
        /// Срабатывает при открытии этого блока
        /// </summary>
        protected virtual void OnOpening() { }

        /// <summary>
        /// Срабатывает при выходе из этого блока
        /// </summary>
        protected virtual void OnClosing() { }

        /// <summary>
        /// Срабатывает при открытии свёрнутого окна
        /// </summary>
        protected virtual void OnShowing() { }

        /// <summary>
        /// Срабатывает при возвращении в этот блок из другого блока
        /// </summary>
        protected virtual void OnReturned() { }

        /// <summary>
        /// Срабатывает при изменении размера окна консоли и при старте блока
        /// </summary>
        protected virtual void OnResize() { }

        #endregion
        public TimeSpan FormTimer { get; private set; }
        public IWidget RootWidget { get; protected set; }
        public IControlable ActiveWidget { get => activeWidget; protected set
            {
                activeWidget = value;
                is_redrawable = value is IRedrawable;
                redrawable = value as IRedrawable;
            }
        }
        private IControlable activeWidget;
        private bool is_redrawable;
        private IRedrawable redrawable;

        /// <summary>
        /// Словарь флагов
        /// </summary>
        public Dictionary<string, bool> Reactions { get; private set; } = new Dictionary<string, bool>();
        private List<Reaction> reactionlist = new List<Reaction>();

        private Reaction[] reactions;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public Form Reference { get; set; } = null;

        private void Optimize()
        {
            if (reactionlist?.Count > 0)
            {
                foreach (var x in reactionlist)
                    if (!x.multicondition && !Reactions.ContainsKey(x.reference)) Reactions.Add(x.reference, false);

                reactions = new Reaction[reactionlist.Count]; var index = 0;

                for (var i = 0; i < reactionlist.Count; i++)
                    if (!reactionlist[i].isredrawer) reactions[index++] = reactionlist[i];
                for (var i = 0; i < reactionlist.Count; i++)
                    if (reactionlist[i].isredrawer) reactions[index++] = reactionlist[i];

                redrawersstart = Array.FindIndex(reactions, x => x.isredrawer);
                if (redrawersstart == -1) redrawersstart = reactions.Length;
            }
            else
            {
                reactions = new Reaction[0];
            }
            optimized = true;
        }

        #region KeypressInline
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void KeypressInline(byte key, string reaction, byte key1)
        {
            if (key == key1) Reactions[reaction] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void KeypressInline(byte key, string reaction, byte key1, byte key2)
        {
            if (key == key1 || key == key2) Reactions[reaction] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void KeypressInline(byte key, string reaction, byte key1, byte key2, byte key3)
        {
            if (key == key1 || key == key2 || key == key3) Reactions[reaction] = true;
        }
        #endregion

        #region Adders
        protected void Add(params object[] objects)
        {
            for (var i = 0; i < objects.Length; i++)
            {
                switch (objects[i])
                {
                    case Action action: reactionlist.Add(Reaction.Create(action)); break;
                    case ValueTuple<string, Action> refaction: reactionlist.Add(Reaction.Create(refaction.Item1, refaction.Item2)); break;
                    case ValueTuple<Func<bool>, Action> condaction: reactionlist.Add(Reaction.Create(condaction.Item1, condaction.Item2)); break;
                    default: throw new ArgumentException("Overload not find");
                }
            }
            optimized = false;
        }

        protected void Add(Action action)
        {
            reactionlist.Add(Reaction.Create(action));
            optimized = false;

        }

        protected void Add(params Action[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
                reactionlist.Add(Reaction.Create(actions[i]));
            optimized = false;
        }

        protected void Add(string reference, Action action)
        {
            reactionlist.Add(Reaction.Create(reference, action));
            optimized = false;
        }

        protected void Add(ValueTuple<string, Action> refaction)
        {
            reactionlist.Add(Reaction.Create(refaction.Item1, refaction.Item2));
            optimized = false;
        }

        protected void Add(params ValueTuple<string, Action>[] refactions)
        {
            for (var i = 0; i < refactions.Length; i++)
                reactionlist.Add(Reaction.Create(refactions[i].Item1, refactions[i].Item2));
            optimized = false;
        }

        protected void Add(Func<bool> cond, Action action)
        {
            reactionlist.Add(Reaction.Create(cond, action));
            optimized = false;
        }

        protected void Add(ValueTuple<Func<bool>, Action> condaction)
        {
            reactionlist.Add(Reaction.Create(condaction.Item1, condaction.Item2));
            optimized = false;
        }

        protected void Add(params ValueTuple<Func<bool>, Action>[] condactions)
        {
            for (var i = 0; i < condactions.Length; i++)
                reactionlist.Add(Reaction.Create(condactions[i].Item1, condactions[i].Item2));
            optimized = false;
        }

        protected void AddRedrawer(params object[] objects)
        {
            for (var i = 0; i < objects.Length; i++)
            {
                switch (objects[i])
                {
                    case Action action: reactionlist.Add(Reaction.CreateRedrawer(action)); break;
                    case ValueTuple<string, Action> refaction: reactionlist.Add(Reaction.CreateRedrawer(refaction.Item1, refaction.Item2)); break;
                    case ValueTuple<Func<bool>, Action> condaction: reactionlist.Add(Reaction.CreateRedrawer(condaction.Item1, condaction.Item2)); break;
                    default: throw new ArgumentException("Overload not find");
                }
            }
            optimized = false;
        }

        protected void AddRedrawer(Action action)
        {
            reactionlist.Add(Reaction.CreateRedrawer(action));
            optimized = false;
        }

        protected void AddRedrawer(params Action[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
                reactionlist.Add(Reaction.CreateRedrawer(actions[i]));
            optimized = false;
        }

        protected void AddRedrawer(string reference, Action action)
        {
            reactionlist.Add(Reaction.CreateRedrawer(reference, action));
            optimized = false;
        }

        protected void AddRedrawer(ValueTuple<string, Action> refaction)
        {
            reactionlist.Add(Reaction.CreateRedrawer(refaction.Item1, refaction.Item2));
            optimized = false;
        }

        protected void AddRedrawer(params ValueTuple<string, Action>[] refactions)
        {
            for (var i = 0; i < refactions.Length; i++)
                reactionlist.Add(Reaction.CreateRedrawer(refactions[i].Item1, refactions[i].Item2));
            optimized = false;
        }

        protected void AddRedrawer(Func<bool> cond, Action action)
        {
            reactionlist.Add(Reaction.CreateRedrawer(cond, action));
            optimized = false;
        }

        protected void AddRedrawer(ValueTuple<Func<bool>, Action> condaction)
        {
            reactionlist.Add(Reaction.CreateRedrawer(condaction.Item1, condaction.Item2));
            optimized = false;
        }

        protected void AddRedrawer(params ValueTuple<Func<bool>, Action>[] condactions)
        {
            for (var i = 0; i < condactions.Length; i++)
                reactionlist.Add(Reaction.CreateRedrawer(condactions[i].Item1, condactions[i].Item2));
            optimized = false;
        }

        protected void AddRedrawer(Redrawable redrawable)
        {
            reactionlist.Add(Reaction.CreateRedrawer(() => redrawable.NeedRedraw, () => redrawable.Draw()));
            optimized = false;
        }
        #endregion

        #region Main Logic
        private void MainLoop(bool exit_after = true)
        {
            if (resize)
            {
                Terminal.Resize(Background);
                allredraw = true;
                if (Terminal.WindowSizeExternalManage && !ResizeViewer.ResizeStarted)
                    Start(new ResizeViewer());
                RootWidget?.UpdateSizes();
                OnResize();
                onetimeresized = true;
                Console.CursorVisible = false;
            }

            if (needshowing)
            {
                OnShowing();
                needshowing = false;
            }

            if (ManualRedraw)
            {
                clear = true;
                ManualRedraw = false;
            }

            if (clear)
            {
                allredraw = true;
                clear = false;
            }

            if (!(resize | !allredraw)) Terminal.Clear(Background);

            if (resize)
            {
                resize = false;
            }

            if (ActiveDashborads && !ManualLock)
            {
                if (AllowedDashboard && !IsRecursiveForm() && DashboardForm != null)
                {
                    Start(DashboardForm);
                }
                ActiveDashborads = false;
            }

            if (RaiseException)
            {
                RaiseException = false;
                if (!IsRecursiveForm()) throw new Exception();
            }

            try
            {
                for (var i = 0; i < redrawersstart; i++)
                {
                    if (reactions[i].multicondition)
                    {
                        if (reactions[i].condition.Invoke()) reactions[i].action();
                    }
                    else if (Reactions[reactions[i].reference])
                    {
                        reactions[i].action();
                        Reactions[reactions[i].reference] = false;
                    }
                }

                if (exit_after && resize || !status) return;

                if (allredraw) OnAllRedraw();

                if (is_redrawable && redrawable.NeedRedraw)
                {
                    redrawable.Redraw();
                    redrawable.NeedRedraw = false;
                }

                for (var i = redrawersstart; i < reactions.Length; i++)
                {
                    if (reactions[i].multicondition)
                    {
                        if (allredraw || reactions[i].condition.Invoke()) reactions[i].action();
                    }
                    else if (allredraw || Reactions[reactions[i].reference])
                    {
                        reactions[i].action();
                        Reactions[reactions[i].reference] = false;
                    }
                }

                if (allredraw) OnTopAllRedraw();

                if (allredraw)
                {
                    if (AllowedDashboard && !IsRecursiveForm())
                    {
                        Terminal.Set(Terminal.FixedWindowWidth - ConsoleShellTextRight.Length - 1, Terminal.FixedWindowHeight - 1);
                        Terminal.Write(ConsoleShellTextRight);
                        Terminal.Set(1, Terminal.FixedWindowHeight - 1);
                        Terminal.Write(ConsoleShellTextLeft);
                    }
                    allredraw = false;
                }
            }
            catch (ArgumentException e) when (e.TargetSite.Name == "SetCursorPosition")
            {
                resize = true;
            }
            catch (ArgumentOutOfRangeException e) when (e.TargetSite.Name == "SetBufferSize")
            {
                resize = true;
            }

            if (!needshowing && !Terminal.WindowIsActive)
            {
                if (!Terminal.UpdateWindowInactive) while (!Terminal.WindowIsActive) System.Threading.Thread.Sleep(20);
                needshowing = true;
            }
            if (Terminal.UpdatesPerSecond > 0) System.Threading.Thread.Sleep(Terminal.UPS);

            if (ManualStop)
            {
                while (ManualStop) System.Threading.Thread.Sleep(20);
            }

            var bfi = TerminalBase.GetBufferInfo();
            if (!(bfi.WindowWidth() == Terminal.FixedWindowWidth && bfi.WindowHeight() == Terminal.FixedWindowHeight))
            {
                resize = true;
            }
        }

        public void OneTimeDrawForRedirection()
        {
            if (!optimized) Optimize();

            OnOpening();
            OnResize();
            var loc_status = status;
            allredraw = true;
            status = true;

            try
            {
                MainLoop(false);
            }
            catch { }
            status = loc_status;

        }

        public void Start(bool redirected = false)
        {
            if (!optimized) Optimize();

            Terminal.AddKeyDown(OnKeyDown);
            Terminal.AddKeyUp(OnKeyUp);

            OnOpening();
            OnResize();

            status = true;
            allredraw = !redirected;
            stopwatch.Start();
            while (status)
            {
                try
                {
                    MainLoop();
                    FormTimer = stopwatch.Elapsed;
                }
                catch (Exception e)
                {
                    var ev = new ExceptionViewer(e, AllowedRestartAfterException);
                    this.StartAnimation(ev);
                    if (!ev.Restarted) status = false;
                }
            }
            stopwatch.Stop();

            Terminal.ClearKeyDown(OnKeyDown);
            Terminal.ClearKeyUp(OnKeyUp);

            OnClosing();
        }

        public void Start(Form form, bool redirected = false, bool other_form_will_be_start = false)
        {
            form.Reference = this;

            Terminal.ClearKeyUp(form.Reference.OnKeyUp);
            Terminal.ClearKeyDown(form.Reference.OnKeyDown);

            stopwatch.Stop();
            form.Start(redirected);
            stopwatch.Start();

            OnReturned();
            if (!other_form_will_be_start)
            {
                Terminal.Clear(Background);
            }

            allredraw = true;
            resize = form.onetimeresized;

            Terminal.ClearKeyDown(OnKeyDown);
            Terminal.ClearKeyUp(OnKeyUp);
            Terminal.AddKeyDown(form.Reference.OnKeyDown);
            Terminal.AddKeyUp(form.Reference.OnKeyUp);

            form.Reference = null;
        }
        #endregion

        #region Manage methods
        internal void StopAllRedraw()
        {
            allredraw = false;
        }

        public void CancelAllActions()
        {
            for (var i = 0; i < reactions.Length; i++)
            {
                if (!reactions[i].isredrawer && !reactions[i].multicondition) Reactions[reactions[i].reference] = false;
            }
        }

        public void Close() => status = false;

        public void Clear() => clear = true;
        #endregion

        #region Form Finding
        private static Type FormType { get; } = typeof(Form);

        private Form InternalFindFirst(Type type)
        {
            if (Reference != null)
            {
                if (Reference.GetType() == type) return Reference;
                else return Reference.InternalFindFirst(type);
            }
            else
            {
                throw new NullReferenceException("Form with this type not found");
            }
        }
        private Form InternalFindFirstOrDefault(Type type)
        {
            if (Reference != null)
            {
                if (Reference.GetType() == type) return Reference;
                else return Reference.InternalFindFirstOrDefault(type);
            }
            else
            {
                return null;
            }
        }

        private static bool IsFormType(Type type)
        {
            var tp = type;
            var find = type == FormType;
            while (!find && tp != typeof(object))
            {
                if (tp == FormType) find = true;
                tp = tp.BaseType;
            }
            return find;
        }

        /// <summary>
        /// Находит ссылку на первый блок в цепочке блоков с заданным типом
        /// </summary>
        /// <param name="type">Тип-наследник блока</param>
        /// <returns>Первый найденный блок</returns>
        protected Form FindFirst(Type type)
        {
            if (!IsFormType(type)) throw new Exception("BaseType is not Form");

            return InternalFindFirst(type);
        }

        /// <summary>
        /// Находит ссылку на первый блок в цепочке блоков с заданным типом
        /// </summary>
        /// <param name="type">Тип-наследник блока</param>
        /// <returns>Первый найденный блок, либо null</returns>
        protected Form FindFirstOrDefault(Type type)
        {
            if (!IsFormType(type)) throw new Exception("BaseType is not Form");

            return InternalFindFirstOrDefault(type);
        }
        #endregion

        #region Manual Manage methods
        public static void Stop()
        {
            ManualStop = true;
        }

        public static void Resume()
        {
            ManualStop = false;
        }

        public static void Abort()
        {
            Terminal.OnKeyDown -= ConsoleShellActivate;
        }
        #endregion

        public static bool AllowDashboardInAllForms = true;
        protected bool AllowedRestartAfterException = true;
        protected bool AllowedDashboard = AllowDashboardInAllForms;
        private static bool ManualStop = false;
        const string ConsoleShellTextLeft = "KCore " + KCoreVersion.VersionWithoutRevision + " ";
        const string ConsoleShellTextRight = "Dashboard: F12";
        private static bool ManualRedraw = false;
        private static bool RaiseException = false;
        public bool ManualLock = false;
        protected virtual bool IsRecursiveForm() => false;
        protected static Form DashboardForm = new Dashboard();
        protected static Form ExceptionForm;
        private static bool ActiveDashborads;
        private static void ConsoleShellActivate(byte x)
        {
            if (x == Key.F2) RaiseException = true;
            if (x == Key.F8) ManualStop = !ManualStop;
            if (x == Key.F10) ManualRedraw = true;
            if (Key.Control.Pressed() && x == Key.F12) ActiveDashborads = true;
        }
        static Form()
        {
            Terminal.OnKeyDown += ConsoleShellActivate;
        }
    }
}