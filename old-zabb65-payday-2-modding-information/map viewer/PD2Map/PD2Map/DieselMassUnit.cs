// -----------------------------------------------------------------------
// <copyright file="DieselMassUnit.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PD2Map
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public struct UnitPosition
    {
        public float[] Position;

        public float[] Rotation;
    }

    struct MassUnitHeader
    {
        public ulong UnitPathHash;

        public string UnitPath;

        public uint Offset;

        public uint InstanceCount;
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DieselMassUnit
    {

        public Dictionary<string, List<UnitPosition>> Instances = new Dictionary<string, List<UnitPosition>>(); 
        private static string BasePath;
        public DieselMassUnit(string basePath, string filePath)
        {
            BasePath = basePath;
            Load(filePath);
        }

        public void Load(string filePath)
        {
            using (var fs = new FileStream(BasePath + filePath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    var unitCount = br.ReadUInt32();
                    br.ReadUInt32(); // Unknown purpose.
                    var unitsOffset = br.ReadUInt32();
                    br.BaseStream.Seek((long)unitsOffset, SeekOrigin.Begin);
                    var headers = new List<MassUnitHeader>();
                    for (int i = 0; i < unitCount; ++i)
                    {
                        var header = new MassUnitHeader();
                        header.UnitPathHash = br.ReadUInt64();
                        header.UnitPath = KnownHashIndex.GetKnownValue(header.UnitPathHash);
                        br.ReadSingle(); // Unknown.
                        header.InstanceCount = br.ReadUInt32();
                        br.ReadUInt32(); // Unknown
                        header.Offset = br.ReadUInt32();
                        br.BaseStream.Seek(8, SeekOrigin.Current);
                        headers.Add(header);
                    }

                    foreach (var header in headers)
                    {
                        if (header.UnitPath == null)
                        {
                            Console.WriteLine("Massunit with id of {0:x} has no known path. Ignoring unit.", header.UnitPathHash);
                            continue;
                        }
                        var instances = new List<UnitPosition>();
                        br.BaseStream.Seek(header.Offset, SeekOrigin.Begin);
                        for (int i = 0; i < header.InstanceCount; ++i)
                        {
                            var instance = new UnitPosition();
                            instance.Position = new float[3];
                            instance.Rotation = new float[4];
                            instance.Position[0] = br.ReadSingle();
                            instance.Position[1] = br.ReadSingle();
                            instance.Position[2] = br.ReadSingle();
                            instance.Rotation[0] = br.ReadSingle();
                            instance.Rotation[1] = br.ReadSingle();
                            instance.Rotation[2] = br.ReadSingle();
                            instance.Rotation[3] = br.ReadSingle();
                            instances.Add(instance);
                        }
                        this.Instances[header.UnitPath] = instances;
                    }
                }
            }
        }
    }
}
