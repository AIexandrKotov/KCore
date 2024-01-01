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
using System.CodeDom.Compiler;
using KCore.Storage;
using KCore.Tools;

namespace KCore
{
    /// <summary>
    /// Базовый тип консольных окон
    /// </summary>
    public abstract class Form : BasePanel
    {
        public Form()
        {
            Root = new RootWidget(this);
        }
        #region fields and vmethods 
        internal bool allredraw;

        private bool status, resize;

        private bool clear;

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
        private TimeSpan LastStartExecuted;
        private TimeSpan LastEndExecuted;
        public TimeSpan RealUPS => LastEndExecuted - LastStartExecuted;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public Form Reference { get; set; } = null;
        public override Form GetBindingForm() => this;
        public override bool AllRedraw => allredraw;
        protected override IList<Request> GetPanelBinds() => requestList;

        public readonly RootWidget Root;
        /// <summary>
        /// IControlable, управление которым будет перехвачено в данный момент
        /// </summary>
        public IControlable ActiveWidget { get; set; }

        #region Main Logic
        private void MainLoop(bool exit_after = true)
        {
            if (resize)
            {
                Terminal.Resize(Background);
                allredraw = true;
                if (Terminal.WindowSizeExternalManage && !ResizeViewer.ResizeStarted)
                    Start(new ResizeViewer());
                Root.Resize();
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

            if (ActiveDashboard && !ManualLock)
            {
                if (AllowedDashboard && !IsRecursiveForm())
                {
                    Start((Form)Activator.CreateInstance(DashboardType));
                }
                ActiveDashboard = false;
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
                    if (requests[i].Condition())
                        requests[i].Invoke();
                }

                if (exit_after && resize || !status) return;

                if (allredraw) OnAllRedraw();

                for (var i = redrawersstart; i < requests.Length; i++)
                {
                    if (allredraw || requests[i].Condition())
                        requests[i].Invoke();
                }

                if (allredraw) OnTopAllRedraw();

                if (ShowUPS)
                {
                    Terminal.Set(Terminal.FixedWindowWidth / 2 - 5, Terminal.FixedWindowHeight - 1);
                    Terminal.Write($"{1000 / (LastEndExecuted - LastStartExecuted).TotalMilliseconds:0}".PadCenter(10));
                }

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
                Log.Add(e.ToString());
            }
            catch (ArgumentOutOfRangeException e) when (e.TargetSite.Name == "SetBufferSize")
            {
                Log.Add(e.ToString());
            }

            if (!needshowing && !Terminal.WindowIsActive)
            {
                if (!Terminal.UpdateWindowInactive) while (!Terminal.WindowIsActive) System.Threading.Thread.Sleep(20);
                needshowing = true;
            }
            if (Terminal.UpdatesPerSecond > 0) System.Threading.Thread.Sleep(Terminal.UPS);
            if (ShowUPS)
            {
                LastStartExecuted = LastEndExecuted;
                LastEndExecuted = stopwatch.Elapsed;
            }


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
            Root.Resize();
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
            KCoreStorage.Init();
            if (!optimized) Optimize();

            Terminal.AddKeyDown(OnKeyDown);
            Terminal.AddKeyUp(OnKeyUp);

            OnOpening();
            Root.Resize();
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
                    Log.Add(e.ToString());
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
            for (var i = 0; i < requests.Length; i++)
                if (!requests[i].AllRedraw) requests[i].Cancel();
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
        protected Form FindFirst<T>()
        {
            var type = typeof(T);
            if (!IsFormType(type)) throw new Exception("BaseType is not Form");

            return InternalFindFirst(type);
        }

        /// <summary>
        /// Находит ссылку на первый блок в цепочке блоков с заданным типом
        /// </summary>
        /// <param name="type">Тип-наследник блока</param>
        /// <returns>Первый найденный блок, либо null</returns>
        protected Form FindFirstOrDefault<T>()
        {
            var type = typeof(T);
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
        internal static string ConsoleShellTextLeft = "KCore " + Dashboard.Version.Major + "." + Dashboard.Version.Minor + " ";
        internal static string ConsoleShellTextRight => Localization.Current["KCore_DASH"];
        private static bool ManualRedraw = false;
        private static bool RaiseException = false;
        internal static bool ShowUPS = false;
        public bool ManualLock = false;
        protected virtual bool IsRecursiveForm() => false;
        protected static Form ExceptionForm;
        public static Type DashboardType = typeof(Dashboard);
        private static bool ActiveDashboard;
        private static void ConsoleShellActivate(byte x)
        {
            if (x == Key.F8) ManualRedraw = true;
            if (x == Key.F9) ManualStop = !ManualStop;
            if (x == Key.F10) ManualRedraw = true;
            if (x == Key.F12) ActiveDashboard = true;
        }
        static Form()
        {
            Terminal.OnKeyDown += ConsoleShellActivate;
        }
    }
}