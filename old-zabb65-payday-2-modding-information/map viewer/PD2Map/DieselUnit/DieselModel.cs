namespace DieselUnit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;

    using global::DieselUnit.Utils;

    public class DieselModel
    {
        #region Fields

        private readonly string basePath;

        private readonly List<ModelDataModelSection> modelDataSections = new List<ModelDataModelSection>();

        private readonly Dictionary<int, ModelSection> modelSections = new Dictionary<int, ModelSection>();

        private static readonly Dictionary<int, Type> ModelSectionTypes = new Dictionary<int, Type>()
                                                                              {
                                                                                  {0x7AB072D3, typeof(GeometryModelSection)},
                                                                                  {0x4C507A13, typeof(TopologyModelSection)},
                                                                                  {0x0FFCD100, typeof(Object3DModelSection)},
                                                                                  {0x62212D88, typeof(ModelDataModelSection)},
                                                                                  {-475811382, typeof(PassthroughGPModelSection)},
                                                                                  {0x03B634BD, typeof(TopologyIPModelSection)}
                                                                              }; 

        private readonly string unitPath;

        #endregion

        #region Constructors and Destructors

        public DieselModel(string unitPath, string basePath)
        {
            this.unitPath = unitPath;
            this.basePath = basePath;
            this.Load();
        }

        #endregion

        #region Public Methods and Operators

        public ModelDataModelSection GetModelDataByName(string name)
        {
            UInt64 hash = Hash64.HashString(name);
            return this.modelDataSections.FirstOrDefault(ms => ms.Object3D.UniqueId == hash);
        }

        public Object3DModelSection GetObject3DByName(string name)
        {
            if (name == null)
            {
                Console.WriteLine("Attempted to get orientation object with null name.");
                return null;
            }
            UInt64 hash = Hash64.HashString(name);
            return (from object section in this.modelSections.Values where section.GetType() == typeof(Object3DModelSection) select (Object3DModelSection)section into realSection where realSection.UniqueId == hash select realSection).FirstOrDefault();
        }

        public List<ModelSection> GetObject3DModelSections()
        {
            return this.modelSections.Values.Where(section => section.GetType() == typeof(Object3DModelSection)).ToList();
        }

        public ModelSection GetModelSectionById(int sectionId)
        {
            return this.modelSections.ContainsKey(sectionId) ? this.modelSections[sectionId] : null;
        }

        private void Load()
        {
            using (var fs = new FileStream(this.basePath + this.unitPath + ".model", FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    int sectionCount = br.ReadInt32();
                    if (sectionCount == -1)
                    {
                        br.ReadInt32(); // File size, including header.
                        sectionCount = br.ReadInt32();
                    }
                    for (int currentSection = 0; currentSection < sectionCount; ++currentSection)
                    {
                        int sectionTag = br.ReadInt32();
                        int sectionId = br.ReadInt32();
                        int sectionSize = br.ReadInt32();
                        long sectionStart = br.BaseStream.Position;
                        this.ParseModelSection((int)sectionStart, sectionTag, sectionId, sectionSize, br);
                        br.BaseStream.Seek(sectionStart + sectionSize, SeekOrigin.Begin);
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void ParseModelSection(int offset, int sectionTag, int sectionId, int sectionSize, BinaryReader br)
        {
            if (ModelSectionTypes.ContainsKey(sectionTag))
            {
                var newSection = (ModelSection)Activator.CreateInstance(ModelSectionTypes[sectionTag]);
                newSection.Parse(br);
                newSection.FileOffset = offset;
                newSection.SectionId = sectionId;
                newSection.SectionTag = sectionTag;
                this.modelSections[sectionId] = newSection;
                if (sectionTag == (int)ModelSectionTag.ModelDataTag) this.modelDataSections.Add((ModelDataModelSection)newSection);
            }
        }

        #endregion
    }
}