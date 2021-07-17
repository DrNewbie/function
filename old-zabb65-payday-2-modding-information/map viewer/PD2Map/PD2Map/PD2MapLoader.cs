using System.Collections.Generic;
using Axiom.Core;
using Axiom.Math;

namespace PD2Map
{
    using System;

    using DieselScriptData = DieselScriptData.DieselScriptData;
    using System.IO;

    public class PD2MapLoader
    {
        private readonly Dictionary<int, SceneNode> idNodeTable = new Dictionary<int, SceneNode>();
        private MapViewer viewer;

        List<String> used_units = new List<string>();

        public static string BasePath;

        private void ParseMissions(DieselScriptData baseMission)
        {
            foreach (Dictionary<string, object> scriptNode in baseMission.Root.Values)
            {
                if (scriptNode.ContainsKey("file"))
                {
                    DieselScriptData missionData = new DieselScriptData(BasePath, scriptNode["file"] + ".mission");
                    this.ParseMission(missionData);
                }
            }
        }

        private void ParseMission(DieselScriptData missionData)
        {
            foreach (Dictionary<string, object> scriptSection in missionData.Root.Values)
            {
                if(scriptSection.ContainsKey("elements"))
                    this.ParseScriptSection((Dictionary<string, object>)scriptSection["elements"]);
            }
        }

        private void ParseScriptSection(Dictionary<string, object> scriptSection)
        {
            foreach (dynamic element in scriptSection.Values)
            {
                if (!element.ContainsKey("class")) continue;
                if (element["class"] == "ElementAreaTrigger") this.ParseAreaTrigger(element);
            }
        }

