using System.IO;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KCore.Extensions.InsteadSLThree;

// Общие сведения об этой сборке предоставляются следующим набором
// набора атрибутов. Измените значения этих атрибутов для изменения сведений,
// связанных со сборкой.
[assembly: AssemblyTitle(EditorVersion.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(EditorVersion.Name)]
[assembly: AssemblyCopyright(EditorVersion.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// COM, следует установить атрибут ComVisible в TRUE для этого типа.
[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("7450c379-dd7c-410e-a122-a96ef8082f37")]

// Сведения о версии сборки состоят из указанных ниже четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии
//      Номер сборки
//      Редакция
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(EditorVersion.Version)]
[assembly: AssemblyFileVersion(EditorVersion.Version)]

public static class EditorVersion
{
    public class Reflected
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }
        public DateTime LastUpdate { get; private set; }

        public string Version { get; private set; }
        public string VersionWithoutRevision { get; private set; }

        public Reflected()
        {
            var slt_version = typeof(EditorVersion);
            Major = int.Parse(slt_version.GetField("Major").GetValue(null).Cast<string>());
            Minor = int.Parse(slt_version.GetField("Minor").GetValue(null).Cast<string>());
            Build = int.Parse(slt_version.GetField("Build").GetValue(null).Cast<string>());
            Revision = int.Parse(slt_version.GetField("Revision").GetValue(null).Cast<string>());
            LastUpdate = new DateTime(slt_version.GetField("LastUpdate").GetValue(null).Cast<long>());

            Version = Major + "." + Minor + "." + Build + "." + Revision;
            VersionWithoutRevision = Major + "." + Minor + "." + Build;
        }
    }

    public const string Major = "0"; //vh
    public const string Minor = "1"; //vh
    public const string Build = "0"; //vh
    public const string Revision = "53"; //vh
    public const long LastUpdate = 638396998313907149; //vh

    public const string Version = Major + "." + Minor + "." + Build + "." + Revision;
    public const string VersionWithoutRevision = Major + "." + Minor + "." + Build;
    public const string VersionRevisionInBrackets = Major + "." + Minor + "." + Build + " (" + Revision + ")";
    public const string Name = "KCore Editor";
    public const string Author = "Alexandr Kotov";
    public const string Copyright = Author + " 2023";

    internal static string[] ReadStrings(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
        }
    }

    static EditorVersion()
    {
        var ass = Assembly.GetExecutingAssembly();

    }
}
