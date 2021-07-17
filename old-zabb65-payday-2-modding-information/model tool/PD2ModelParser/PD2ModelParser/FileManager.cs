//Original code by PoueT

using Nexus;
using PD2Bundle;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    class Hash64
    {
        [DllImport("hash64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong Hash(byte[] k, ulong length, ulong level);
        public static ulong HashString(string input, ulong level = 0)
        {
            return Hash(UTF8Encoding.UTF8.GetBytes(input), (ulong)UTF8Encoding.UTF8.GetByteCount(input), level);
        }
    }

    /// <summary>
    ///     The static storage.
    /// </summary>
    public static class StaticStorage
    {
        #region Static Fields
        /// <summary>
        ///     The known index.
        /// </summary>
        public static KnownIndex hashindex = new KnownIndex();
        public static uint rp_id = 0;
        public static List<String> objects_list = new List<String>();

        #endregion
    }

    public class FileManager
    {
        public static uint animation_data_tag = 0x5DC011B8; // Animation data
        public static uint author_tag = 0x7623C465; // Author tag
        public static uint material_group_tag = 0x29276B1D; // Material Group
        public static uint material_tag = 0x3C54609C; // Material
        public static uint object3D_tag = 0x0FFCD100; // Object3D
        public static uint model_data_tag = 0x62212D88; // Model data
        public static uint geometry_tag = 0x7AB072D3; // Geometry
        public static uint topology_tag = 0x4C507A13; // Topology
        public static uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public static uint topologyIP_tag = 0x03B634BD;  // TopologyIP
        public static uint quatLinearRotationController_tag = 0x648A206C; // QuatLinearRotationController
        public static uint quatBezRotationController_tag = 0x197345A5; // QuatBezRotationController
        public static uint skinbones_tag = 0x65CC1825; // SkinBones
        public static uint bones_tag = 0xEB43C77; // Bones
        public static uint light_tag = 0xFFA13B80; //Light
        public static uint lightSet_tag = 0x33552583; //LightSet
        public static uint linearVector3Controller_tag = 0x26A5128C; //LinearVector3Controller
        public static uint linearFloatController_tag = 0x76BF5B66; //LinearFloatController
        public static uint lookAtConstrRotationController = 0x679D695B; //LookAtConstrRotationController
        public static uint camera_tag = 0x46BF31A7; //Camera


        public List<SectionHeader> sections = new List<SectionHeader>();
        public Dictionary<UInt32, object> parsed_sections = new Dictionary<UInt32, object>();
        public byte[] leftover_data = null;


        public void Open(string filepath, string rp = null)
        {
            StaticStorage.hashindex.Load();

            Console.WriteLine("Loading: " + filepath);

            uint offset = 0;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int random = br.ReadInt32();
                    offset += 4;
                    int filesize = br.ReadInt32();
                    offset += 4;
                    int sectionCount;
                    if (random == -1)
                    {
                        sectionCount = br.ReadInt32();
                        offset += 4;
                    }
                    else
                        sectionCount = random;

                    Console.WriteLine("Size: " + filesize + " bytes, Sections: " + sectionCount);

                    for (int x = 0; x < sectionCount; x++)
                    {
                        SectionHeader sectionHead = new SectionHeader(br);
                        sections.Add(sectionHead);
                        Console.WriteLine(sectionHead);
                        offset += sectionHead.size + 12;
                        Console.WriteLine("Next offset: " + offset);
                        fs.Position = (long)offset;
                    }


                    foreach (SectionHeader sh in sections)
                    {
                        object section = new object();

                        fs.Position = sh.offset + 12;

                        if (sh.type == animation_data_tag)
                        {
                            Console.WriteLine("Animation Tag at " + sh.offset + " Size: " + sh.size);
                            
                            section = new Animation(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == author_tag)
                        {
                            Console.WriteLine("Author Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Author(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == material_group_tag)
                        {
                            Console.WriteLine("Material Group Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Material_Group(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == material_tag)
                        {
                            Console.WriteLine("Material Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Material(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == object3D_tag)
                        {
                            Console.WriteLine("Object 3D Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Object3D(br, sh);

                            if ((section as Object3D).hashname == 4921176767231919846)
                                Console.WriteLine();

                            Console.WriteLine(section);
                        }
                        else if (sh.type == geometry_tag)
                        {
                            Console.WriteLine("Geometry Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Geometry(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == model_data_tag)
                        {
                            Console.WriteLine("Model Data Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Model(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == topology_tag)
                        {
                            Console.WriteLine("Topology Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Topology(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == passthroughGP_tag)
                        {
                            Console.WriteLine("passthroughGP Tag at " + sh.offset + " Size: " + sh.size);

                            section = new PassthroughGP(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == topologyIP_tag)
                        {
                            Console.WriteLine("TopologyIP Tag at " + sh.offset + " Size: " + sh.size);

                            section = new TopologyIP(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == bones_tag)
                        {
                            Console.WriteLine("Bones Tag at " + sh.offset + " Size: " + sh.size);

                            section = new Bones(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == skinbones_tag)
                        {
                            Console.WriteLine("SkinBones Tag at " + sh.offset + " Size: " + sh.size);

                            section = new SkinBones(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == quatLinearRotationController_tag)
                        {
                            Console.WriteLine("QuatLinearRotationController Tag at " + sh.offset + " Size: " + sh.size);

                            section = new QuatLinearRotationController(br, sh);

                            Console.WriteLine(section);
                        }
                        else if (sh.type == linearVector3Controller_tag)
                        {
                            Console.WriteLine("QuatLinearRotationController Tag at " + sh.offset + " Size: " + sh.size);

                            section = new LinearVector3Controller(br, sh);

                            Console.WriteLine(section);
                        }
                        else
                        {
                            Console.WriteLine("UNKNOWN Tag at " + sh.offset + " Size: " + sh.size);
                            fs.Position = sh.offset;

                            section = new Unknown(br, sh);

                            Console.WriteLine(section);
                        }

                        parsed_sections.Add(sh.id, section);
                    }

                    if (fs.Position < fs.Length)
                        leftover_data = br.ReadBytes((int)(fs.Length - fs.Position));

                    br.Close();
                }
                fs.Close();
            }

            if (rp != null)
            {
                this.updateRP(rp);
            }


            //Generate outinfo.txt - Used for research and debug purposes
            using (FileStream fs = new FileStream("outinfo.txt", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == passthroughGP_tag)
                        {
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[sectionheader.id];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];
                            sw.WriteLine("Object ID: " + sectionheader.id);
                            sw.WriteLine("Verts (x, z, y)");
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine(vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture) + " " + (-vert.Y).ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("UV (u, -v)");
                            foreach (Vector2D uv in geometry_section.uvs)
                            {
                                sw.WriteLine(uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + uv.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("Normals (i, j, k)");

                            //Testing
                            List<Vector3D> norm_tangents = new List<Vector3D>();
                            List<Vector3D> norm_binorms = new List<Vector3D>();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine(norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));


                                Vector3D norm_t;
                                Vector3D binorm;

                                Vector3D t1 = Vector3D.Cross(norm, Vector3D.Right);
                                Vector3D t2 = Vector3D.Cross(norm, Vector3D.Forward);

                                if (t1.Length() > t2.Length())
                                    norm_t = t1;
                                else
                                    norm_t = t2;
                                norm_t.Normalize();
                                norm_tangents.Add(norm_t);

                                binorm = Vector3D.Cross(norm, norm_t);
                                binorm = new Vector3D(binorm.X*-1.0f, binorm.Y*-1.0f, binorm.Z*-1.0f);
                                binorm.Normalize();
                                norm_binorms.Add(binorm);

                            }

                            if (norm_binorms.Count > 0 && norm_tangents.Count > 0)
                            {
                                Vector3D[] arranged_unknown20 = norm_binorms.ToArray();
                                Vector3D[] arranged_unknown21 = norm_tangents.ToArray();

                                for (int fcount = 0; fcount < topology_section.facelist.Count; fcount++)
                                {
                                    Face f = topology_section.facelist[fcount];

                                    //unknown_20
                                    arranged_unknown20[f.x] = norm_binorms[topology_section.facelist[fcount].x];
                                    arranged_unknown20[f.y] = norm_binorms[topology_section.facelist[fcount].y];
                                    arranged_unknown20[f.z] = norm_binorms[topology_section.facelist[fcount].z];

                                    //unknown_21
                                    arranged_unknown21[f.x] = norm_tangents[topology_section.facelist[fcount].x];
                                    arranged_unknown21[f.y] = norm_tangents[topology_section.facelist[fcount].y];
                                    arranged_unknown21[f.z] = norm_tangents[topology_section.facelist[fcount].z];

                                }
                                norm_binorms = arranged_unknown20.ToList();
                                norm_tangents = arranged_unknown21.ToList();
                            }

                            sw.WriteLine("Pattern UVs (u, v)");
                            foreach (Vector2D pattern_uv_entry in geometry_section.pattern_uvs)
                            {
                                sw.WriteLine(pattern_uv_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + pattern_uv_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            int unk20tangcount = 0;
                            int unk21tangcount = 0;
                            sw.WriteLine("Unknown 20 (float, float, float) - Normal tangents???");
                            foreach (Vector3D unknown_20_entry in geometry_section.unknown20)
                            {
                                sw.WriteLine(unknown_20_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_20_entry.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                Vector3D normt = norm_tangents[unk20tangcount];
                                sw.WriteLine("* " + normt.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                unk20tangcount++;
                            }
                            sw.WriteLine("Unknown 21 (float, float, float) - Normal tangents???");
                            foreach (Vector3D unknown_21_entry in geometry_section.unknown21)
                            {
                                sw.WriteLine(unknown_21_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_21_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_21_entry.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                Vector3D normt = norm_binorms[unk21tangcount];
                                sw.WriteLine("* " + normt.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + normt.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                                unk21tangcount++;
                            }

                            sw.WriteLine("Unknown 15 (float, float) - Weights???");
                            foreach (GeometryWeightGroups unknown_15_entry in geometry_section.weight_groups)
                            {
                                sw.WriteLine(unknown_15_entry.Bones1.ToString() + " " + unknown_15_entry.Bones2.ToString() + " " + unknown_15_entry.Bones3.ToString() + " " + unknown_15_entry.Bones4.ToString());
                            
                                //sw.WriteLine(unknown_15_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_15_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            sw.WriteLine("Unknown 17 (float, float, float) - Weights???");
                            foreach (Vector3D unknown_17_entry in geometry_section.weights)
                            {
                                sw.WriteLine(unknown_17_entry.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + unknown_17_entry.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }

                            foreach (Byte[] gunkid in geometry_section.unknown_item_data)
                            {
                                if (gunkid.Length % 8 == 0)
                                {
                                    sw.WriteLine("Unknown X (float, float)");
                                    for (int x = 0; x < gunkid.Length; )
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else if (gunkid.Length % 12 == 0)
                                {
                                    sw.WriteLine("Unknown X (float, float, float)");
                                    for (int x = 0; x < gunkid.Length; )
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture) + " ");
                                        x += 4;
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else if (gunkid.Length % 4 == 0)
                                {
                                    sw.WriteLine("Unknown X (float)");
                                    for (int x = 0; x < gunkid.Length; )
                                    {
                                        sw.Write(BitConverter.ToSingle(gunkid, x).ToString("0.000000", CultureInfo.InvariantCulture));
                                        x += 4;
                                        sw.WriteLine();

                                    }
                                }
                                else
                                {
                                    sw.Write("Something else... [for debugging]");
                                }
                            }

                            sw.WriteLine("Faces (f1, f2, f3)");
                            foreach (Face face in topology_section.facelist)
                            {
                                sw.WriteLine((face.x + 1) + " " + (face.y + 1) + " " + (face.z + 1));
                            }

                            sw.WriteLine();
                            geometry_section.PrintDetailedOutput(sw);
                            sw.WriteLine();
                        }

                    }

                    sw.Close();
                }
                fs.Close();
            }


            //Generate obj
            ushort maxfaces = 0;
            UInt32 uvcount = 0;
            UInt32 normalcount = 0;
            string newfolder = "";//@"c:/Program Files (x86)/Steam/SteamApps/common/PAYDAY 2/models/";

            Directory.CreateDirectory(Path.GetDirectoryName((newfolder + filepath).Replace(".model", ".obj")));

            using (FileStream fs = new FileStream((newfolder + filepath).Replace(".model", ".obj"), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == model_data_tag)
                        {
                            Model model_data = (Model)parsed_sections[sectionheader.id];
                            if (model_data.version == 6)
                                continue;
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data.passthroughGP_ID];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];



                            sw.WriteLine("#");
                            sw.WriteLine("# object " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine("v " + vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.verts.Count + " vertices");
                            sw.WriteLine();

                            foreach (Vector2D uv in geometry_section.uvs)
                            {
                                sw.WriteLine("vt " + uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + uv.Y.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.uvs.Count + " UVs");
                            sw.WriteLine();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine("vn " + norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.normals.Count + " Normals");
                            sw.WriteLine();

                            sw.WriteLine("g " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.x + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.y + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.uvs.Count > 0)
                                    sw.Write((uvcount + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.z + 1));

                                sw.WriteLine();
                            }
                            sw.WriteLine("# " + topology_section.facelist.Count + " Faces");
                            sw.WriteLine();

                            maxfaces += (ushort)geometry_section.verts.Count;
                            uvcount += (ushort)geometry_section.uvs.Count;
                            normalcount += (ushort)geometry_section.normals.Count;
                        }
                    }

                    sw.Close();
                }
                fs.Close();
            }

            //Pattern UV
            maxfaces = 0;
            uvcount = 0;
            normalcount = 0;

            Directory.CreateDirectory(Path.GetDirectoryName((newfolder + filepath).Replace(".model", "_pattern_uv.obj")));

            using (FileStream fs = new FileStream((newfolder + filepath).Replace(".model", "_pattern_uv.obj"), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (SectionHeader sectionheader in sections)
                    {
                        if (sectionheader.type == model_data_tag)
                        {
                            Model model_data = (Model)parsed_sections[sectionheader.id];
                            if (model_data.version == 6)
                                continue;
                            PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data.passthroughGP_ID];
                            Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                            Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];



                            sw.WriteLine("#");
                            sw.WriteLine("# object " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            sw.WriteLine("#");
                            sw.WriteLine();
                            sw.WriteLine("o " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Vector3D vert in geometry_section.verts)
                            {
                                sw.WriteLine("v " + vert.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + vert.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.verts.Count + " vertices");
                            sw.WriteLine();

                            foreach (Vector2D uv in geometry_section.pattern_uvs)
                            {
                                sw.WriteLine("vt " + uv.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + (-uv.Y).ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.pattern_uvs.Count + " UVs");
                            sw.WriteLine();

                            foreach (Vector3D norm in geometry_section.normals)
                            {
                                sw.WriteLine("vn " + norm.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Y.ToString("0.000000", CultureInfo.InvariantCulture) + " " + norm.Z.ToString("0.000000", CultureInfo.InvariantCulture));
                            }
                            sw.WriteLine("# " + geometry_section.normals.Count + " Normals");
                            sw.WriteLine();

                            sw.WriteLine("g " + StaticStorage.hashindex.GetString(model_data.object3D.hashname));
                            foreach (Face face in topology_section.facelist)
                            {
                                //x
                                sw.Write("f " + (maxfaces + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.x + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.x + 1));

                                //y
                                sw.Write(" " + (maxfaces + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.y + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.y + 1));

                                //z
                                sw.Write(" " + (maxfaces + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.pattern_uvs.Count > 0)
                                    sw.Write((uvcount + face.z + 1));
                                sw.Write('/');
                                if (geometry_section.normals.Count > 0)
                                    sw.Write((normalcount + face.z + 1));

                                sw.WriteLine();
                            }
                            sw.WriteLine("# " + topology_section.facelist.Count + " Faces");
                            sw.WriteLine();

                            maxfaces += (ushort)geometry_section.verts.Count;
                            uvcount += (ushort)geometry_section.pattern_uvs.Count;
                            normalcount += (ushort)geometry_section.normals.Count;
                        }
                    }

                    sw.Close();
                }
                fs.Close();
            }



        }

        public bool ImportNewObjPatternUV(String filepath)
        {
            Console.WriteLine("Importing new obj with file for UV patterns: " + filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();

            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        obj_data obj = new obj_data();
                        bool reading_faces = false;
                        int prevMaxVerts = 0;
                        int prevMaxUvs = 0;
                        int prevMaxNorms = 0;


                        while ((line = sr.ReadLine()) != null)
                        {
                            
                            //preloading objects
                            if (line.StartsWith("#"))
                                continue;
                            else if (line.StartsWith("o ") || line.StartsWith("g "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                obj.object_name = line.Substring(2);
                            }
                            else if (line.StartsWith("usemtl ") )
                            {
                                obj.material_name = line.Substring(2);
                            }
                            else if (line.StartsWith("v "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] verts = line.Replace("  ", " ").Split(' ');
                                Vector3D vert = new Vector3D();
                                vert.X = Convert.ToSingle(verts[1], CultureInfo.InvariantCulture);
                                vert.Y = Convert.ToSingle(verts[2], CultureInfo.InvariantCulture);
                                vert.Z = Convert.ToSingle(verts[3], CultureInfo.InvariantCulture);

                                obj.verts.Add(vert);
                            }
                            else if (line.StartsWith("vt "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] uvs = line.Split(' ');
                                Vector2D uv = new Vector2D();
                                uv.X = Convert.ToSingle(uvs[1], CultureInfo.InvariantCulture);
                                uv.Y = Convert.ToSingle(uvs[2], CultureInfo.InvariantCulture);

                                obj.uv.Add(uv);
                            }
                            else if (line.StartsWith("vn "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] norms = line.Split(' ');
                                Vector3D norm = new Vector3D();
                                norm.X = Convert.ToSingle(norms[1], CultureInfo.InvariantCulture);
                                norm.Y = Convert.ToSingle(norms[2], CultureInfo.InvariantCulture);
                                norm.Z = Convert.ToSingle(norms[3], CultureInfo.InvariantCulture);

                                obj.normals.Add(norm);
                            }
                            else if (line.StartsWith("f "))
                            {
                                reading_faces = true;
                                String[] faces = line.Substring(2).Split(' ');
                                for (int x = 0; x < 3; x++)
                                {
                                    Face face = new Face();
                                    if (obj.verts.Count > 0)
                                        face.x = (ushort)(Convert.ToUInt16(faces[x].Split('/')[0]) - prevMaxVerts - 1);
                                    if (obj.uv.Count > 0)
                                        face.y = (ushort)(Convert.ToUInt16(faces[x].Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        face.z = (ushort)(Convert.ToUInt16(faces[x].Split('/')[2]) - prevMaxNorms - 1);
                                    if (face.x < 0 || face.y < 0 || face.z < 0)
                                        throw new Exception();
                                    obj.faces.Add(face);
                                }

                            }
                        }

                        if (!objects.Contains(obj))
                            objects.Add(obj);

                    }
                }
                            
                

                //Read each object
                foreach(obj_data obj in objects)
                {

                    //Locate the proper model
                    uint modelSectionid = 0;
                    foreach (KeyValuePair<uint, object> pair in parsed_sections)
                    {
                        if (modelSectionid != 0)
                            break;

                        if(pair.Value is Model)
                        {
                            UInt64 tryp;
                            if (UInt64.TryParse(obj.object_name, out tryp))
                            {
                                if (tryp == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                            else
                            {
                                if (Hash64.HashString(obj.object_name) == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                        }
                    }

                    //Apply new changes
                    if (modelSectionid == 0)
                        continue;

                    Model model_data_section = (Model)parsed_sections[modelSectionid];
                    PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data_section.passthroughGP_ID];
                    Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                    Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];

                    //Arrange UV and Normals
                    Vector2D[] new_arranged_UV = new Vector2D[geometry_section.verts.Count];
                    for (int x = 0; x < new_arranged_UV.Length; x++)
                        new_arranged_UV[x] = new Vector2D(100f, 100f);
                    Vector2D sentinel = new Vector2D(100f, 100f);

                    if (topology_section.facelist.Count != obj.faces.Count/3)
                        return false;

                    for (int fcount = 0; fcount < topology_section.facelist.Count; fcount += 3)
                    {
                        Face f1 = obj.faces[fcount+0];
                        Face f2 = obj.faces[fcount+1];
                        Face f3 = obj.faces[fcount+2];

                        //UV
                        if (obj.uv.Count > 0)
                        {
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].x].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].x] = obj.uv[f1.y];
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].y].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].y] = obj.uv[f2.y];
                            if (new_arranged_UV[topology_section.facelist[fcount / 3 + 0].z].Equals(sentinel))
                                new_arranged_UV[topology_section.facelist[fcount / 3 + 0].z] = obj.uv[f3.y];
                        }
                    }



                    geometry_section.pattern_uvs = new_arranged_UV.ToList();

                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).pattern_uvs = new_arranged_UV.ToList();
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }

        public bool updateRP(string rp)
        {
            ulong rp_hash = Hash64.HashString(rp);
            foreach (object section in this.parsed_sections.Values)
            {
                if (section is Object3D)
                {
                    if ((section as Object3D).hashname == rp_hash)
                    {
                        StaticStorage.rp_id = (section as Object3D).id;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ImportNewObj(String filepath, bool addNew = false)
        {
            Console.WriteLine("Importing new obj with file: " + filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();
            List<obj_data> toAddObjects = new List<obj_data>();


            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        obj_data obj = new obj_data();
                        bool reading_faces = false;
                        int prevMaxVerts = 0;
                        int prevMaxUvs = 0;
                        int prevMaxNorms = 0;
                        string current_shade_group = null;

                        while ((line = sr.ReadLine()) != null)
                        {
                            
                            //preloading objects
                            if (line.StartsWith("#"))
                                continue;
                            else if (line.StartsWith("o ") || line.StartsWith("g "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                    current_shade_group = null;
                                }

                                if (String.IsNullOrEmpty( obj.object_name ))
                                {
                                    obj.object_name = line.Substring(2);
                                    Console.WriteLine("Object " + (objects.Count + 1) + " named: " + obj.object_name);
                                }
                            }
                            else if (line.StartsWith("usemtl "))
                            {
                                obj.material_name = line.Substring(7);
                            }
                            else if (line.StartsWith("v "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] verts = line.Replace("  ", " ").Split(' ');
                                Vector3D vert = new Vector3D();
                                vert.X = Convert.ToSingle(verts[1], CultureInfo.InvariantCulture);
                                vert.Y = Convert.ToSingle(verts[2], CultureInfo.InvariantCulture);
                                vert.Z = Convert.ToSingle(verts[3], CultureInfo.InvariantCulture);

                                obj.verts.Add( vert );
                            }
                            else if (line.StartsWith("vt "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                String[] uvs = line.Split(' ');
                                Vector2D uv = new Vector2D();
                                uv.X = Convert.ToSingle(uvs[1], CultureInfo.InvariantCulture);
                                uv.Y = Convert.ToSingle(uvs[2], CultureInfo.InvariantCulture);

                                obj.uv.Add( uv );
                            }
                            else if (line.StartsWith("vn "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                String[] norms = line.Split(' ');
                                Vector3D norm = new Vector3D();
                                norm.X = Convert.ToSingle(norms[1], CultureInfo.InvariantCulture);
                                norm.Y = Convert.ToSingle(norms[2], CultureInfo.InvariantCulture);
                                norm.Z = Convert.ToSingle(norms[3], CultureInfo.InvariantCulture);

                                obj.normals.Add( norm );
                            }
                            else if (line.StartsWith("s "))
                            {
                                current_shade_group = line.Substring(2);
                            }
                            else if (line.StartsWith("f "))
                            {
                                reading_faces = true;

                                if (current_shade_group != null)
                                {
                                    if (obj.shading_groups.ContainsKey(current_shade_group))
                                        obj.shading_groups[current_shade_group].Add(obj.faces.Count);
                                    else
                                    {
                                        List<int> newfaces = new List<int>();
                                        newfaces.Add(obj.faces.Count);
                                        obj.shading_groups.Add(current_shade_group, newfaces);
                                    }
                                }

                                String[] faces = line.Substring(2).Split(' ');
                                for (int x = 0; x < 3; x++)
                                {
                                    Face face = new Face();
                                    if (obj.verts.Count > 0)
                                        face.x = (ushort)(Convert.ToUInt16(faces[x].Split('/')[0]) - prevMaxVerts - 1);
                                    if (obj.uv.Count > 0)
                                        face.y = (ushort)(Convert.ToUInt16(faces[x].Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        face.z = (ushort)(Convert.ToUInt16(faces[x].Split('/')[2]) - prevMaxNorms - 1);
                                    if (face.x < 0 || face.y < 0 || face.z < 0)
                                        throw new Exception();
                                    obj.faces.Add(face);
                                }
                            }
                        }

                        if (!objects.Contains(obj))
                            objects.Add(obj);
                    }
                }
                

                //Read each object
                foreach(obj_data obj in objects)
                {
                    //One would fix Tatsuto's broken shading here.
                    
                    //Locate the proper model
                    uint modelSectionid = 0;
                    foreach (KeyValuePair<uint, object> pair in parsed_sections)
                    {
                        if (modelSectionid != 0)
                            break;

                        if(pair.Value is Model)
                        {
                            UInt64 tryp;
                            if (UInt64.TryParse(obj.object_name, out tryp))
                            {
                                if (tryp == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                            else
                            {
                                if (Hash64.HashString(obj.object_name) == ((Model)pair.Value).object3D.hashname)
                                    modelSectionid = pair.Key;
                            }
                        }
                    }

                    //Apply new changes
                    if (modelSectionid == 0)
                    {
                        toAddObjects.Add(obj);
                        continue;
                    }

                    Model model_data_section = (Model)parsed_sections[modelSectionid];
                    PassthroughGP passthrough_section = (PassthroughGP)parsed_sections[model_data_section.passthroughGP_ID];
                    Geometry geometry_section = (Geometry)parsed_sections[passthrough_section.geometry_section];
                    Topology topology_section = (Topology)parsed_sections[passthrough_section.topology_section];

                    uint geometry_size = geometry_section.size - (uint)((geometry_section.verts.Count * 12) + (geometry_section.uvs.Count * 8) + (geometry_section.normals.Count * 12));
                    uint facelist_size = topology_section.size - (uint)(topology_section.facelist.Count * 6);

                    List<Face> called_faces = new List<Face>();
                    List<int> duplicate_verts = new List<int>();
                    Dictionary<int, Face> dup_faces = new Dictionary<int, Face>();

                    bool broken = false;
                    for( int x_f = 0; x_f < obj.faces.Count; x_f++)
                    {
                        Face f = obj.faces[x_f];
                        broken = false;

                        foreach( Face called_f in called_faces)
                        {
                            if( called_f.x == f.x && called_f.y != f.y)
                            {
                                duplicate_verts.Add(x_f);
                                broken = true;
                                break;
                            }
                        }

                        if(!broken)
                            called_faces.Add(f);
                    }

                    Dictionary<int, Face> done_faces = new Dictionary<int, Face>();

                    foreach (int dupe in duplicate_verts)
                    {
                        int replacedF = -1;
                        foreach (KeyValuePair<int, Face> pair in done_faces)
                        {
                            Face f = pair.Value;
                            if (f.x == obj.faces[dupe].x && f.y == obj.faces[dupe].y)
                            {
                                replacedF = pair.Key;
                            }
                        }

                        Face new_face = new Face();
                        if (replacedF > -1)
                        {
                            new_face.x = obj.faces[replacedF].x;
                            new_face.y = obj.faces[replacedF].y;
                            new_face.z = obj.faces[dupe].z;

                        }
                        else
                        {
                            new_face.x = (ushort)obj.verts.Count;
                            new_face.y = obj.faces[dupe].y;
                            new_face.z = obj.faces[dupe].z;
                            obj.verts.Add(obj.verts[obj.faces[dupe].x]);

                            done_faces.Add(dupe, obj.faces[dupe]);
                        }

                        obj.faces[dupe] = new_face;
                    }

                    Vector3D new_Model_data_bounds_min = new Vector3D();// Z (max), X (low), Y (low)
                    Vector3D new_Model_data_bounds_max = new Vector3D();// Z (low), X (max), Y (max)

                    foreach(Vector3D vert in obj.verts)
                    {
                        //Z
                        if (new_Model_data_bounds_min.Z == null)
                        {
                            new_Model_data_bounds_min.Z = vert.Z;
                        }
                        else
                        {
                            if (vert.Z > new_Model_data_bounds_min.Z)
                                new_Model_data_bounds_min.Z = vert.Z;
                        }

                        if (new_Model_data_bounds_max.Z == null)
                        {
                            new_Model_data_bounds_max.Z = vert.Z;
                        }
                        else
                        {
                            if (vert.Z < new_Model_data_bounds_max.Z)
                                new_Model_data_bounds_max.Z = vert.Z;
                        }

                        //X
                        if (new_Model_data_bounds_min.X == null)
                        {
                            new_Model_data_bounds_min.X = vert.X;
                        }
                        else
                        {
                            if (vert.X < new_Model_data_bounds_min.X)
                                new_Model_data_bounds_min.X = vert.X;
                        }

                        if (new_Model_data_bounds_max.X == null)
                        {
                            new_Model_data_bounds_max.X = vert.X;
                        }
                        else
                        {
                            if (vert.X > new_Model_data_bounds_max.X)
                                new_Model_data_bounds_max.X = vert.X;
                        }

                        //Y
                        if (new_Model_data_bounds_min.Y == null)
                        {
                            new_Model_data_bounds_min.Y = vert.Y;
                        }
                        else
                        {
                            if (vert.Y < new_Model_data_bounds_min.Y)
                                new_Model_data_bounds_min[2] = vert.Y;
                        }

                        if (new_Model_data_bounds_max.Y == null)
                        {
                            new_Model_data_bounds_max.Y = vert.Y;
                        }
                        else
                        {
                            if (vert.Y > new_Model_data_bounds_max.Y)
                                new_Model_data_bounds_max.Y = vert.Y;
                        }
                    }

                    //Arrange UV and Normals
                    List<Vector2D> new_arranged_Geometry_UVs = new List<Vector2D>();
                    List<Vector3D> new_arranged_Geometry_normals = new List<Vector3D>();
                    List<Vector3D> new_arranged_Geometry_unknown20 = new List<Vector3D>();
                    List<Vector3D> new_arranged_Geometry_unknown21 = new List<Vector3D>();
                    List<int> added_uvs = new List<int>();
                    List<int> added_normals = new List<int>();

                    Vector2D[] new_arranged_UV = new Vector2D[obj.verts.Count];
                    for (int x = 0; x < new_arranged_UV.Length; x++)
                        new_arranged_UV[x] = new Vector2D(100f, 100f);
                    Vector2D sentinel = new Vector2D(100f, 100f);
                    Vector3D[] new_arranged_Normals = new Vector3D[obj.verts.Count];
                    Vector3D[] new_arranged_unknown20 = new Vector3D[obj.verts.Count];
                    Vector3D[] new_arranged_unknown21 = new Vector3D[obj.verts.Count];

                    List<Face> new_faces = new List<Face>();

                    for (int fcount = 0; fcount < obj.faces.Count; fcount += 3)
                    {
                        Face f1 = obj.faces[fcount+0];
                        Face f2 = obj.faces[fcount+1];
                        Face f3 = obj.faces[fcount+2];

                        //UV
                        if (obj.uv.Count > 0)
                        {
                            if (new_arranged_UV[f1.x].Equals(sentinel))
                                new_arranged_UV[f1.x] = obj.uv[f1.y];
                            if (new_arranged_UV[f2.x].Equals(sentinel))
                                new_arranged_UV[f2.x] = obj.uv[f2.y];
                            if (new_arranged_UV[f3.x].Equals(sentinel))
                                new_arranged_UV[f3.x] = obj.uv[f3.y];
                        }
                        //normal
                        if (obj.normals.Count > 0)
                        {
                            new_arranged_Normals[f1.x] = obj.normals[f1.z];
                            new_arranged_Normals[f2.x] = obj.normals[f2.z];
                            new_arranged_Normals[f3.x] = obj.normals[f3.z];
                        }
                        Face new_f = new Face();
                        new_f.x = f1.x;
                        new_f.y = f2.x;
                        new_f.z = f3.x;

                        new_faces.Add(new_f);
                    }

                    List<Vector3D> obj_verts = obj.verts;
                    ComputeTangentBasis(ref new_faces, ref obj_verts, ref new_arranged_UV, ref new_arranged_Normals, ref new_arranged_unknown20, ref new_arranged_unknown21);

                    UInt32[] size_index = { 0, 4, 8, 12, 16, 4, 4, 8, 12 };
                    UInt32 calc_size = 0;
                    foreach (GeometryHeader head in geometry_section.headers)
                    {
                        calc_size += size_index[(int)head.item_size];
                    }

                    List<ModelItem> new_Model_items2 = new List<ModelItem>();

                    foreach (ModelItem modelitem in model_data_section.items)
                    {
                        ModelItem new_model_item = new ModelItem();
                        new_model_item.unknown1 = modelitem.unknown1;
                        new_model_item.vertCount = (uint)new_faces.Count;
                        new_model_item.unknown2 = modelitem.unknown2;
                        new_model_item.faceCount = (uint)obj.verts.Count;
                        new_model_item.material_id = modelitem.material_id;

                        new_Model_items2.Add(new_model_item);
                    }

                    model_data_section.items = new_Model_items2;
                    geometry_section.vert_count = (uint)obj.verts.Count;
                    geometry_section.verts = obj.verts;
                    topology_section.facelist = new_faces;
                    geometry_section.uvs = new_arranged_UV.ToList();
                    geometry_section.normals = new_arranged_Normals.ToList();

                    if (model_data_section.version != 6)
                    {
                        model_data_section.bounds_min = new_Model_data_bounds_min;
                        model_data_section.bounds_max = new_Model_data_bounds_max;
                    }

                    UInt32 geometry_calulated_size = calc_size * (UInt32)obj.verts.Count + (8 + ((UInt32)geometry_section.headers.Count * 8)) + (UInt32)geometry_section.unknown_item_data.Count;
                    if (geometry_section.remaining_data != null)
                        geometry_calulated_size += (UInt32)geometry_section.remaining_data.Length;

                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).size = geometry_calulated_size;
                    ((Topology)parsed_sections[passthrough_section.topology_section]).size = facelist_size + (uint)(new_faces.Count * 6);
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).size = calc_size * (UInt32)obj.verts.Count;

                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).vert_count = (uint)obj.verts.Count;
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).verts = obj.verts;
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).normals = new_arranged_Normals.ToList();
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).uvs = new_arranged_UV.ToList();
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).unknown20 = new_arranged_unknown20.ToList();
                    ((Geometry)parsed_sections[passthrough_section.geometry_section]).unknown21 = new_arranged_unknown21.ToList();

                    ((Topology)parsed_sections[passthrough_section.topology_section]).count1 = (uint)(new_faces.Count * 3);
                    ((Topology)parsed_sections[passthrough_section.topology_section]).facelist = new_faces;

                }


                //Add new objects
                if (addNew)
                {
                    foreach (obj_data obj in toAddObjects)
                    {
                        //create new Model
                        Material newMat = new Material((uint)(obj.object_name + ".material").GetHashCode(), obj.material_name);
                        Material_Group newMatG = new Material_Group((uint)(obj.object_name + ".materialGroup").GetHashCode(), newMat.id);
                        Geometry newGeom = new Geometry((uint)(obj.object_name + ".geom").GetHashCode(), obj);
                        Topology newTopo = new Topology((uint)(obj.object_name + ".topo").GetHashCode(), obj);

                        PassthroughGP newPassGP = new PassthroughGP((uint)(obj.object_name + ".passGP").GetHashCode(), newGeom.id, newTopo.id);
                        TopologyIP newTopoIP = new TopologyIP((uint)(obj.object_name + ".topoIP").GetHashCode(), newTopo.id);

                        Model newModel = new Model(obj, newPassGP.id, newTopoIP.id, newMatG.id);

                        List<Face> called_faces = new List<Face>();
                        List<int> duplicate_verts = new List<int>();
                        Dictionary<int, Face> dup_faces = new Dictionary<int, Face>();

                        bool broken = false;
                        for (int x_f = 0; x_f < obj.faces.Count; x_f++)
                        {
                            Face f = obj.faces[x_f];
                            broken = false;

                            foreach (Face called_f in called_faces)
                            {
                                if (called_f.x == f.x && called_f.y != f.y)
                                {
                                    duplicate_verts.Add(x_f);
                                    broken = true;
                                    break;
                                }
                            }

                            if (!broken)
                                called_faces.Add(f);
                        }

                        Dictionary<int, Face> done_faces = new Dictionary<int, Face>();

                        foreach (int dupe in duplicate_verts)
                        {
                            int replacedF = -1;
                            foreach (KeyValuePair<int, Face> pair in done_faces)
                            {
                                Face f = pair.Value;
                                if (f.x == obj.faces[dupe].x && f.y == obj.faces[dupe].y)
                                {
                                    replacedF = pair.Key;
                                }
                            }

                            Face new_face = new Face();
                            if (replacedF > -1)
                            {
                                new_face.x = obj.faces[replacedF].x;
                                new_face.y = obj.faces[replacedF].y;
                                new_face.z = obj.faces[dupe].z;

                            }
                            else
                            {
                                new_face.x = (ushort)obj.verts.Count;
                                new_face.y = obj.faces[dupe].y;
                                new_face.z = obj.faces[dupe].z;
                                obj.verts.Add(obj.verts[obj.faces[dupe].x]);

                                done_faces.Add(dupe, obj.faces[dupe]);
                            }

                            obj.faces[dupe] = new_face;
                        }

                        Vector3D new_Model_data_bounds_min = new Vector3D();// Z (max), X (low), Y (low)
                        Vector3D new_Model_data_bounds_max = new Vector3D();// Z (low), X (max), Y (max)

                        foreach (Vector3D vert in obj.verts)
                        {
                            //Z
                            if (new_Model_data_bounds_min.Z == null)
                            {
                                new_Model_data_bounds_min.Z = vert.Z;
                            }
                            else
                            {
                                if (vert.Z > new_Model_data_bounds_min.Z)
                                    new_Model_data_bounds_min.Z = vert.Z;
                            }

                            if (new_Model_data_bounds_max.Z == null)
                            {
                                new_Model_data_bounds_max.Z = vert.Z;
                            }
                            else
                            {
                                if (vert.Z < new_Model_data_bounds_max.Z)
                                    new_Model_data_bounds_max.Z = vert.Z;
                            }

                            //X
                            if (new_Model_data_bounds_min.X == null)
                            {
                                new_Model_data_bounds_min.X = vert.X;
                            }
                            else
                            {
                                if (vert.X < new_Model_data_bounds_min.X)
                                    new_Model_data_bounds_min.X = vert.X;
                            }

                            if (new_Model_data_bounds_max.X == null)
                            {
                                new_Model_data_bounds_max.X = vert.X;
                            }
                            else
                            {
                                if (vert.X > new_Model_data_bounds_max.X)
                                    new_Model_data_bounds_max.X = vert.X;
                            }

                            //Y
                            if (new_Model_data_bounds_min.Y == null)
                            {
                                new_Model_data_bounds_min.Y = vert.Y;
                            }
                            else
                            {
                                if (vert.Y < new_Model_data_bounds_min.Y)
                                    new_Model_data_bounds_min[2] = vert.Y;
                            }

                            if (new_Model_data_bounds_max.Y == null)
                            {
                                new_Model_data_bounds_max.Y = vert.Y;
                            }
                            else
                            {
                                if (vert.Y > new_Model_data_bounds_max.Y)
                                    new_Model_data_bounds_max.Y = vert.Y;
                            }
                        }

                        //Arrange UV and Normals
                        List<Vector2D> new_arranged_Geometry_UVs = new List<Vector2D>();
                        List<Vector3D> new_arranged_Geometry_normals = new List<Vector3D>();
                        List<Vector3D> new_arranged_Geometry_unknown20 = new List<Vector3D>();
                        List<Vector3D> new_arranged_Geometry_unknown21 = new List<Vector3D>();
                        List<int> added_uvs = new List<int>();
                        List<int> added_normals = new List<int>();

                        Vector2D[] new_arranged_UV = new Vector2D[obj.verts.Count];
                        for (int x = 0; x < new_arranged_UV.Length; x++)
                            new_arranged_UV[x] = new Vector2D(100f, 100f);
                        Vector2D sentinel = new Vector2D(100f, 100f);
                        Vector3D[] new_arranged_Normals = new Vector3D[obj.verts.Count];
                        for (int x = 0; x < new_arranged_Normals.Length; x++)
                            new_arranged_Normals[x] = new Vector3D(0f, 0f, 0f);
                        Vector3D[] new_arranged_unknown20 = new Vector3D[obj.verts.Count];
                        Vector3D[] new_arranged_unknown21 = new Vector3D[obj.verts.Count];

                        List<Face> new_faces = new List<Face>();

                        for (int fcount = 0; fcount < obj.faces.Count; fcount += 3)
                        {
                            Face f1 = obj.faces[fcount + 0];
                            Face f2 = obj.faces[fcount + 1];
                            Face f3 = obj.faces[fcount + 2];

                            //UV
                            if (obj.uv.Count > 0)
                            {
                                if (new_arranged_UV[f1.x].Equals(sentinel))
                                    new_arranged_UV[f1.x] = obj.uv[f1.y];
                                if (new_arranged_UV[f2.x].Equals(sentinel))
                                    new_arranged_UV[f2.x] = obj.uv[f2.y];
                                if (new_arranged_UV[f3.x].Equals(sentinel))
                                    new_arranged_UV[f3.x] = obj.uv[f3.y];
                            }
                            //normal
                            if (obj.normals.Count > 0)
                            {
                                new_arranged_Normals[f1.x] += obj.normals[f1.z];
                                new_arranged_Normals[f2.x] += obj.normals[f2.z];
                                new_arranged_Normals[f3.x] += obj.normals[f3.z];
                            }
                            Face new_f = new Face();
                            new_f.x = f1.x;
                            new_f.y = f2.x;
                            new_f.z = f3.x;

                            new_faces.Add(new_f);
                        }

                        for (int x = 0; x < new_arranged_Normals.Length; x++)
                            new_arranged_Normals[x].Normalize();

                        List<Vector3D> obj_verts = obj.verts;
                        ComputeTangentBasis(ref new_faces, ref obj_verts, ref new_arranged_UV, ref new_arranged_Normals, ref new_arranged_unknown20, ref new_arranged_unknown21);

                        UInt32[] size_index = { 0, 4, 8, 12, 16, 4, 4, 8, 12 };
                        UInt32 calc_size = 0;
                        foreach (GeometryHeader head in newGeom.headers)
                        {
                            calc_size += size_index[(int)head.item_size];
                        }

                        List<ModelItem> new_Model_items2 = new List<ModelItem>();

                        foreach (ModelItem modelitem in newModel.items)
                        {
                            ModelItem new_model_item = new ModelItem();
                            new_model_item.unknown1 = modelitem.unknown1;
                            new_model_item.vertCount = (uint)new_faces.Count;
                            new_model_item.unknown2 = modelitem.unknown2;
                            new_model_item.faceCount = (uint)obj.verts.Count;
                            new_model_item.material_id = modelitem.material_id;

                            new_Model_items2.Add(new_model_item);
                        }

                        newModel.items = new_Model_items2;
                        newGeom.vert_count = (uint)obj.verts.Count;
                        newGeom.verts = obj.verts;
                        newTopo.facelist = new_faces;
                        newGeom.uvs = new_arranged_UV.ToList();
                        newGeom.normals = new_arranged_Normals.ToList();

                        if (newModel.version != 6)
                        {
                            newModel.bounds_min = new_Model_data_bounds_min;
                            newModel.bounds_max = new_Model_data_bounds_max;
                        }

                        newGeom.vert_count = (uint)obj.verts.Count;
                        newGeom.verts = obj.verts;
                        newGeom.normals = new_arranged_Normals.ToList();
                        newGeom.uvs = new_arranged_UV.ToList();
                        newGeom.unknown20 = new_arranged_unknown20.ToList();
                        newGeom.unknown21 = new_arranged_unknown21.ToList();

                        newTopo.count1 = (uint)(new_faces.Count * 3);
                        newTopo.facelist = new_faces;


                        //Add new sections
                        parsed_sections.Add(newMat.id, newMat);
                        sections.Add(new SectionHeader(newMat.id));
                        parsed_sections.Add(newMatG.id, newMatG);
                        sections.Add(new SectionHeader(newMatG.id));
                        parsed_sections.Add(newGeom.id, newGeom);
                        sections.Add(new SectionHeader(newGeom.id));
                        parsed_sections.Add(newTopo.id, newTopo);
                        sections.Add(new SectionHeader(newTopo.id));
                        parsed_sections.Add(newPassGP.id, newPassGP);
                        sections.Add(new SectionHeader(newPassGP.id));
                        parsed_sections.Add(newTopoIP.id, newTopoIP);
                        sections.Add(new SectionHeader(newTopoIP.id));
                        parsed_sections.Add(newModel.id, newModel);
                        sections.Add(new SectionHeader(newModel.id));
                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }


        public bool GenerateModelFromObj(String filepath)
        {
            Console.WriteLine("Importing new obj with file: " + filepath);

            //Preload the .obj
            List<obj_data> objects = new List<obj_data>();


            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        obj_data obj = new obj_data();
                        bool reading_faces = false;
                        int prevMaxVerts = 0;
                        int prevMaxUvs = 0;
                        int prevMaxNorms = 0;

                        while ((line = sr.ReadLine()) != null)
                        {
                            
                            //preloading objects
                            if (line.StartsWith("#"))
                                continue;
                            else if (line.StartsWith("o ") || line.StartsWith("g "))
                            {

                                if (reading_faces && obj.faces.Count > 0)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                obj.object_name = line.Substring(2);
                            }
                            else if (line.StartsWith("v "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }

                                String[] verts = line.Replace("  ", " ").Split(' ');
                                Vector3D vert = new Vector3D();
                                vert.X = Convert.ToSingle(verts[1], CultureInfo.InvariantCulture);
                                vert.Y = Convert.ToSingle(verts[2], CultureInfo.InvariantCulture);
                                vert.Z = Convert.ToSingle(verts[3], CultureInfo.InvariantCulture);

                                obj.verts.Add( vert );
                            }
                            else if (line.StartsWith("vt "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                String[] uvs = line.Split(' ');
                                Vector2D uv = new Vector2D();
                                uv.X = Convert.ToSingle(uvs[1], CultureInfo.InvariantCulture);
                                uv.Y = Convert.ToSingle(uvs[2], CultureInfo.InvariantCulture);

                                obj.uv.Add( uv );
                            }
                            else if (line.StartsWith("vn "))
                            {

                                if (reading_faces)
                                {
                                    reading_faces = false;
                                    prevMaxVerts += obj.verts.Count;
                                    prevMaxUvs += obj.uv.Count;
                                    prevMaxNorms += obj.normals.Count;

                                    objects.Add(obj);
                                    obj = new obj_data();
                                }
                                
                                String[] norms = line.Split(' ');
                                Vector3D norm = new Vector3D();
                                norm.X = Convert.ToSingle(norms[1], CultureInfo.InvariantCulture);
                                norm.Y = Convert.ToSingle(norms[2], CultureInfo.InvariantCulture);
                                norm.Z = Convert.ToSingle(norms[3], CultureInfo.InvariantCulture);

                                obj.normals.Add( norm );
                            }
                            else if (line.StartsWith("f "))
                            {
                                reading_faces = true;
                                String[] faces = line.Substring(2).Split(' ');
                                foreach (string f in faces)
                                {
                                    Face face = new Face();
                                    if(obj.verts.Count > 0)
                                        face.x = (ushort)(Convert.ToUInt16(f.Split('/')[0]) - prevMaxVerts - 1);
                                    if(obj.uv.Count > 0)
                                        face.y = (ushort)(Convert.ToUInt16(f.Split('/')[1]) - prevMaxUvs - 1);
                                    if (obj.normals.Count > 0)
                                        face.z = (ushort)(Convert.ToUInt16(f.Split('/')[2]) - prevMaxNorms - 1);
                                    if (face.x < 0 || face.y < 0 || face.z < 0)
                                        throw new Exception();
                                    obj.faces.Add(face);
                                }

                            }
                        }

                        if (!objects.Contains(obj))
                            objects.Add(obj);

                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }


        public bool GenerateNewModel(String filename)
        {

            //you remove items from the parsed_sections
            //you edit items in the parsed_sections, they will get read and exported

            //Sort the sections
            List<Animation> animation_sections = new List<Animation>();
            List<Author> author_sections = new List<Author>();
            List<Material_Group> material_group_sections = new List<Material_Group>();
            List<Object3D> object3D_sections = new List<Object3D>();
            List<Model> model_sections = new List<Model>();


            foreach (SectionHeader sectionheader in sections)
            {
                if (!parsed_sections.Keys.Contains(sectionheader.id))
                    continue;
                object section = parsed_sections[sectionheader.id];

                if (section is Animation)
                {
                    animation_sections.Add(section as Animation);
                }
                else if (section is Author)
                {
                    author_sections.Add(section as Author);
                }
                else if (section is Material_Group)
                {
                    material_group_sections.Add(section as Material_Group);
                }
                else if (section is Object3D)
                {
                    object3D_sections.Add(section as Object3D);
                }
                else if (section is Model)
                {
                    model_sections.Add(section as Model);
                }

            }

            //after each section, you go back and enter it's new size
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {

                        bw.Write(-1); //the - (yyyy)
                        bw.Write((UInt32)100); //Filesize (GO BACK AT END AND CHANGE!!!)
                        int sectionCount = sections.Count;
                        bw.Write(sectionCount); //Sections count

                        foreach (Animation anim_sec in animation_sections)
                        {
                            anim_sec.StreamWrite(bw);
                        }

                        foreach (Author author_sec in author_sections)
                        {
                            author_sec.StreamWrite(bw);
                        }

                        foreach (Material_Group mat_group_sec in material_group_sections)
                        {
                            mat_group_sec.StreamWrite(bw);
                            foreach (uint id in mat_group_sec.items)
                            {
                                if (parsed_sections.Keys.Contains(id))
                                    (parsed_sections[id] as Material).StreamWrite(bw);
                            }
                        }

                        foreach (Object3D obj3d_sec in object3D_sections)
                        {
                            obj3d_sec.StreamWrite(bw);
                        }

                        foreach (Model model_sec in model_sections)
                        {
                            model_sec.StreamWrite(bw);
                        }


                        foreach (SectionHeader sectionheader in sections)
                        {
                            if (!parsed_sections.Keys.Contains(sectionheader.id))
                                continue;
                            object section = parsed_sections[sectionheader.id];

                            if (section is Unknown)
                            {
                                (section as Unknown).StreamWrite(bw);
                            }
                            else if (section is Animation ||
                                    section is Author ||
                                    section is Material_Group ||
                                    section is Material ||
                                    section is Object3D ||
                                    section is Model
                                )
                            {
                                continue;
                            }
                            else if (section is Geometry)
                            {
                                (section as Geometry).StreamWrite(bw);
                            }
                            else if (section is Topology)
                            {
                                (section as Topology).StreamWrite(bw);
                            }
                            else if (section is PassthroughGP)
                            {
                                (section as PassthroughGP).StreamWrite(bw);
                            }
                            else if (section is TopologyIP)
                            {
                                (section as TopologyIP).StreamWrite(bw);
                            }
                            else if (section is Bones)
                            {
                                (section as Bones).StreamWrite(bw);
                            }
                            else if (section is SkinBones)
                            {
                                (section as SkinBones).StreamWrite(bw);
                            }
                            else if (section is QuatLinearRotationController)
                            {
                                (section as QuatLinearRotationController).StreamWrite(bw);
                            }
                            else if (section is LinearVector3Controller)
                            {
                                (section as LinearVector3Controller).StreamWrite(bw);
                            }
                            else
                            {
                                Console.WriteLine("Tried to export an unknown section.");
                            }
                        }

                        if (leftover_data != null)
                            bw.Write(leftover_data);


                        fs.Position = 4;
                        bw.Write((UInt32)fs.Length);

                    }
                }
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show(exc.ToString());
                return false;
            }
            return true;
        }

        private static void ComputeTangentBasis(ref List<Face> faces, ref List<Vector3D> verts, ref Vector2D[] uvs, ref Vector3D[] normals, ref Vector3D[] tangents, ref Vector3D[] binormals)
	    {
            //Taken from various sources online. Search up Normal Vector Tangent calculation.

            List<ushort> parsed = new List<ushort>();

            foreach (Face f in faces)
            {
                Vector3D P0 = verts[f.x];
                Vector3D P1 = verts[f.y];
                Vector3D P2 = verts[f.z];
                
                Vector2D UV0 = uvs[f.x];
                Vector2D UV1 = uvs[f.y];
                Vector2D UV2 = uvs[f.z];


                float u02 = (uvs[f.z].X - uvs[f.x].X);
                float v02 = (uvs[f.z].Y - uvs[f.x].Y);
                float u01 = (uvs[f.y].X - uvs[f.x].X);
                float v01 = (uvs[f.y].Y - uvs[f.x].Y);
                float dot00 = u02 * u02 + v02 * v02;
                float dot01 = u02 * u01 + v02 * v01;
                float dot11 = u01 * u01 + v01 * v01;
                float d = dot00 * dot11 - dot01 * dot01;
                float u = 1.0f;
                float v = 1.0f;
                if (d != 0.0f)
                {
                    u = (dot11 * u02 - dot01 * u01) / d;
                    v = (dot00 * u01 - dot01 * u02) / d;
                }

                Vector3D tangent = verts[f.z] * u + verts[f.y] * v - verts[f.x] * (u + v);

                //vert1
                if (!parsed.Contains(f.x))
                {
                    binormals[f.x] = Vector3D.Cross(tangent, normals[f.x]);
                    binormals[f.x].Normalize();
                    tangents[f.x] = Vector3D.Cross(binormals[f.x], normals[f.x]);
                    tangents[f.x].Normalize();
                    parsed.Add(f.x);
                }

                //vert2
                if (!parsed.Contains(f.y))
                {
                    binormals[f.y] = Vector3D.Cross(tangent, normals[f.y]);
                    binormals[f.y].Normalize();
                    tangents[f.y] = Vector3D.Cross(binormals[f.y], normals[f.y]);
                    tangents[f.y].Normalize();
                    parsed.Add(f.y);
                }
                //vert3
                if (!parsed.Contains(f.z))
                {
                    binormals[f.z] = Vector3D.Cross(tangent, normals[f.z]);
                    binormals[f.z].Normalize();
                    tangents[f.z] = Vector3D.Cross(binormals[f.z], normals[f.z]);
                    tangents[f.z].Normalize();
                    parsed.Add(f.z);
                }

            }
	    }

        private static float fabsf(float p)
        {
            return Math.Abs(p);
        }
    }
}