        private void ParseAreaTrigger(Dictionary<string, object> areaTrigger)
        {
            if (!areaTrigger.ContainsKey("id") || !areaTrigger.ContainsKey("editor_name")
                || !areaTrigger.ContainsKey("values"))
            {
                return;
            }
            dynamic values = areaTrigger["values"];
            if (!values.ContainsKey("height") || !values.ContainsKey("position") || !values.ContainsKey("rotation")
                || !values.ContainsKey("enabled"))
            {
                return;
            }
            if (values["enabled"] != true) return;
            if (values["instigator"] != "player") return;
            var newMesh = (Mesh)MeshManager.Instance.GetByName("cube.mesh");
            var scale = new Vector3();
            if (values.ContainsKey("shape_type") && values["shape_type"] == "cylinder")
            {
                newMesh = (Mesh)MeshManager.Instance.GetByName("cylinder.mesh");
                scale[2] = values["height"];
                scale[1] = values["radius"] * 2;
                scale[0] = values["radius"] * 2;
            }
            else
            {
                scale[0] = values["width"];
                scale[1] = values["depth"];
                scale[2] = values["height"];
            }
            var newEntity = this.viewer.Scene.CreateEntity(areaTrigger["editor_name"].ToString(), newMesh);
            newEntity.MaterialName = "areaTriggerMaterial";
            newEntity.VisibilityFlags = (int)ObjectLayers.ScriptObjects;
            newEntity.UserData = areaTrigger["id"].ToString() + areaTrigger["editor_name"];
            var newNode = this.viewer.SceneRoot.CreateChildSceneNode(areaTrigger["editor_name"].ToString());
            newNode.ScaleBy(scale);
            var position = values["position"];
            newNode.Translate(new Vector3(position[0], position[1], position[2]));
            var rotation = values["rotation"];
            newNode.Rotate(new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]));
            newNode.AttachObject(newEntity);
        }

        private void ParseUnitData(Dictionary<string, object> objectData)
        {
            if (objectData.ContainsKey("name") &&
                objectData.ContainsKey("unit_id") &&
                objectData.ContainsKey("name_id") &&
                objectData.ContainsKey("position") &&
                objectData.ContainsKey("rotation"))
            {
                string unitPath = (string)objectData["name"];
                dynamic unitName = objectData["name_id"];
                dynamic unitId = objectData["unit_id"];
                dynamic unitPosition = objectData["position"];
                dynamic unitRotation = objectData["rotation"];
                uint visibilityFlags = 0;
                if (unitPath.StartsWith("units/dev_tools") || unitPath.StartsWith("units/lights"))
                {
                    visibilityFlags |= (int)ObjectLayers.DevObjects;
                }
                else
                {
                    visibilityFlags |= (int)ObjectLayers.StaticObjects;
                    if (!used_units.Contains(unitPath))
                        used_units.Add(unitPath);

                }
                SceneNode newUnit = UnitManager.CreateSceneObjectFromUnit(
                    viewer,
                    unitPath,
                    unitName + unitId.ToString(),
                    visibilityFlags);
                if (newUnit == null) return;
                idNodeTable[(int)unitId] = newUnit;
                newUnit.Translate(new Vector3(unitPosition[0], unitPosition[1], unitPosition[2]));
                newUnit.Rotate(new Quaternion(unitRotation[3], unitRotation[0], unitRotation[1], unitRotation[2]));
                //newUnit.Rotate(new Quaternion(unitRotation[3], unitRotation[2], unitRotation[1], -unitRotation[0]));
                //newUnit.Rotate(new Quaternion(unitRotation[1], -unitRotation[3], unitRotation[2], -unitRotation[0]));

                /*
                if (object_data.ContainsKey("lights"))
                {
                    dynamic lights = object_data["lights"];
                    foreach (KeyValuePair<string, object> kvpair in lights)
                    {
                        dynamic light = kvpair.Value;
                        if (light.ContainsKey("color") && 
                            light.ContainsKey("far_range") && 
                            light.ContainsKey("falloff_exponent") && 
                            light.ContainsKey("enabled") &&
                            light.ContainsKey("name"))
                        {
                            dynamic light_falloff = light["falloff_exponent"];
                            dynamic light_color = light["color"];
                            dynamic light_enabled = light["enabled"];
                            dynamic light_range = light["far_range"];
                            Light light_unit = viewer.mScene.CreateLight((string)unit_name+"_light"+(string)kvpair.Key);
                            new_unit.AttachObject(light_unit);
                            light_unit.Type = LightType.Point;
                            light_unit.Diffuse = new ColorEx(light_color[0], light_color[1], light_color[2]);
                            light_unit.CastShadows = false;
                            light_unit.RenderingDistance = light_range;
                            light_unit.SetAttenuation(light_range, 0, light_falloff, 0);
                        }
                    }
                }
                */
            }
        }

        private void ParseContinent(DieselScriptData baseContinent)
        {
            if (baseContinent.Root.ContainsKey("statics"))
            {
                dynamic statics = baseContinent.Root["statics"];
                foreach (KeyValuePair<string, object> kvpair in statics)
                {
                    dynamic value = kvpair.Value;
                    if (value.ContainsKey("unit_data"))
                    {
                        dynamic unitData = value["unit_data"];
                        ParseUnitData(unitData);
                    }
                }
            }
        }

        private void ParseContinents(DieselScriptData baseContinents)
        {
            foreach( KeyValuePair<string, object> kvpair in baseContinents.Root)
            {
                dynamic value = kvpair.Value;
                if (value.ContainsKey("enabled"))
                {
                    if (value["enabled"] != true)
                        continue;
                }
                /*
                if (value.ContainsKey("visible"))
                {
                    if (value["visible"] != true)
                        continue;
                }
                */
                if (value.ContainsKey("name"))
                {
                    if (value["name"] != null)
                    {
                        string name = value["name"].ToString();
                        DieselScriptData continentData = new DieselScriptData(BasePath, name + "/" + name + ".continent");
                        ParseContinent(continentData);
                    }
                }
            }
        }

        private void ParseMassUnit(DieselMassUnit massUnit)
        {
            int unitId = 0;
            foreach (var unit in massUnit.Instances)
            {
                foreach (var instance in unit.Value)
                {
                    var newUnit = UnitManager.CreateSceneObjectFromUnit(
                        viewer,
                        unit.Key,
                        "massunit_unit_" + unitId++,
                        (uint)ObjectLayers.MassunitObjects);
                    if (newUnit == null) continue;
                    newUnit.Translate(new Vector3(instance.Position[0], instance.Position[1], instance.Position[2]));
                    newUnit.Rotate(new Quaternion(instance.Rotation[0], instance.Rotation[1], instance.Rotation[2], instance.Rotation[3]));
                }
            }
        }

        private void ParseBaseWorld(DieselScriptData baseWorld)
        {
            if (baseWorld.Root.ContainsKey("world_data"))
            {
                if (baseWorld.Root["world_data"].ContainsKey("continents_file"))
                {
                    DieselScriptData continentsData = new DieselScriptData(BasePath, baseWorld.Root["world_data"]["continents_file"] + ".continents");
                    ParseContinents(continentsData);
                }
            }
            if (baseWorld.Root.ContainsKey("brush"))
            {
                if (baseWorld.Root["brush"].ContainsKey("file"))
                {
                    DieselMassUnit massUnitData = new DieselMassUnit(BasePath, baseWorld.Root["brush"]["file"] + ".massunit");
                    ParseMassUnit(massUnitData);
                }
            }
        }

        public void LoadMap(MapViewer upViewer, string worldFile)
        {
            viewer = upViewer;
            BasePath = System.IO.Path.GetDirectoryName(worldFile) + "\\";
            worldFile = System.IO.Path.GetFileName(worldFile);
            DieselScriptData baseWorld = new DieselScriptData(BasePath, worldFile);
            this.ParseBaseWorld(baseWorld);
            DieselScriptData baseMission = new DieselScriptData(BasePath, "mission.mission"); //If this can be referenced by path, I have not found where.
            this.ParseMissions(baseMission);

            //generate used units
            used_units.Sort();
            /*
            using (FileStream fs = new FileStream("Framing_frame_stage1.txt", FileMode.CreateNew, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("The following units have been used in Framing Frame Stage 1");
                sw.WriteLine();


                foreach(var unit in used_units)
                    sw.WriteLine(unit);
            }
            */

        }
    }
}
