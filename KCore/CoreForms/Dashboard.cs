using KCore.Extensions.InsteadSLThree;
using KCore.Extensions;
using KCore.Graphics;
using KCore.Graphics.Containers;
using KCore.Graphics.Core;
using KCore.Graphics.Widgets;
using KCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Permissions;

namespace KCore.CoreForms
{
    internal class Dashboard : Form
    {
        public static KCoreVersion.Reflected Version = new KCoreVersion.Reflected();
        public TriggerRedrawer Downline;

        //Dashboard options:
        //KCore version info (+ difference list)
        //KCore settings
        //KCore Form information
        //KCore Palette
        //SLThree console (if SLThree.dll available)

        public static class Settings
        {
            public static readonly Type SettingsType = typeof(Settings);
            public static Type EntryType = null;

            public const int Spacing = 40;
            public abstract class Setting : Widget
            {
                public Dashboard Reference;
                public virtual bool HasTitle => false;
                public override bool AllowedVisible => base.AllowedVisible;
                public virtual bool HasValue => true;
                public override int Height => HasTitle ? 3 : 1;
                public override int Width => Container.Width;
                public override void Resize() { }
                public override (int, int) Clear(int left, int top)
                {
                    Graph.FillRectangle(Theme.Back, left, top, Width, Height);
                    return (left, top);
                }
                public Setting(Dashboard dashboard) : base(0, 0, null, null, true, false)
                {
                    Reference = dashboard;
                }

                public virtual void OnEntered() { }
                public virtual void OnLeaving() { }

                public IContainer GetSettingKeyContainer(int left, int top)
                {
                    return new StaticContainer(left + 2, top, Spacing - 2, 1);
                }

                public IContainer GetSettingValueContainer(int left, int top)
                {
                    return new StaticContainer(left + Spacing + 2, top, Width - Spacing - 2, 1);
                }

                public IContainer GetContainer(int left, int top)
                {
                    return new StaticContainer(left + 2, top, Width - 2, 1);
                }

                public override (int, int) Draw(int left, int top)
                {
                    if (HasTitle)
                    {
                        Terminal.Set(left, ++top);
                        Terminal.Write(Title.PadCenter(Width).Replace(' ', '-').Replace('_', ' '));
                        top += 1;
                    }

                    if (HasValue)
                    {
                        GetSettingKey().PrintSuperText(GetSettingKeyContainer(left, top), null, TextAlignment.Left);
                        Terminal.Set(left + Spacing, top);
                        Terminal.Write(": ");
                        var value_container = GetSettingValueContainer(left, top);
                        Graph.FillRectangle(Theme.Back, value_container);
                        GetSettingValue().PrintSuperText(value_container, null, TextAlignment.Left);
                    }
                    else
                    {
                        GetSettingKey().PrintSuperText(GetContainer(left, top), null, TextAlignment.Left);
                    }

                    return (left, top);
                }

                public override bool SelectNeedsRedraw => true;

                public virtual string Title => "";
                public abstract string GetSettingKey();
                public abstract string GetSettingValue();
                public bool IsActive => Reference.FieldSettingsPanel.ActiveSubwidget == this;
            }

            public abstract class OnlyKeySetting : Setting
            {
                public override bool HasValue => false;
                public override string GetSettingValue() => "";
                public OnlyKeySetting(Dashboard dashboard) : base(dashboard)
                {
                }
            }

            public class Language : Setting, IControlable
            {
                public Language(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;
                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] { 
                        EnterTrigger = new Trigger(form, panel =>
                        {
                            Localization.Next();
                            (panel as Form).Clear();
                            KCoreStorage.SaveGlobalConfig();
                        }) 
                    };
                }
                public override bool HasTitle => true;
                public override string Title => Localization.Current["DASH_MAIN"];

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_Language"];

                public override string GetSettingValue()
                {
                    var str = Localization.Current.LocalName;
                    return IsActive ? $"%=>Magenta%{str}%=>reset%" : str;
                }
            }

            public class ConsoleSize : Setting, IControlable
            {
                public ConsoleSize(Dashboard dashboard) : base(dashboard) { }

                public Trigger UpTrigger;
                public Trigger DownTrigger;
                public Trigger LeftTrigger;
                public Trigger RightTrigger;
                public override bool HasTitle => true;
                public override string Title => Localization.Current["DASH_GRAPHICS"];
                public bool HeightResizing;
                public bool SavedWSEM;

