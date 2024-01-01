using KCore.Extensions;
using KCore.Graphics.Core;
using KCore.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using static KCore.Refactoring.KCoreDataFiles;

namespace KCore.Refactoring
{
    public interface IKCoreDataFile
    {
        FileInfo ReadedFileInfo { get; }
        KCoreFileSpecification Specification { get; }
        KCoreDataFiles.KCoreDataFile Data { get; }
    }

    public static class KCoreDataFile
    {
        public class PrimitiveFile : IKCoreDataFile
        {
            public FileInfo ReadedFileInfo { get; private set; }
            public PrimitiveSpecification Specification { get; private set; }
            public PrimitiveData Data { get; private set; }

            KCoreFileSpecification IKCoreDataFile.Specification => Specification;
            KCoreDataFiles.KCoreDataFile IKCoreDataFile.Data => Data;
        }
    }

    public static class KCoreDataFiles
    {

        public enum DataType
        {
            Unknown,
            Primitive,
            Charitive,
            Complexive,
            Videoxive,
        }


        public abstract class KCoreFileSpecification
        {
            public abstract DataType DataType { get; }
            public Version KCoreVersion { get; protected set; }
            public int DataFormatVersion { get; protected set; }

            internal static KCoreFileSpecification GetObject(DataType dataType)
            {
                switch (dataType)
                {
                    case DataType.Primitive: return new PrimitiveSpecification();
                }
                throw new ArgumentException("Unsupported", nameof(dataType));
            }
            internal static KCoreFileSpecification Read(Initial initial)
            {
                var ini = initial["Specification"];
                var dt = ini["Type"].ToEnum<DataType>();
                var ret = GetObject(dt);
                ret.KCoreVersion = new Version(ini["KCoreVersion"]);
                ret.DataFormatVersion = ini["DataFormatVersion"].ToInt32();
                ret.AdditionalRead(ini);
                return ret;
            }
            internal Initial Write()
            {
                var ret = new Initial();
                var ini = ret["Specification"];
                ini["Type"] = DataType.ToString();
                ini["KCoreVersion"] = global::KCoreVersion.Version;
                ini["DataFormatVersion"] = DataFormatVersion.ToString();
                AdditionalWrite(ini);
                return ret;
            }
            protected virtual void AdditionalRead(Initial.IniSection ini) { }
            protected virtual void AdditionalWrite(Initial.IniSection ini) { }
        }
        public abstract class KCoreDataFile
        {
            public byte[] Data { get; protected set; }
            internal static KCoreDataFile GetObject(DataType dataType)
            {
                switch (dataType)
                {
                    case DataType.Primitive: return new PrimitiveData();
                }
                throw new ArgumentException("Unsupported", nameof(dataType));
            }
        }
        public class UnknownSpecification : KCoreFileSpecification
        {
            public override DataType DataType => DataType.Unknown;
            internal UnknownSpecification() { }
        }
        public class PrimitiveSpecification : KCoreFileSpecification
        {
            public override DataType DataType => DataType.Primitive;
            public int Width { get; private set; }
            public int Height { get; private set; }
            protected override void AdditionalRead(Initial.IniSection ini)
            {
                Width = ini["Width"].ToInt32();
                Height = ini["Height"].ToInt32();
            }
            protected override void AdditionalWrite(Initial.IniSection ini)
            {
                ini["Width"] = Width.ToString();
                ini["Height"] = Height.ToString();
            }
        }
        public class PrimitiveData : KCoreDataFile
        {
            public Primitive Primitive { get; set; }
        }
    }
}
