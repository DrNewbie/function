using System;
using System.Collections.Generic;
using System.IO;

namespace DieselUnit
{

    /* List of known and documented model section tags.
     * The atual engine contains one of these for every data type that is knows how to serialize and deserialize.
     * There appear to be at least several dozen of these possible data types, each has a unique data layout and
     * serialization and deserialization routine.
     */
    public enum ModelSectionTag
    {
        GeometryTag = 0x7AB072D3,
        TopologyTag = 0x4C507A13,
        AnimationDataTag = 0x5DC011B8,
        BonesTag = 0x2EB43C77,
        SkinBonesTag = 0x65CC1825,
        AuthorTag = 0x7623C465,
        MaterialGroupTag = 0x29276B1D,
        MaterialTag = 0x3C54609C,
        Object3DTag = 0x0FFCD100,
        ModelDataTag = 0x62212D88,
        PassthroughGPTag = -475811382, //0xE3A3B1CA
        TopologyIPTag = 0x03B634BD,
        QuatLinearRotationControllerTag = 0x648A206C,
        QuatBezRotationControllerTag = 0x197345A5
    };

    /* Base model section class, stores the basic stuff about a model section. */
    public abstract class ModelSection
    {
        #region Members
        public int FileOffset;
        public int SectionTag;
        public int SectionId;
        #endregion
        public ModelSection() { }

        public abstract void Parse(BinaryReader br);
    }

    /* TopologyIP Section
     * TopologyIP sections contain only a field that points to an actual Topology section by section id.
     * Appears to be a means of abstraction for topology data.
     */
    public class TopologyIPModelSection : ModelSection
    {
        #region Members
        public int TopologySectionId = 0;
        #endregion
        #region Methods
        public TopologyIPModelSection() { }
        public override void Parse(BinaryReader br)
        {
            this.TopologySectionId = br.ReadInt32();
        }
        #endregion
    }
    /* PassthroughGP Section
     * PAssthroughGP is a geometry pointer section, that links to a geometry and topology section by section id.
     * Appears to be a means of abstraction for geometry and topology data.
     */
    public class PassthroughGPModelSection : ModelSection
    {
        #region Members
        public int GeometrySectionId = 0;
        public int TopologySectionId = 0;
        #endregion
        #region Methods
        public PassthroughGPModelSection() { }
        public override void Parse(BinaryReader br)
        {
            this.GeometrySectionId = br.ReadInt32();
            this.TopologySectionId = br.ReadInt32();
        }
        #endregion
    }

    /* Object3D Section
     * Object3D sections appear to be related to heirarchy and submodel positioning and rotations.
     * May link to another Object3D section by section id. If this link is a child or parent is unknown.
     * The uint64 in this section is most likely related to one of the strings located in the .object file.
     * Further research is needed on this section and what all of the fields mean.
     */
    public class Object3DModelSection : ModelSection
    {
        public float[,] RotationMatrix = new float[4,4];
        public UInt64 UniqueId;
        public float[] Position = new float[3];
        public int ParentSectionId;
        public Object3DModelSection() { }
        public override void Parse(BinaryReader br)
        {
            this.UniqueId = br.ReadUInt64();
            int somethingCount = br.ReadInt32();
            for (int i = 0; i < somethingCount; ++i)
            {
                br.ReadInt32();
                br.ReadInt32();
                br.ReadInt32(); //All unknown purpose.
            }
            for (int x = 0; x < 4; ++x)
            {
                for (int y = 0; y < 4; ++y)
                {
                    this.RotationMatrix[x, y] = br.ReadSingle();
                }
            }
            this.Position[0] = br.ReadSingle();
            this.Position[1] = br.ReadSingle();
            this.Position[2] = br.ReadSingle();
            this.ParentSectionId = br.ReadInt32();
        }
    }

