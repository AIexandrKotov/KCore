using System;

namespace KCore
{
    /// <summary>
    /// Виды действий:
    /// — Реакция (Reaction). Подразумевает в себе реакцию на нажатие клавиши.
    ///   Ради потокобезопасности передаётся через булевую переменную.
    ///   Реакция при AllRedraw: нет.
    ///   Есть условие: нет. Есть ссылка на булевое значение в словаре
    ///   Есть итоговый делегат: да.
    ///   
    /// — Перерисовка (Redrawer). Представляет интерфейс, который будет выведен в консоль
    ///   Ради потокобезопасности использует булевые передатчики
    ///   Реакция при AllRedraw: да.
    ///   Есть условие: нет.
    ///   Есть итоговый делегат: да.
    ///   
    /// В итоговом делегате можно прописывать условия
    /// </summary>
    public sealed class Reaction
    {
        internal Action action;
        internal Func<bool> condition;
        internal bool multicondition;
        internal bool isredrawer;
        internal bool allredraw;
        internal string reference;
        public void Invoke() => action();

        private Reaction() { }
        public static Reaction Create(Action action)
        {
            var r = new Reaction();
            r.action = action;
            r.reference = action.Method.Name;
            return r;
        }
        public static Reaction Create(string reference, Action action)
        {
            var r = new Reaction();
            r.action = action;
            r.reference = reference;
            return r;
        }
        public static Reaction Create(Func<bool> cond, Action action)
        {
            var r = new Reaction();
            r.action = action;
            r.multicondition = true;
            r.condition = cond;
            return r;
        }

        public static Reaction CreateRedrawer(Action action)
        {
            var r = new Reaction();
            r.isredrawer = true;
            r.allredraw = true;
            r.action = action;
            r.reference = action.Method.Name;
            return r;
        }
        public static Reaction CreateRedrawer(string reference, Action action)
        {
            var r = new Reaction();
            r.isredrawer = true;
            r.allredraw = true;
            r.action = action;
            r.reference = reference;
            return r;
        }
        public static Reaction CreateRedrawer(Func<bool> cond, Action action)
        {
            var r = new Reaction();
            r.isredrawer = true;
            r.allredraw = true;
            r.action = action;
            r.multicondition = true;
            r.condition = cond;
            return r;
        }

        public override string ToString()
        {
            return reference == null ? action?.Method.Name : reference;
        }
    }
}
