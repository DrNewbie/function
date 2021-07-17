// -----------------------------------------------------------------------
// <copyright file="DieselUnit.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DieselUnit
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DieselUnit
    {
        public static string BasePath;

        public string UnitPath { get; private set; }

        public Dictionary<string, DieselGeometry> SubModels { get; private set; }

        public Dictionary<string, DieselMaterial> Materials { get; private set; }

        public Dictionary<int, DieselObject3D> Objects { get; private set; } 

        public float[,] RootOrientation { get; private set; }

        public float[] RootOffset { get; private set; }

        public bool UnitDataLoaded { get; private set; }

        public bool ModelDataLoaded { get; private set; }

        public string Error { get; private set; }

        private readonly HashSet<string> toLoadSubModels = new HashSet<string>();

        private readonly HashSet<string> enabledSubModels = new HashSet<string>(); 

        private string orientationObjectName;

        public DieselUnit(string unitPath)
        {
            this.UnitPath = unitPath;
            this.Materials = new Dictionary<string, DieselMaterial>();
            this.SubModels = new Dictionary<string, DieselGeometry>();
            this.Objects = new Dictionary<int, DieselObject3D>();
            this.LoadUnitData();
            this.LoadModelData();
            //this.CleanupUnused();
        }

        #region Model Parsing

        void LoadModelData()
        {
            try
            {
                var model = new DieselModel(this.UnitPath, BasePath);
                this.Objects[0] = new DieselObject3D();
                foreach (Object3DModelSection object3d in model.GetObject3DModelSections())
                {
                    var newObject = new DieselObject3D
                                        {
                                            Orientation = object3d.RotationMatrix,
                                            Position = object3d.Position,
                                            ParentId = object3d.ParentSectionId,
                                            SectionId = object3d.SectionId
                                        };
                    this.Objects[object3d.SectionId] = newObject;
                }
                foreach (var subModelName in this.toLoadSubModels)
                {
                    var objectSection = model.GetModelDataByName(subModelName);
                    if (objectSection == null) continue;
                    var passthroughGP =
                        (PassthroughGPModelSection)model.GetModelSectionById(objectSection.PassthroughGPSectionId);
                    var indicesSection =
                        (TopologyModelSection)model.GetModelSectionById(passthroughGP.TopologySectionId);
                    var geometrySection =
                        (GeometryModelSection)model.GetModelSectionById(passthroughGP.GeometrySectionId);
                    if (indicesSection == null || geometrySection == null) continue;
                    var subModel = new DieselGeometry
                                       {
                                           SectionId = objectSection.SectionId,
                                           ParentId = objectSection.Object3D.ParentSectionId,
                                           BoundsLower = objectSection.BoundsLower,
                                           BoundsUpper = objectSection.BoundsUpper,
                                           Offset = objectSection.Object3D.Position,
                                           Orientation = objectSection.Object3D.RotationMatrix,
                                           Enabled = this.enabledSubModels.Contains(subModelName),
                                           Indices = indicesSection.Indicies,
                                           IndiceCount = indicesSection.IndexCount,
                                           VertexCount = geometrySection.ItemCount,
                                           Vertices = geometrySection.Vertices,
                                           UVs = geometrySection.UVs,
                                           Normals = geometrySection.Normals,
                                           UnknownB = geometrySection.VertexColors,
                                           UnknownC = geometrySection.VertexSpecular
                                       };
                    SubModels[subModelName] = subModel;
                    var object3d = objectSection.Object3D;
                    var newObject = new DieselObject3D
                    {
                        Orientation = object3d.RotationMatrix,
                        Position = object3d.Position,
                        ParentId = object3d.ParentSectionId,
                        SectionId = objectSection.SectionId
                    };
                    Objects[subModel.SectionId] = newObject;
                }
                var orientationObject = model.GetObject3DByName(this.orientationObjectName);
                if (orientationObject == null)
                {
                    Console.WriteLine("Failed to find orientation node {0} in unit {1}", this.orientationObjectName, this.UnitPath);
                    orientationObject = new Object3DModelSection();
                }
                this.RootOffset = orientationObject.Position;
                this.RootOrientation = orientationObject.RotationMatrix;
                this.ModelDataLoaded = true;
            }
            catch (Exception e)
            {
                this.ModelDataLoaded = false;
                this.Error = e.Message;
            }
        }

        #endregion

        #region Object XML Parsing
        private void ParseObjectNode(XmlNode node)
        {
            var tempNode = node.Attributes.GetNamedItem("shadow_caster");
            if (tempNode != null)
                if(tempNode.Value != "false")
                    return;
            tempNode = node.Attributes.GetNamedItem("lod");
            if(tempNode != null)
                return;
            if (node.Attributes.GetNamedItem("name") != null)
            {
                var name = node.Attributes.GetNamedItem("name").Value;
                toLoadSubModels.Add(name);
                tempNode = node.Attributes.GetNamedItem("enabled");
                if (tempNode == null)
                {
                    enabledSubModels.Add(name);
                }
                else if (tempNode.Value != "false")
                {
                    enabledSubModels.Add(name);
                }
            }
        }

        private void ParseLodObject(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case "object":
                        ParseObjectNode(child);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void ParseGraphicGroup(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case "object":
                        ParseObjectNode(child);
                        break;
                    case "lod_object":
                        ParseLodObject(child);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void LoadUnitData()
        {
            var unitPath = BasePath + this.UnitPath + ".object";
            var xml = new XmlDocument();
            try
            {
                xml.Load(unitPath);
                XmlNodeList dieselItems = xml.GetElementsByTagName("diesel");
                if (dieselItems.Count == 0 || dieselItems.Item(0) == null)
                {
                    this.Error = string.Format("'diesel' node does not exist in unit object file {0}", this.UnitPath);
                    this.UnitDataLoaded = false;
                    return;
                }
                this.orientationObjectName = dieselItems.Item(0).Attributes.GetNamedItem("orientation_object").Value;
                XmlNodeList graphicsItems = xml.GetElementsByTagName("graphics");
                if (graphicsItems.Count == 0 || graphicsItems.Item(0) == null)
                {
                    this.Error = string.Format("'graphics' node does not exist in unit object file {0}", this.UnitPath);
                    this.UnitDataLoaded = false;
                    return;
                }
                XmlNode graphicsNode = graphicsItems.Item(0);
                foreach (XmlNode child in graphicsNode.ChildNodes)
                {
                    switch (child.LocalName)
                    {
                        case "object":
                            this.ParseObjectNode(child);
                            break;
                        case "graphic_group":
                            this.ParseGraphicGroup(child);
                            break;
                        case "lod_object":
                            this.ParseLodObject(child);
                            break;
                        default:
                            continue;
                    }
                }
            }
            catch (XmlException e)
            {
                Console.WriteLine("Exception when loading object file for unit {0} - {1}", this.UnitPath, e.Message);
                this.Error = e.Message;
                this.UnitDataLoaded = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when loading object file for unit {0} - {1}", this.UnitPath, e.Message);
                this.Error = e.Message;
                this.UnitDataLoaded = false;
            }
        }
        #endregion
    }
}