    /* Model Section
     * Model(ModelData) sections contain model objects and link them to geometry data. Contains an inlined Object3D section.
     * This section also links to MaterialGroup, PassthroughGP, and TopologyIP sections by section id.
     * uint64 appears to be an idstring of the objects renderable name. Usually prefixed with s_ or g_. s_ being a shadow model, and g_ being a render model.
     * Appears to contain additonal position and or scaling data. Purpose is unknown.
     */
    public class ModelDataModelSection : ModelSection
    {
        public Object3DModelSection Object3D;
        public int Version;
        public int PassthroughGPSectionId;
        public int TopologyIPSectionId;
        public int MaterialGroupSectionId;
        public float[] BoundsLower = new float[3];
        public float[] BoundsUpper = new float[3];
        public override void Parse(BinaryReader br)
        {
            this.Object3D = new Object3DModelSection();
            this.Object3D.Parse(br);
            this.Version = br.ReadInt32();
            if (this.Version == 6)
            {
                this.BoundsLower[0] = br.ReadSingle();
                this.BoundsLower[1] = br.ReadSingle();
                this.BoundsLower[2] = br.ReadSingle();
                this.BoundsUpper[0] = br.ReadSingle();
                this.BoundsUpper[1] = br.ReadSingle();
                this.BoundsUpper[2] = br.ReadSingle();
                br.ReadInt32();
                br.ReadInt32();// Last two values unknown.
            }
            else // Appears to always be version 3.
            {
                this.PassthroughGPSectionId = br.ReadInt32();
                this.TopologyIPSectionId = br.ReadInt32();
                int count = br.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32(); //All unknown values.
                }
                br.ReadInt32(); // Unknown
                this.MaterialGroupSectionId = br.ReadInt32();
                br.ReadInt32(); // Unknown
                this.BoundsLower[0] = br.ReadSingle();
                this.BoundsLower[1] = br.ReadSingle();
                this.BoundsLower[2] = br.ReadSingle();
                this.BoundsUpper[0] = br.ReadSingle();
                this.BoundsUpper[1] = br.ReadSingle();
                this.BoundsUpper[2] = br.ReadSingle();
                br.ReadInt32();
                br.ReadInt32();
                br.ReadInt32(); //Unknown
            }
        }
    }

    /* Geometry Section
     * Geometry sections contain vertex buffers and associated tagging data for a submodel.
     * This section appears to be a serialized and raw dump of a hardware vertex buffer.
     * Data is packed into the vertex buffer in a series of sections described by the type header. Type header contains one or more data descriptors
     * that describe the data size, and data usage scenario(position/normal/uv) within the vertex buffer.
     * uint64 is likely another idstring, but it's linking is unknown.
     */
    public class GeometryModelSection : ModelSection
    {
        #region Contstants
        private static readonly uint[] TypeSizes = {0, 4, 8, 12, 16, 4, 4, 8, 12};
        enum GeometryType
        {
            Vertex = 1,
            Normal = 2,
            UV = 7,
            UnknownA = 15,
            UnknownB = 20, // Really unknown still
            UnknownC = 21 // Still unknown as well
        };
        struct GeometryTypeDef
        {
            public uint Size;
            public GeometryType Type;
        }
        #endregion
        #region Members
        public UInt32 BufferItems = 0; // 1 = verticies, 2 = normals, 4 = uvs, 8 = vertex colors, 16 = vertex_specular
        public Int32 ItemCount = 0;
        public float[] Vertices = null;
        public float[] Normals = null;
        public float[] UVs = null;
        public float[] VertexColors = null;
        public float[] VertexSpecular = null;
        #endregion
        #region Methods
        public GeometryModelSection() { }
        public override void Parse(BinaryReader br)
        {
            this.ItemCount = br.ReadInt32();
            uint typeCount = br.ReadUInt32();
            GeometryTypeDef[] types = new GeometryTypeDef[typeCount];
            for (uint i = 0; i < typeCount; ++i)
            {
                types[i].Size = TypeSizes[br.ReadUInt32()];
                types[i].Type = (GeometryType)br.ReadUInt32();
            }
            for (uint i = 0; i < typeCount; ++i)
            {
                switch (types[i].Type)
                {
                    case GeometryType.Vertex:
                        this.BufferItems |= 1;
                        this.Vertices = new float[this.ItemCount*3];
                        for (uint x = 0; x < 3*this.ItemCount; ++x)
                        {
                            this.Vertices[x] = br.ReadSingle();
                        }
                        break;
                    case GeometryType.Normal:
                        this.BufferItems |= 2;
                        this.Normals = new float[this.ItemCount*3];
                        for (uint x = 0; x < 3*this.ItemCount; ++x)
                        {
                            this.Normals[x] = br.ReadSingle();
                        }
                        break;
                    case GeometryType.UnknownB:
                        this.BufferItems |= 8;
                        this.VertexColors = new float[this.ItemCount*3];
                        for (uint x = 0; x < 3*this.ItemCount; ++x)
                        {
                            this.VertexColors[x] = br.ReadSingle();
                        }
                        break;
                    case GeometryType.UnknownC:
                        this.BufferItems |= 16;
                        this.VertexSpecular = new float[this.ItemCount * 3];
                        for (uint x = 0; x < 3 * this.ItemCount; ++x)
                        {
                            this.VertexSpecular[x] = br.ReadSingle();
                        }
                        break;
                    case GeometryType.UV:
                        this.BufferItems |= 4;
                        this.UVs = new float[2*this.ItemCount];
                        for (uint x = 0; x < 2*this.ItemCount; x+=2)
                        {
                            this.UVs[x] = br.ReadSingle();
                            this.UVs[x + 1] = -br.ReadSingle();
                        }
                        break;
                    default:
                        br.BaseStream.Seek(types[i].Size * this.ItemCount, SeekOrigin.Current);
                        break;
                }
            }
            br.ReadUInt64(); //Consume last unknown variable.
        }
        #endregion
    }

    /* Topology Section
     * Contains the indicies for a submodel, allowing it to be rendered. Indicies are of the short data type.
     * Secondary list has unknown purpose.
     * uint64 is of unknown purpose.
     */
    public class TopologyModelSection : ModelSection
    {
        #region Members
        public ushort[] Indicies;
        public int IndexCount = 0;
        #endregion
        #region Methods
        public TopologyModelSection() { }
        public override void Parse(BinaryReader br)
        {
            br.ReadUInt32(); // Unknown variable, appears to be unused in this version of the model format.
            this.IndexCount = br.ReadInt32();
            this.Indicies = new ushort[this.IndexCount];
            for (uint i = 0; i < this.IndexCount; ++i)
            {
                this.Indicies[i] = br.ReadUInt16();
            }
            int unknownCount = br.ReadInt32();
            br.ReadBytes(unknownCount);
            /*
            for (uint i = 0; i < unknown_count; ++i)
            {
                br.ReadByte();
            }
            */
            br.ReadUInt64(); //Consume last unknown variable.
        }
        #endregion
    }
}
