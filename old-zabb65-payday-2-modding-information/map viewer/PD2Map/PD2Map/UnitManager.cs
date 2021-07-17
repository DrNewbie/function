// -----------------------------------------------------------------------
// <copyright file="UnitManager.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PD2Map
{
    using System;
    using System.Collections.Generic;
    using Axiom.Core;
    using Axiom.Math;

    using DieselUnit;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UnitManager
    {
        private static readonly Dictionary<string, PDMapUnit> unitObjects = new Dictionary<string, PDMapUnit>();

        public static PDMapUnit GetOrCreateUnit(string unitPath)
        {
            if (unitObjects.ContainsKey(unitPath))
            {
                return unitObjects[unitPath];
            }
            var newUnit = new PDMapUnit(unitPath);
            unitObjects[unitPath] = newUnit;
            return newUnit;
        }

        private static void ApplyTransformations(Node node, float[,] unitMatrix, float[] unitOffset, bool applyTranslation = true)
        {
            var orientationMatrix = new Matrix4(
                unitMatrix[0, 0],
                unitMatrix[0, 1],
                unitMatrix[0, 2],
                unitMatrix[0, 3],
                unitMatrix[1, 0],
                unitMatrix[1, 1],
                unitMatrix[1, 2],
                unitMatrix[1, 3],
                unitMatrix[2, 0],
                unitMatrix[2, 1],
                unitMatrix[2, 2],
                unitMatrix[2, 3],
                unitMatrix[3, 0],
                unitMatrix[3, 1],
                unitMatrix[3, 2],
                unitMatrix[3, 3]);
            Vector3 scale, translation;
            Quaternion rotation;
            orientationMatrix.Decompose(out translation, out scale, out rotation);
            if (applyTranslation)
            {
                node.Translate(translation);
                node.Translate(new Vector3(unitOffset[0], unitOffset[1], unitOffset[2]));
            }
            node.Rotate(rotation);
            node.ScaleBy(scale);
        }

        public static SceneNode CreateSceneObjectFromUnit(MapViewer viewer, string unitPath, string unitName, uint visibilityFlags)
        {
            var unit = GetOrCreateUnit(unitPath);
            if (unit.Error != null) return null;
            var rootNode = viewer.SceneRoot.CreateChildSceneNode(unitName);
            foreach (var subModelPair in unit.SubModels)
            {
                var subModel = subModelPair.Value;
                var subModelName = subModelPair.Key;
                var subModelNode = rootNode.CreateChildSceneNode(unitName + subModelName);
                var subModelMesh = unit.GetMesh(subModelName);
                subModelMesh.Load();

                if (subModelNode.Name.Contains("g_frame1"))
                    Console.WriteLine();

                Queue<DieselObject3D> transforms = new Queue<DieselObject3D>();
                int parentId = subModel.ParentId;
                while (parentId != 0 && unit.Objects.ContainsKey(parentId))
                {
                    var parentObject = unit.Objects[parentId];
                    transforms.Enqueue(parentObject);
                    parentId = parentObject.ParentId;
                }
                ApplyTransformations(subModelNode, subModel.Orientation, subModel.Offset);
                while (transforms.Count != 0)
                {
                    var object3d = transforms.Dequeue();
                    ApplyTransformations(subModelNode, object3d.Orientation, object3d.Position, object3d.ParentId != 0);
                }
                ApplyTransformations(subModelNode, unit.RootOrientation, unit.RootOffset, false);
                var subModelEntity = viewer.Scene.CreateEntity(unitName + subModelName, subModelMesh);
                subModelEntity.UserData = unitPath;
                subModelEntity.VisibilityFlags = visibilityFlags;
                subModelNode.AttachObject(subModelEntity);
            }
            return rootNode;
        }
    }
}
