using KCore.CoreForms;
using KCore.Extensions;
using KCore.Extensions.InsteadSLThree;
using KCore.Graphics;
using KCore.Graphics.Containers;
using KCore.Graphics.Widgets;
using KCore.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using static KCore.CoreForms.Dashboard.Settings;
using static KCore.Storage.KCoreStorage;

namespace KCore.Storage
{
    public static class KCoreStorage
    {
        private const string PortablePath = "KCore.portable";
        private const string RunInformationFile = "runinfo.ini";
        private const string ConfigPath = "KCore.ini";

        public const string KCoreLINK = "github.com/AIexandrKotov/KCore";
        public static string EntryLINK = "Undefined";

        public static string GetApplicationPath() => Path.Combine(KCorePath, "Applications", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title);
        public static string GetLocalConfigPath() => Path.Combine(GetApplicationPath(), ConfigPath);
        public static string GetGlobalConfigPath() => Path.Combine(KCorePath, ConfigPath);
        public static bool Portable { get; private set; }
        public static string KCorePath { get; private set; } = "";
        public static readonly string KCoreLibPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName);

        public class WelcomeForm : Form
        {
            public ListBox Actions;
            public Trigger EnterTrigger;
            public Trigger LanguageTrigger;

            #region Data

            public bool Installed = false;
            public Choise? Return = null;
            public string InstallationPath = null;
            public enum Choise
            {
                PortableMode,
                InstallationMode
            }
            #endregion

            private void ReinitActions()
            {
                if (Installed)
                {
                    Actions.Childs = new List<ListBox.ListItem>()
                    {
                        new ListBox.ListItem(new TextWidget(Localization.Current["Welcome_Next"], fillHeight: false)),
                        new ListBox.ListItem(new TextWidget(Localization.Current["Welcome_ForcePortable"], fillHeight: false)),
                    };
                }
                else
                {
                    Actions.Childs = new List<ListBox.ListItem>()
                    {
                        new ListBox.ListItem(new TextWidget(Localization.Current["Welcome_Portable"], fillHeight: false)),
                        new ListBox.ListItem(new TextWidget(Localization.Current["Welcome_Intall"], fillHeight: false)),
                    };
                }
                Actions.Container = new DynamicContainer()
                {
                    GetLeft = () => (Terminal.FixedWindowWidth - 40) / 2,
                    GetTop = () => 7,
                    GetHeight = () => 5,
                    GetWidth = () => 40
                };
                Actions.Resize();
            }

            public WelcomeForm(bool installed = false)
            {
                AllowedDashboard = false;

                Installed = installed;
                Actions = new ListBox();
                ReinitActions();
                

                ActiveWidget = Actions;
                Root.AddWidget(Actions);
                Bind(EnterTrigger = new Trigger(this, form => (form as WelcomeForm).Enter(Actions.Position)));
                Bind(LanguageTrigger = new Trigger(this, form =>
                {
                    Localization.Next();
                    (form as WelcomeForm).ReinitActions();
                    (form as Form).Clear();
                }));
            }

            public void Enter(int position)
            {
                if (Installed)
                {
                    switch (position)
                    {
                        case 0:
                            Close();
                            return;
                        case 1:
                            Return = Choise.PortableMode;
                            Close();
                            return;

                    }
                }
                else
                {
                    switch (position)
                    {
                        case 0:
                            Return = Choise.PortableMode;
                            Close();
                            return;
                        case 1:
                            InstallationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "KCore");
                            Return = Choise.InstallationMode;
                            Close();
                            return;

                    }
                }
            }