                public override void OnEntered()
                {
                    SavedWSEM = Terminal.WindowSizeExternalManage;
                    Terminal.WindowSizeExternalManage = false;
                }

                public override void OnLeaving()
                {
                    Terminal.WindowSizeExternalManage = SavedWSEM;
                }

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] {
                        UpTrigger = new Trigger(form, panel =>
                        {
                            if (HeightResizing)
                                Terminal.FixedWindowHeight += 1;
                            else Terminal.FixedWindowWidth += 1;
                            KCoreStorage.SaveLocalConfig();
                        }),
                        DownTrigger = new Trigger(form, panel =>
                        {
                            if (HeightResizing)
                                Terminal.FixedWindowHeight -= 1;
                            else Terminal.FixedWindowWidth -= 1;
                            KCoreStorage.SaveLocalConfig();
                        }),
                        LeftTrigger = new Trigger(form, panel =>
                        {
                            HeightResizing = false;
                            Redraw();
                        }),
                        RightTrigger = new Trigger(form, panel =>
                        {
                            HeightResizing = true;
                            Redraw();
                        }),
                    };
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.A || key == Key.LeftArrow)
                        LeftTrigger.Pull();
                    if (key == Key.D || key == Key.RightArrow)
                        RightTrigger.Pull();
                    if (key == Key.W || key == Key.UpArrow)
                        UpTrigger.Pull();
                    if (key == Key.S || key == Key.DownArrow)
                        DownTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_ConsoleSize"];

