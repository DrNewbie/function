// -----------------------------------------------------------------------
// <copyright file="PDMapUnit.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PD2Map
{
    using System.Collections.Generic;

    using Axiom.Core;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PDMapUnit : DieselUnit.DieselUnit
    {
        private readonly Dictionary<string, PD2MeshLoader> meshLoaders = new Dictionary<string, PD2MeshLoader>();
        private readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>(); 
        public Dictionary<int, SceneNode> UnitObjectTree = new Dictionary<int, SceneNode>(); 

        public PDMapUnit(string unitPath) : base(unitPath)
        {
            if (this.Error != null) return;
            foreach (var subModelName in this.SubModels.Keys)
            {
                meshLoaders[subModelName] = new PD2MeshLoader(this, subModelName);
                var newMesh = MeshManager.Instance.CreateManual(
                    unitPath + subModelName,
                    ResourceGroupManager.DefaultResourceGroupName,
                    meshLoaders[subModelName]);
                newMesh.Load();
                meshes[subModelName] = newMesh;
            }
        }

        public Mesh GetMesh(string subModelName)
        {
            return meshes.ContainsKey(subModelName) ? meshes[subModelName] : null;
        }
    }
}