            private static void WelcomeRedraw(string start, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    Terminal.Set(0, 2 + i);
                    Localization.Current[start + i.ToString()]
                        .Replace("%Title%", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title)
                        .PrintSuperText(null, TextAlignment.Center);
                }
            }

            protected override void OnAllRedraw()
            {
                base.OnAllRedraw();
                if (Installed)
                    WelcomeRedraw("Welcome_I", 3);
                else
                    WelcomeRedraw("Welcome_U", 4);
                Terminal.Set(1, Terminal.FixedWindowHeight - 4);
                (Localization.Current["Welcome_Lang"] + $" ({Localization.Current.LocalName})").PrintSuperText();

                Terminal.Set(1, Terminal.FixedWindowHeight - 2);
                var entry_assembly = Assembly.GetEntryAssembly();
                var assembly_name = entry_assembly.GetName();
                var name = assembly_name.Name;
                var version = assembly_name.Version;
                var version_str = $"{version.Major}.{version.Minor}.{version.Build}";
                var entry_copyright = entry_assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
                string.Format(Localization.Current["Welcome_EntryBy"], name, version_str, entry_copyright, EntryLINK).PrintSuperText();

                Terminal.Set(1, Terminal.FixedWindowHeight - 1);
                string.Format(Localization.Current["Welcome_KCoreBy"], KCoreVersion.Name, KCoreVersion.VersionWithoutRevision, KCoreVersion.Copyright, KCoreLINK).PrintSuperText();
            }

            protected override void OnKeyDown(byte key)
            {
                if (key == Key.E || key == Key.Enter)
                    EnterTrigger.Pull();
                if (key == Key.LeftAlt || key == Key.RightAlt)
                    LanguageTrigger.Pull();
                base.OnKeyDown(key);
            }
        }

        public static void Install()
        {
            Update();
        }

        private static string[] directories = new string[]
        {
            "Localizations.",
        };

        public static void Update()
        {
            foreach (var x in directories)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(KCorePath, x.Replace(".", "\\"))));
            }
            foreach (var x in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.StartsWith("KCore.__.")))
            {
                var info = Assembly.GetExecutingAssembly().GetManifestResourceInfo(x);
                var sub_path = x.Replace("KCore.__.", "");
                var cur_dir = directories.FirstOrDefault(dir => sub_path.StartsWith(dir));
                if (cur_dir != null)
                    sub_path = sub_path.Replace(cur_dir, cur_dir.Replace(".", "\\"));
                using (var fileStream = File.Create(Path.Combine(KCorePath, sub_path)))
                {
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(x).CopyTo(fileStream);
                }
            }
        }

        public static void Run()
        {
            File.WriteAllText(Path.Combine(KCorePath, RunInformationFile), RunInformation.This.Write().ToIniText());
        }


        public abstract class Configuration
        {
            public abstract void Apply();
            public abstract Initial Save();
        }

        public class GlobalConfig : Configuration
        {
            public string Language;

            public GlobalConfig(bool init = true)
            {
                if (init)
                {
                    Language = Localization.Current.Name;
                }
            }

            public static GlobalConfig Read(Initial ini)
            {
                var ret = new GlobalConfig(false);
                var kcore = ini["KCore"];
                ret.Language = kcore["Language"];
                return ret;
            }

            public override void Apply()
            {
                var ind = Array.FindIndex(Localization.Localizations, x => x.Name == Language);
                if (ind != -1) Localization.Current = Localization.Localizations[ind];
            }

            public override Initial Save()
            {
                var ini = new Initial();
                var kcore = ini["KCore"];
                kcore["Language"] = Language;
                return ini;
            }
        }

        public class LocalConfig : Configuration
        {
            public bool AllowExternal;
            public (int, int) WindowSize;
            public int UpdatePerSecond;
            public bool TransitionAnimations;
            public int AnimationSpeed;

            public LocalConfig(bool init = true)
            {
                if (init)
                {
                    AllowExternal = Terminal.WindowSizeExternalManage;
                    WindowSize = (Terminal.FixedWindowWidth, Terminal.FixedWindowHeight);
                    UpdatePerSecond = Terminal.UpdatesPerSecond;
                    TransitionAnimations = GraphicsExtensions.AllowTransitionAnimations;
                    AnimationSpeed = GraphicsExtensions.TransitionAnimationsSpeed;
                }
            }

            public static LocalConfig Read(Initial ini)
            {
                var ret = new LocalConfig(false);
                var kcore = ini["KCore"];
                ret.AllowExternal = kcore["AllowExternal"].ToBool();
                var ws = kcore["WindowSize"].Split('x');
                ret.WindowSize = (ws[0].ToInt32(), ws[1].ToInt32());
                ret.UpdatePerSecond = kcore["UpdatePerSecond"].ToInt32();
                ret.TransitionAnimations = kcore["TransitionAnimations"].ToBool();
                ret.AnimationSpeed = kcore["AnimationSpeed"].ToInt32();
                return ret;
            }

            public override void Apply()
            {
                Terminal.WindowSizeExternalManage = false;
                (Terminal.FixedWindowWidth, Terminal.FixedWindowHeight) = WindowSize;
                Terminal.Resize();
                Terminal.WindowSizeExternalManage = AllowExternal;
                Terminal.UpdatesPerSecond = UpdatePerSecond;
                GraphicsExtensions.AllowTransitionAnimations = TransitionAnimations;
                GraphicsExtensions.TransitionAnimationsSpeed = AnimationSpeed;
            }

            public override Initial Save()
            {
                var ini = new Initial();
                var kcore = ini["KCore"];
                kcore["AllowExternal"] = Terminal.WindowSizeExternalManage.ToString();
                kcore["WindowSize"] = $"{Terminal.FixedWindowWidth}x{Terminal.FixedWindowHeight}";
                kcore["UpdatePerSecond"] = Terminal.UpdatesPerSecond.ToString();
                kcore["TransitionAnimations"] = GraphicsExtensions.AllowTransitionAnimations.ToString();
                kcore["AnimationSpeed"] = GraphicsExtensions.TransitionAnimationsSpeed.ToString();
                return ini;
            }
        }

        public class RunInformation
        {
            public static RunInformation This = Create();

            public int Revision;
            public Version Version;

            public Initial Write()
            {
                var ret = new Initial();
                var main = ret[""];

                main["Revision"] = Revision.ToString();
                main["Version"] = $"{Version.Major}.{Version.Minor}.{Version.Build}";

                return ret;
            }

            public static RunInformation Read(string path)
            {
                var ret = new RunInformation();

                var ini = Initial.FromIniText(File.ReadAllText(path));
                var main = ini[""];

                ret.Revision = int.Parse(main["Revision"]);
                var ver = main["Version"].Split('.').ConvertAll(x => int.Parse(x));
                ret.Version = new Version(ver[0], ver[1], ver[2]);

                return ret;
            }
            
            public static RunInformation Create()
            {
                return new RunInformation()
                {
                    Revision = Dashboard.Version.Revision,
                    Version = new Version(Dashboard.Version.Major, Dashboard.Version.Minor, Dashboard.Version.Build)
                };
            }
        }

        public static bool NewerInstalled { get; private set; }

        public static bool NeedToUpdate()
        {
            var ripath = Path.Combine(KCorePath, RunInformationFile);
            if (File.Exists(ripath))
            {
                var ri = RunInformation.Read(ripath);
                NewerInstalled = ri.Version > RunInformation.This.Version;
                return RunInformation.This.Revision > ri.Revision;
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ripath));
                File.WriteAllText(ripath, RunInformation.This.Write().ToIniText());
                return true;
            }
        }

        public static bool NeedToInstall { get; set; } = false;

        public static void InitAsPortable()
        {
            File.WriteAllText(Path.Combine(KCoreLibPath, PortablePath), "This file makes KCore portable");
            Portable = true;
            KCorePath = Path.Combine(KCoreLibPath, "KCore");
            NeedToInstall = true;
            Directory.CreateDirectory(GetApplicationPath());
        }
        public static void InitAsInstallation(string path)
        {
            Portable = false;
            KCorePath = path;
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Console", true).CreateSubKey("KCore").SetValue("Storage", KCorePath);
            NeedToInstall = true;
            Directory.CreateDirectory(GetApplicationPath());
        }

        private static bool inited = false;

        private static void Welcome(bool installed = false)
        {
            var welcome = new WelcomeForm(installed);
            welcome.Start();
            if (!installed)
            {
                if (welcome.Return == WelcomeForm.Choise.PortableMode)
                    InitAsPortable();
                else if (welcome.Return == WelcomeForm.Choise.InstallationMode)
                    InitAsInstallation(welcome.InstallationPath);
                else return;
            }
            else
            {
                if (welcome.Return == WelcomeForm.Choise.PortableMode)
                    InitAsPortable();
                else Directory.CreateDirectory(GetApplicationPath());
            }
        }

        public static void InitGlobalConfig()
        {
            var global_config = GetGlobalConfigPath();
            if (File.Exists(global_config))
                GlobalConfig.Read(Initial.FromIniText(File.ReadAllText(global_config))).Apply();
            else SaveGlobalConfig();
        }

        public static void InitLocalConfig()
        {
            var local_config = GetLocalConfigPath();
            if (File.Exists(local_config))
                LocalConfig.Read(Initial.FromIniText(File.ReadAllText(local_config))).Apply();
            else SaveLocalConfig();
        }

        public static void SaveGlobalConfig()
        {
            File.WriteAllText(GetGlobalConfigPath(), new GlobalConfig().Save().ToIniText());
        }

        public static void SaveLocalConfig()
        {
            File.WriteAllText(GetLocalConfigPath(), new LocalConfig().Save().ToIniText());
        }

        public static void Init()
        {
            if (inited) return;
            Localization.InitDefault();
            inited = true;
            Portable = File.Exists(Path.Combine(KCoreLibPath, PortablePath));
            if (Portable)
            {
                KCorePath = Path.Combine(KCoreLibPath, "KCore");
                if (NeedToUpdate())
                    Update();
                InitGlobalConfig();
                Localization.InitStorage();
                InitLocalConfig();
                if (!NewerInstalled) Run();
            }
            else
            {
                var kcore = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Console").OpenSubKey("KCore");
                if (kcore == null)
                {
                    Welcome();
                    if (NeedToInstall)
                        Install();
                    else if (NeedToUpdate())
                        Update();
                    InitGlobalConfig();
                    Localization.InitStorage();
                    InitLocalConfig();
                    if (!NewerInstalled) Run();
                }
                else
                {
                    KCorePath = kcore.GetValue("Storage").Cast<string>();
                    if (!Directory.Exists(KCorePath))
                    {
                        Welcome();
                        if (NeedToInstall)
                            Install();
                        else if (NeedToUpdate())
                            Update();
                        if (!NewerInstalled) Run();
                        InitGlobalConfig();
                        Localization.InitStorage();
                        InitLocalConfig();
                    }
                    else if (!Directory.Exists(GetApplicationPath()))
                    {
                        if (NeedToUpdate())
                            Update();
                        InitGlobalConfig();
                        Localization.InitStorage();
                        if (!NewerInstalled) Run();
                        Welcome(true);
                        InitLocalConfig();
                    }
                    else
                    {
                        if (NeedToUpdate())
                            Update();
                        InitGlobalConfig();
                        Localization.InitStorage();
                        InitLocalConfig();
                        if (!NewerInstalled) Run();
                    }
                }
            }
        }

    }
}