                public override string GetSettingValue()
                {
                    var width = Terminal.FixedWindowWidth.ToString();
                    var height = Terminal.FixedWindowHeight.ToString();
                    if (Reference.FieldSettingsPanel.ActiveSubwidget == this)
                    {
                        if (HeightResizing) height = $"%=>Magenta%{height}%=>reset%";
                        else width = $"%=>Magenta%{width}%=>reset%";
                    }
                    return $"{width}x{height}";
                }
            }

            public class MakeMaximal : OnlyKeySetting, IControlable
            {
                public MakeMaximal(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;
                public bool HeightResizing;
                public bool SavedWSEM;

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] {
                        EnterTrigger = new Trigger(form, panel =>
                        {
                            (Terminal.FixedWindowWidth, Terminal.FixedWindowHeight) = (Console.LargestWindowWidth, Console.LargestWindowHeight);
                            KCoreStorage.SaveLocalConfig();
                            (panel as Dashboard).Clear();
                        }),
                    };
                }

                public override void OnEntered()
                {
                    SavedWSEM = Terminal.WindowSizeExternalManage;
                    Terminal.WindowSizeExternalManage = false;
                }

                public override void OnLeaving()
                {
                    Terminal.WindowSizeExternalManage = SavedWSEM;
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => IsActive ? $"%=>Magenta%{Localization.Current["DASH_MakeMaximal"]}%=>reset%": Localization.Current["DASH_MakeMaximal"];
            }

            public class MakeMinimal : OnlyKeySetting, IControlable
            {
                public MakeMinimal(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;
                public bool HeightResizing;
                public bool SavedWSEM;

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] {
                        EnterTrigger = new Trigger(form, panel =>
                        {
                            (Terminal.FixedWindowWidth, Terminal.FixedWindowHeight) = (Terminal.MinimalWindowWidth, Terminal.MinimalWindowHeight);
                            KCoreStorage.SaveLocalConfig();
                            (panel as Dashboard).Clear();
                        }),
                    };
                }
                public override void OnEntered()
                {
                    SavedWSEM = Terminal.WindowSizeExternalManage;
                    Terminal.WindowSizeExternalManage = false;
                }

                public override void OnLeaving()
                {
                    Terminal.WindowSizeExternalManage = SavedWSEM;
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }


                public override string GetSettingKey() => IsActive ? $"%=>Magenta%{Localization.Current["DASH_MakeMinimal"]}%=>reset%" : Localization.Current["DASH_MakeMinimal"];
            }

            public class ExternalSize : Setting, IControlable
            {
                public ExternalSize(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[1] { EnterTrigger = new Trigger(form, x =>
                    {
                        Terminal.WindowSizeExternalManage = !Terminal.WindowSizeExternalManage;
                        KCoreStorage.SaveLocalConfig();
                        Redraw();
                    })};
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_ExternalResizing"];

                public bool GetWSEM()
                {
                    if (Reference.FieldSettingsPanel.ActiveSubwidget?.GetType() == typeof(ConsoleSize))
                        return (Reference.FieldSettingsPanel.ActiveSubwidget as ConsoleSize).SavedWSEM;
                    else return Terminal.WindowSizeExternalManage;
                }
                
                public override string GetSettingValue()
                {
                    var str = GetWSEM() ? Localization.Current["DASH_Enabled"] : Localization.Current["DASH_Disabled"];
                    return IsActive ? $"%=>Magenta%{str}%=>reset%" : str;
                }
            }

            public class UpdatesPerSec : Setting, IControlable
            {
                public Trigger LeftTrigger;
                public Trigger RightTrigger;
                public static string GetText()
                {
                    if (Terminal.UpdatesPerSecond == 0)
                        return Localization.Current["DASH_Unlimited"];
                    return Terminal.UpdatesPerSecond.ToString();
                }

                public static int[] Allowed = new int[] { 12, 24, 44, 70, 0 };

                public UpdatesPerSec(Dashboard dashboard) : base(dashboard) { }
                
                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] {
                        LeftTrigger = new Trigger(form, panel =>
                        {
                            var ind = Array.IndexOf(Allowed, Terminal.UpdatesPerSecond);
                            if (ind == -1) ind = 0;
                            ind--;
                            if (ind < 0) ind = 0;
                            Terminal.UpdatesPerSecond = Allowed[ind];
                            KCoreStorage.SaveLocalConfig();
                            Redraw();
                        }),
                        RightTrigger = new Trigger(form, panel =>
                        {
                            var ind = Array.IndexOf(Allowed, Terminal.UpdatesPerSecond);
                            if (ind == -1) ind = Allowed.Length - 1;
                            ind++;
                            if (ind >= Allowed.Length) ind = Allowed.Length - 1;
                            Terminal.UpdatesPerSecond = Allowed[ind];
                            KCoreStorage.SaveLocalConfig();
                            Redraw();
                        }),
                    };
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.A || key == Key.LeftArrow)
                        LeftTrigger.Pull();
                    if (key == Key.D || key == Key.RightArrow)
                        RightTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_UpdatesPerSec"];
                public override string GetSettingValue()
                {
                    var str = GetText().ToString();
                    return IsActive ? $"%=>Magenta%{str}%=>reset%" : str;
                }
            }

            public class TransitionAnimations : Setting, IControlable
            {
                public TransitionAnimations(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[1] { EnterTrigger = new Trigger(form, x =>
                    {
                        GraphicsExtensions.AllowTransitionAnimations = !GraphicsExtensions.AllowTransitionAnimations;
                        KCoreStorage.SaveLocalConfig();
                        Redraw();
                    })};
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_TransitionAnimations"];
                public override string GetSettingValue()
                {
                    var str = GraphicsExtensions.AllowTransitionAnimations ? Localization.Current["DASH_Enabled"] : Localization.Current["DASH_Disabled"];
                    return IsActive ? $"%=>Magenta%{str}%=>reset%" : str;
                }
            }

            public class TransitionAnimationSpeed : Setting, IControlable
            {
                public Trigger LeftTrigger;
                public Trigger RightTrigger;

                public static int[] Allowed = new int[] { 1, 2, 3, 4, 5, 7, 10, 15, 20, 25, 30, 35, 40 };
                
                public TransitionAnimationSpeed(Dashboard dashboard) : base(dashboard) { }

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[] {
                        LeftTrigger = new Trigger(form, panel =>
                        {
                            var ind = Array.IndexOf(Allowed, GraphicsExtensions.TransitionAnimationsSpeed);
                            if (ind == -1) ind = 0;
                            ind--;
                            if (ind < 0) ind = 0;
                            GraphicsExtensions.TransitionAnimationsSpeed = Allowed[ind];
                            KCoreStorage.SaveLocalConfig();
                            Redraw();
                        }),
                        RightTrigger = new Trigger(form, panel =>
                        {
                            var ind = Array.IndexOf(Allowed, GraphicsExtensions.TransitionAnimationsSpeed);
                            if (ind == -1) ind = Allowed.Length - 1;
                            ind++;
                            if (ind >= Allowed.Length) ind = Allowed.Length - 1;
                            GraphicsExtensions.TransitionAnimationsSpeed = Allowed[ind];
                            KCoreStorage.SaveLocalConfig();
                            Redraw();
                        }),
                    };
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.A || key == Key.LeftArrow)
                        LeftTrigger.Pull();
                    if (key == Key.D || key == Key.RightArrow)
                        RightTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_TransitionAnimationSpeed"];
                public override string GetSettingValue()
                {
                    return IsActive ? $"%=>Magenta%{GraphicsExtensions.TransitionAnimationsSpeed}%=>reset%" : GraphicsExtensions.TransitionAnimationsSpeed.ToString();
                }
            }

            public class CurrentApplication : Setting, IControlable
            {
                public CurrentApplication(Dashboard dashboard) : base(dashboard) { }

                public override bool HasTitle => true;
                public override string Title => Localization.Current["DASH_ADVANCED"];

                public override IList<Request> GetBinds(Form form)
                {
                    return Array.Empty<Request>();
                }

                public void OnKeyDown(byte key)
                {
                    //if (key == Key.E || key == Key.Enter)
                    //    EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_CurrentApplication"];
                public override string GetSettingValue() => KCoreStorage.Portable ? Localization.Current["DASH_Portable"] : Localization.Current["DASH_Common"];
            }

            public class ShowUPS : Setting, IControlable
            {
                public ShowUPS(Dashboard dashboard) : base(dashboard) { }

                public Trigger EnterTrigger;

                public override IList<Request> GetBinds(Form form)
                {
                    return new Request[1] { EnterTrigger = new Trigger(form, panel =>
                    {
                        Form.ShowUPS = !Form.ShowUPS;
                        (panel as Form).Clear();
                    })};
                }

                public void OnKeyDown(byte key)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterTrigger.Pull();
                }

                public void OnKeyUp(byte key) { }

                public override string GetSettingKey() => Localization.Current["DASH_ShowUPS"];
                public override string GetSettingValue()
                {
                    var str = Form.ShowUPS ? Localization.Current["DASH_Enabled"] : Localization.Current["DASH_Disabled"];
                    return IsActive ? $"%=>Magenta%{str}%=>reset%": str;
                }
            }
        }

        public abstract class DashboardPanel : Panel
        {
            public DashboardPanel(Form form) : base(form) { }

            public abstract string PanelName { get; }
            public abstract bool LockChanger { get; }
        }

        public class SettingsPanel : DashboardPanel, IControlable
        {
            public override string PanelName => Localization.Current["DASH_PN_Settings"];
            public override bool LockChanger => ActiveSubwidget != null;
            public ListBox SettingsList;
            public Trigger EnterAction;
            public Trigger BackAction;
            public IControlable ActiveSubwidget;

            public SettingsPanel(Form form) : base(form) 
            {
                SettingsList = new ListBox();
                SettingsList.SelectingPadding = (-1, 0);
                SettingsList.SelectingLocation = ListBox.Location.Left;
                SettingsList.SelectingRelativeChildLocation = ListBox.RelativeLocation.Down;
                SettingsList.Container = new DynamicContainer()
                {
                    GetLeft = () => 1,
                    GetTop = () => 3,
                    GetHeight = () => Terminal.FixedWindowHeight - 5,
                    GetWidth = () => Terminal.FixedWindowWidth - 2,
                };
                SettingsList.Childs = typeof(Settings).GetNestedTypes().Where(x => x.BaseType == typeof(Settings.OnlyKeySetting) || x.BaseType == typeof(Settings.Setting) && !(x == typeof(Settings.OnlyKeySetting)))
                    .Select(x => new ListBox.ListItem(Activator.CreateInstance(x, new object[1] { form as Dashboard }) as Widget)).ToList();

                OnResizing += panel => SettingsList.Resize();

                Bind(SettingsList);
                foreach (var x in SettingsList.Childs)
                {
                    Bind(x.Child);
                    foreach (var req in x.Child.GetBinds(form))
                        Bind(req);
                }
                Bind(EnterAction = new Trigger(this, panel => (panel as SettingsPanel).Enter()));
                Bind(BackAction = new Trigger(this, panel => (panel as SettingsPanel).Back()));

                Optimize();
            }

            public void Enter()
            {
                ActiveSubwidget = SettingsList.Childs[SettingsList.Position].Child as IControlable;
                var setting = ActiveSubwidget as Settings.Setting;
                setting.OnEntered();
                setting.Redraw();
            }

            public void Back()
            {
                ActiveSubwidget = null;
                var setting = SettingsList.Childs[SettingsList.Position].Child as Settings.Setting;
                setting.OnLeaving();
                setting.Redraw();
            }

            public void OnKeyDown(byte key)
            {
                if (ActiveSubwidget == null)
                {
                    if (key == Key.E || key == Key.Enter)
                        EnterAction.Pull();
                    else SettingsList.OnKeyDown(key);
                }
                else
                {
                    if (key == Key.Tab || key == Key.Backspace)
                        BackAction.Pull();
                    else ActiveSubwidget.OnKeyDown(key);
                }
                
            }

            public void OnKeyUp(byte key)
            {
                if (ActiveSubwidget == null)
                    SettingsList.OnKeyUp(key);
                else ActiveSubwidget.OnKeyUp(key);
            }
        }

        public class FormPanel : DashboardPanel, IControlable
        {
            public override string PanelName => Localization.Current["DASH_PN_FormInfo"];
            public override bool LockChanger => false;
            public TriggerRedrawer OldFormInfoRedrawer;

            public FormPanel(Form form) : base(form)
            {
                Bind(OldFormInfoRedrawer = new TriggerRedrawer(form, panel =>
                {
                    var dashboard = (panel as Form);
                    var Reference = dashboard.Reference;

                    Terminal.Set(1, 4);
                    $"{Localization.Current["DASH_F_Current"]}: %=>Red%{Reference?.GetType().FullName ?? "null"}%=>reset%".PrintSuperText();
                    if (Reference != null)
                    {
                        var req = Reference.Requests;
                        Terminal.Set(4, 5);
                        $"{Localization.Current["DASH_F_StartedFrom"]}: %=>Red%{Reference.Reference?.GetType().FullName ?? "null"}%=>reset%".PrintSuperText();
                        Terminal.Set(4, 6);
                        $"{Localization.Current["DASH_F_FormTimer"]}: %=>DarkGreen%{Reference.FormTimer.TotalSeconds:0.##}%=>reset% {Localization.Current["DASH_F_sec"]}".PadRight(50).PrintSuperText();
                        Terminal.Set(4, 7);
                        $"{Localization.Current["DASH_F_Delay"]}: %=>DarkGreen%{Reference.RealUPS.TotalMilliseconds:0.##}%=>reset% {Localization.Current["DASH_F_msec"]}".PadRight(50).PrintSuperText();
                        Terminal.Set(4, 8);
                        $"{Localization.Current["DASH_F_Total"]}: %=>DarkGreen%{req.Count(x => x.AllRedraw)}%=>reset%/%=>DarkGreen%{req.Length}%=>reset%".PrintSuperText();
                        Terminal.Set(4, 9);
                        var childs = Reference.Root.GetChilds();
                        $"{Localization.Current["DASH_F_RootWidgets"]}: %=>DarkGreen%{childs.Length}%=>reset%".PrintSuperText();
                        var max = Math.Min(Terminal.FixedWindowHeight - 12, childs.Length);
                        for (var i = 0; i < max; i++)
                        {
                            Terminal.Set(7, 10 + i);
                            var type = childs[i].GetType();
                            var assembly = type.Assembly.GetName().Name;
                            var name = type.Name;
                            $"%=>Blue%[{assembly}]%=>reset% {name}".PrintSuperText();
                        }
                    }
                }));
                Optimize();
            }

            public void OnKeyDown(byte key)
            {
                
            }

            public void OnKeyUp(byte key)
            {

            }
        }

        public class PalettePanel : DashboardPanel, IControlable
        {
            public override string PanelName => Localization.Current["DASH_PN_Palette"];
            public override bool LockChanger => false;
            public TriggerRedrawer PaletteRedrawer;
            public string chars = Chars.MixingMultipliers.Keys.ToArray().JoinIntoString("");

            public PalettePanel(Form form) : base(form)
            {
                
                Bind(PaletteRedrawer = new TriggerRedrawer(form, panel =>
                {
                    Terminal.Set(1, 4);
                    Terminal.Write(Localization.Current["DASH_PaletteInfo"]);
                    Terminal.ResetColor();
                    var j = 0;
                    foreach (var x in chars.Batch(Terminal.FixedWindowWidth / 2 + Terminal.FixedWindowWidth / 3))
                    {
                        Terminal.Set(2, j + 6);
                        Terminal.Write(x.JoinIntoString(""));
                        j += 1;
                    }
                    Terminal.ResetColor();
                }));
                Optimize();
            }

            public void OnKeyDown(byte key)
            {

            }

            public void OnKeyUp(byte key)
            {

            }
        }

        public class VersionPanel : DashboardPanel, IControlable
        {
            private const string DeveloperNotSpecify = "#DNS#";
            #region KCore Version API

            public static Func<IKCoreVersionInfo> GetEntryVersion = () =>
            {
                var assembly = Assembly.GetEntryAssembly();
                var ret = new KCoreVersionInfo();
                var name = assembly.GetName();
                ret.Name = name.Name;
                ret.Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
                ret.Version = name.Version.ToString();
                ret.Differences = new KeyValuePair<string, string>[1] {
                    new KeyValuePair<string, string>(ret.Version.ToString(), DeveloperNotSpecify)
                };
                return ret;
            };
            public interface IKCoreVersionInfo
            {
                string Copyright { get; set; }
                KeyValuePair<string, string>[] Differences { get; set; }
                string Name { get; set; }
                string Version { get; set; }
            }

            #region non-public
            internal class KCoreVersionInfo : IKCoreVersionInfo
            {
                public string Name { get; set; }
                public string Copyright { get; set; }
                public string Version { get; set; }
                public KeyValuePair<string, string>[] Differences { get; set; }
            }

            private static KeyValuePair<string, string>[] kcore_diffs;

            static VersionPanel()
            {
                var kcore_ass = Assembly.GetExecutingAssembly();
                kcore_diffs = kcore_ass.GetManifestResourceNames().Where(x => x.StartsWith("KCore.docs.versions."))
                    .Select(x => new KeyValuePair<string, string>(x.Replace("KCore.docs.versions.", ""), kcore_ass.GetManifestResourceStream(x).ReadString())).ToArray();
            }

            internal static Func<IKCoreVersionInfo> GetKCoreVersion = () =>
            {
                return new KCoreVersionInfo()
                {
                    Name = KCoreVersion.Name,
                    Copyright = KCoreVersion.Copyright,
                    Version = Version.Version,
                    Differences = kcore_diffs, 
                };
            };
            #endregion
            #endregion

            public IKCoreVersionInfo KCoreVersionInformation;
            public IKCoreVersionInfo EntryVersionInformation;

            public string[] VersionContent;
            public ListBox VersionList;
            public ScrollableText VersionText;
            public Window VersionShow;
            public TriggerRedrawer MainInfoRedrawer;
            private Localization last_localization;

            public VersionPanel(Form form) : base(form)
            {
                KCoreVersionInformation = GetKCoreVersion();
                EntryVersionInformation = GetEntryVersion();

                VersionList = new ListBox();

                var initial_diffs = EntryVersionInformation.Differences.OrderByDescending(x => x.Key)
                    .Select(x => (x.Value, $"{EntryVersionInformation.Name} {x.Key}"))
                    .Concat(KCoreVersionInformation.Differences.OrderByDescending(x => x.Key)
                        .Select(x => (x.Value, $"{KCoreVersionInformation.Name} {x.Key}")));


                VersionContent = initial_diffs.Select(x => x.Item1).ToArray();
                VersionList.Childs =
                    initial_diffs.Select(x => x.Item2)
                    .Select(x => new ListBox.ListItem(new TextWidget(x, alignment: Alignment.LeftWidth | Alignment.UpHeight, fillHeight: false)))
                    .ToList();
                VersionList.SelectingPadding = (1, 1);

                var max = initial_diffs.Select(x => x.Item2).MaxBy(x => x).Length;
                VersionList.Container = new DynamicContainer()
                {
                    GetLeft = () => 1,
                    GetTop = () => 4,
                    GetWidth = () => max + 4,
                    GetHeight = () => 4,
                };
                var scroll = new VerticalScroll(VersionList, container: new DynamicContainer()
                {
                    GetLeft = () => max + 5,
                    GetTop = () => 4,
                    GetWidth = () => 1,
                    GetHeight = () => 4,
                }, pixel: ' ', scrollPixel: '▐');
                VersionList.Scroll = scroll;
                scroll.Resize();
                Bind(scroll);

                VersionList.Resize();
                VersionList.OnChanged += list_box =>
                {
                    VersionText.Text = VersionContent[VersionList.Position];
                    VersionShow.Resize();
                    VersionText.Redraw();
                };
                Bind(VersionList);

                VersionShow = new Window(
                    VersionText = new ScrollableText(VersionContent[0], textAlignment: TextAlignment.Left)
                );
                VersionShow.HasBorder = false;
                VersionShow.Padding = (0, 0, 0, 0);
                VersionText.Autoscroll = true;
                Bind(VersionText);

                VersionShow.Container = new DynamicContainer()
                {
                    GetLeft = () => 1,
                    GetTop = () => 9,
                    GetWidth = () => Terminal.FixedWindowWidth - 2,
                    GetHeight = () => Terminal.FixedWindowHeight - 10,
                };
                VersionShow.Resize();
                Bind(VersionShow);

                Bind(MainInfoRedrawer = new TriggerRedrawer(form, __ =>
                {
                    var cont = new StaticContainer(max + 8, 4, Terminal.FixedWindowWidth - max - 8, 2);
                    var cont2 = new StaticContainer(max + 8, 6, Terminal.FixedWindowWidth - max - 8, 2);

                    string.Format(Localization.Current["DASH_EntryBy"], EntryVersionInformation.Name, EntryVersionInformation.Version, EntryVersionInformation.Copyright, KCoreStorage.EntryLINK).PrintSuperText(cont);

                    string.Format(Localization.Current["DASH_KCoreBy"], KCoreVersionInformation.Name, KCoreVersionInformation.Version, KCoreVersionInformation.Copyright, KCoreStorage.KCoreLINK).PrintSuperText(cont2);
                }));

                Bind(new DelegateRequest(form, frm => VersionText.Text.StartsWith(DeveloperNotSpecify) || last_localization != Localization.Current, frm =>
                {
                    if (Localization.Current != last_localization && VersionContent[VersionList.Position].StartsWith(DeveloperNotSpecify))
                    {
                        VersionText.Text = VersionContent[VersionList.Position].Replace(DeveloperNotSpecify, string.Format(Localization.Current["DASH_VersionUndefined"], EntryVersionInformation.Name));
                    }
                    else VersionText.Text = VersionText.Text.Replace(DeveloperNotSpecify, string.Format(Localization.Current["DASH_VersionUndefined"], EntryVersionInformation.Name));
                    last_localization = Localization.Current;
                    VersionText.Resize();
                    VersionText.Redraw();
                }));

                Optimize();
            }

            public override string PanelName => Localization.Current["DASH_PN_Version"];

            public override bool LockChanger => false;

            public void OnKeyDown(byte key)
            {
                VersionList.OnKeyDown(key);
            }

            public void OnKeyUp(byte key)
            {

            }
        }

        public class PanelChanger : Widget, IControlable
        {
            public Dashboard Reference;
            public Trigger LeftTrigger;
            public Trigger RightTrigger;

            public PanelChanger(Dashboard form)
            {
                Reference = form;
                Container = new DynamicContainer()
                {
                    GetLeft = () => 0,
                    GetTop = () => 1,
                    GetWidth = () => Terminal.FixedWindowWidth,
                    GetHeight = () => 2
                };
            }

            public override IList<Request> GetBinds(Form form)
            {
                return new Request[]
                {
                    LeftTrigger = new Trigger(form, x => Prev()),
                    RightTrigger = new Trigger(form, x => Next()),
                };
            }


            public void Prev()
            {
                var ind = Array.FindIndex(Reference.Panels, x => Reference.current_panel == x);
                var xind = ind;
                ind--;
                if (ind < 0) ind = 0;
                if (xind != ind)
                {
                    Reference.CurrentPanel = Reference.Panels[ind] as IControlable;
                    Reference.Clear();
                }
            }
            public void Next()
            {
                var ind = Array.FindIndex(Reference.Panels, x => Reference.current_panel == x);
                var xind = ind;
                ind++;
                if (ind >= Reference.Panels.Length) ind = Reference.Panels.Length - 1;
                if (xind != ind)
                {
                    Reference.CurrentPanel = Reference.Panels[ind] as IControlable;
                    Reference.Clear();
                }
            }

            public override IContainer Container { get => TerminalContainer.This; set { } }
            public override int Width => Terminal.FixedWindowWidth;
            public override int Height => 1;
            public override void Resize() { }
            public override bool AlwaysClear => true;

            public override (int, int) Draw(int left, int top)
            {
                var indent = 0;
                Terminal.Set(0, 2);
                Terminal.Write(Graph.Chars('─', Terminal.FixedWindowWidth));
                for (var i = 0; i < Reference.Panels.Length; i++)
                {
                    var str = $"{Reference.Panels[i].PanelName}";
                    if (Reference.CurrentPanel == Reference.Panels[i])
                    {
                        Terminal.Set(1 + indent, 2);
                        Console.Write('┘' + Graph.Chars(' ', str.Length + 2) + '└');
                        str = $"│ {str} │";
                    }
                    else str = $"  {str}  ";
                    Terminal.Set(1 + indent, 1);
                    indent += str.Length + 2;
                    str.PrintSuperText();
                }
                return (left, top);
            }
            public override (int, int) Clear(int left, int top)
            {
                Graph.FillRectangle(Theme.Back, this);
                return (left, top);
            }

            public void OnKeyDown(byte key)
            {
                Reference.CurrentPanel.OnKeyDown(key);
                if ((Reference.CurrentPanel as DashboardPanel).LockChanger) return;
                if (key == Key.A || key == Key.LeftArrow)
                    LeftTrigger.Pull();
                else if (key == Key.D || key == Key.RightArrow)
                    RightTrigger.Pull();
            }
            public void OnKeyUp(byte key)
            {
                Reference.CurrentPanel.OnKeyUp(key);
            }
        }

        public PanelChanger Changer;

        public DashboardPanel[] Panels;

        public SettingsPanel FieldSettingsPanel;
        public FormPanel FieldFormPanel;
        public VersionPanel FieldVersionPanel;
        public PalettePanel FieldPalettePanel;

        internal IControlable current_panel;
        public IControlable CurrentPanel { get => current_panel; set
            {
                if (current_panel != null)
                {
                    var panel1 = (current_panel as Panel);
                    panel1.Visible = false;
                    panel1.Enabled = false;
                }
                current_panel = value;
                var panel2 = (current_panel as Panel);
                panel2.Visible = true;
                panel2.Enabled = true;
            } }

        public Dashboard()
        {
            Bind(Downline = new TriggerRedrawer(this, form =>
            {
                Terminal.Set(1, Terminal.FixedWindowHeight - 1);
                ($"KCore {Version.Version}").PrintSuperText();
            }));

            FieldSettingsPanel = new SettingsPanel(this);
            FieldFormPanel = new FormPanel(this);
            FieldVersionPanel = new VersionPanel(this);
            FieldPalettePanel = new PalettePanel(this);

            Panels = new DashboardPanel[] { FieldSettingsPanel, FieldFormPanel, FieldVersionPanel, FieldPalettePanel };
            foreach (var x in Panels)
            {
                Bind(x);
                if (x == FieldSettingsPanel) continue;
                x.Visible = false;
                x.Enabled = false;
            }
            Bind(Changer = new PanelChanger(this));
            ActiveWidget = Changer;
        }

        protected override void OnOpening()
        {
            CurrentPanel = FieldSettingsPanel;
            Clear();
        }

        protected override void OnResize()
        {
            FieldSettingsPanel.Resize();
        }

        protected override bool IsRecursiveForm() => true;

        protected override void OnAllRedraw()
        {
            var str = Localization.Current["KCore_DASH_Close"];
            Terminal.Set(Terminal.FixedWindowWidth - 1 - str.Length, Terminal.FixedWindowHeight - 1);
            Terminal.Write(str);
        }

        protected override void OnKeyDown(byte key)
        {
            base.OnKeyDown(key);
            if (key == Key.F12) Close();
            if (key == Key.OemPlus && Terminal.UpdatesPerSecond > 0)
            {
                Terminal.UpdatesPerSecond += 10;
                Downline.Pull();
            }
            if (key == Key.OemMinus && Terminal.UpdatesPerSecond > 10)
            {
                Terminal.UpdatesPerSecond -= 10;
                Downline.Pull();
            }
        }
    }
}
