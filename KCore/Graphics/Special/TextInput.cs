using KCore.Graphics.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Special
{

    /// <summary>
    /// Помощник при вводе текста. Блок, в котором определён этот помощник, обязан иметь метод InputStringRedraw, а в конце KeyPress блока должно быть добавлено Input(key)
    /// </summary>
    public class TextInput : IControlable
    {
        #region Internal
        static char[] Chars = new char[59] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ы', 'Ь', 'Э', 'Ю', 'Я', 'Ё' };
        static char[] Alpha_specs = new char[1] { '_' };
        static byte[] SevenKeys = new byte[7] { 186, 188, 190, 192, 219, 221, 222 };
        //static char[] Availables = new char[] { ' ', ';', '\'', '/', '@', '$', '%', '&', '*', '?', '!', '(', ')', '"', '-', '+', '=', '_' };
        static bool IsAvailableNickname(char c)
        {
            if (char.IsDigit(c)) return true;
            if (Chars.Contains(c)) return true;
            if (Alpha_specs.Contains(c)) return true;
            return false;
        }
        static bool IsCharMeetRequirement(string s, char c, TextInputStern textInputStern)
        {
            if (textInputStern == TextInputStern.AllSymbols) return true;
            if (textInputStern == TextInputStern.AvailableNickname)
            {
                return IsAvailableNickname(c);
            }
            if (textInputStern == TextInputStern.Digits)
            {
                if (s.Length == 0) return char.IsDigit(c) || c == '-' || c == '+';
                return char.IsDigit(c);
            }

            return false;
        }
        static char ToRus(byte x)
        {
            switch (x)
            {
                case Key.F: return 'А';
                case 188: return 'Б';
                case Key.D: return 'В';
                case Key.U: return 'Г';
                case Key.L: return 'Д';
                case Key.T: return 'Е';
                case 192: return 'Ё';
                case 186: return 'Ж';
                case Key.P: return 'З';
                case Key.B: return 'И';
                case Key.Q: return 'Й';
                case Key.R: return 'К';
                case Key.K: return 'Л';
                case Key.V: return 'М';
                case Key.Y: return 'Н';
                case Key.J: return 'О';
                case Key.G: return 'П';
                case Key.H: return 'Р';
                case Key.C: return 'С';
                case Key.N: return 'Т';
                case Key.E: return 'У';
                case Key.A: return 'Ф';
                case 219: return 'Х';
                case Key.W: return 'Ц';
                case Key.X: return 'Ч';
                case Key.I: return 'Ш';
                case Key.O: return 'Щ';
                case 221: return 'Ъ';
                case Key.S: return 'Ы';
                case Key.M: return 'Ь';
                case 222: return 'Э';
                case 190: return 'Ю';
                case Key.Z: return 'Я';
                default: return (char)x;
            }
        }

        public enum TextInputStern
        {
            AllSymbols,
            AvailableNickname,
            Digits
        }

        int limitlength = 65535;
        bool rusinput = false;
        bool inputmode = false;
        TextInputStern stern = TextInputStern.AllSymbols;
        StringBuilder inputstring = new StringBuilder();
        Form Reference;

        void Add(char c)
        {
            if (!inputmode) return;
            if (inputstring.Length < limitlength)
            {
                if (!IsCharMeetRequirement(inputstring.ToString(), c, stern)) return;
                if (Key.CapsLock.Triggered() ^ Key.Shift.Pressed()) inputstring.Append(c);
                else inputstring.Append(char.ToLower(c));
                Reference.Reactions[RedrawMethodName] = true;
            }
        }
        void DelWord()
        {
            if (!inputmode) return;
            if (inputstring.Length > 0)
            {
                var s = inputstring.ToString();
                if (string.IsNullOrWhiteSpace(s)) return;
                else
                {
                    var ind = s.LastIndexOf(' ');
                    if (ind != -1)
                    {
                        inputstring.Clear();
                        inputstring.Append(s.Substring(0, ind));
                    }
                    else
                    {
                        inputstring.Clear();
                    }
                    Reference.Reactions[RedrawMethodName] = true;
                }
            }
        }
        void Del()
        {
            if (!inputmode) return;
            if (inputstring.Length > 0)
            {
                inputstring.Remove(inputstring.Length - 1, 1);
                Reference.Reactions[RedrawMethodName] = true;
            }
        }
        #endregion

        public Action OnAnyInput { get; set; } = delegate { };
        public string RedrawMethodName { get; set; } = "InputStringRedraw";
        public Action RedrawMethod { set => RedrawMethodName = value.Method.Name; }
        public TextInput(Form form)
        {
            Reference = form;
        }
        public TextInput(Form form, int capacity)
        {
            Reference = form;
            limitlength = capacity;
        }
        public int Capacity { get => limitlength; set => limitlength = value; }
        public string String
        {
            get => inputstring.ToString();
            set
            {
                inputstring.Clear();
                inputstring.Append(value);
            }
        }
        public bool InputMode { get => inputmode; set => inputmode = value; }
        public bool AllowMultiline { get; set; } = false;
        public bool AllowTabulation { get; set; } = false;

        public void Activate()
        {
            inputmode = true;
        }
        public void Activate(int capacity)
        {
            inputmode = true;
            limitlength = capacity;
        }
        public void Activate(TextInputStern stern)
        {
            inputmode = true;
            this.stern = stern;
        }
        public void Activate(TextInputStern stern, int capacity)
        {
            inputmode = true;
            this.stern = stern;
            limitlength = capacity;
        }
        public void Activate(string s)
        {
            inputmode = true;
            String = s;
        }
        public void Activate(string s, int capacity)
        {
            inputmode = true;
            String = s;
            limitlength = capacity;
        }
        public void Activate(string s, TextInputStern stern)
        {
            inputmode = true;
            String = s;
            this.stern = stern;
        }
        public void Activate(string s, TextInputStern stern, int capacity)
        {
            inputmode = true;
            String = s;
            this.stern = stern;
            limitlength = capacity;
        }
        public string Deactivate()
        {
            inputmode = false;
            stern = TextInputStern.AllSymbols;
            return String;
        }

        public void Input(byte key)
        {
            if (!inputmode) return;
            if (key == Key.Alt) rusinput = !rusinput;
            if (key >= 'A' && key <= 'Z') Add(rusinput ? ToRus(key) : (char)key);
            else if (rusinput && SevenKeys.Contains(key)) Add(ToRus(key));
            else if (key == "'"[0]) Add((char)key);
            else if (key >= Key.NumPad0 && key <= Key.NumPad9) Add((char)(key - 48));
            else if (key == Key.Backspace)
            {
                if (Key.Control.Pressed()) DelWord();
                else Del();
            }
            else
            {
                switch (key)
                {
                    case Key.Enter:
                        {
                            if (AllowMultiline) Add((char)10);
                        }
                        break;
                    case Key.Tab:
                        {
                            if (AllowTabulation)
                            {
                                Add(' ');
                                Add(' ');
                                Add(' ');
                                Add(' ');
                            }
                        }
                        break;
                    case Key.Spacebar: Add(' '); break;
                    case Key.D0: Add(Key.Shift.Pressed() ? ')' : '0'); break;
                    case Key.D1: Add(Key.Shift.Pressed() ? '!' : '1'); break;
                    case Key.D2: Add(Key.Shift.Pressed() ? (rusinput ? '"' : '@') : '2'); break;
                    case Key.D3: Add(Key.Shift.Pressed() ? (rusinput ? ';' : '#') : '3'); break;
                    case Key.D4: Add(Key.Shift.Pressed() ? (rusinput ? '%' : '$') : '4'); break;
                    case Key.D5: Add(Key.Shift.Pressed() ? '%' : '5'); break;
                    case Key.D6: Add(Key.Shift.Pressed() ? (rusinput ? ':' : '^') : '6'); break;
                    case Key.D7: Add(Key.Shift.Pressed() ? (rusinput ? '?' : '&') : '7'); break;
                    case Key.D8: Add(Key.Shift.Pressed() ? '*' : '8'); break;
                    case Key.D9: Add(Key.Shift.Pressed() ? '(' : '9'); break;
                    case Key.Multiply: Add('*'); break;
                    case Key.Divide: Add('/'); break;
                    case Key.Oem1: Add(Key.Shift.Pressed() ? ':' : ';'); break;
                    case Key.OemPlus: Add(Key.Shift.Pressed() ? '+' : '='); break;
                    case Key.OemComma: Add(Key.Shift.Pressed() ? '<' : ','); break;
                    case Key.OemMinus: Add(Key.Shift.Pressed() ? '_' : '-'); break;
                    case Key.OemPeriod: Add(Key.Shift.Pressed() ? '>' : '.'); break;
                    case Key.Oem2: Add(Key.Shift.Pressed() ? (rusinput ? ',' : '?') : (rusinput ? '.' : '/')); break;
                    case Key.Oem3: Add(Key.Shift.Pressed() ? '~' : '`'); break;
                    case Key.Oem4: Add(Key.Shift.Pressed() ? '{' : '['); break;
                    case Key.Oem5: Add(Key.Shift.Pressed() ? (rusinput ? '/' : '|') : '\\'); break;
                    case Key.Oem6: Add(Key.Shift.Pressed() ? '}' : ']'); break;
                    case Key.Oem7: Add(Key.Shift.Pressed() ? '"' : '\''); break;
                    default: break;
                }
            }
            OnAnyInput();
        }
        public void Clear()
        {
            inputstring.Clear();
        }

        public void OnKeyDown(byte key)
        {
            Input(key);
        }

        public void OnKeyUp(byte key) { }

    }
}
