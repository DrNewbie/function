// -----------------------------------------------------------------------
// <copyright file="DieselGeometry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DieselUnit
{
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public struct DieselGeometry
    {

        public int SectionId;

        public int ParentId;

        public bool Enabled;

        public float[,] Orientation;

        public float[] Offset;

        public int VertexCount;

        public float[] Vertices;

        public float[] Normals;

        public float[] UVs;

        public float[] UnknownB;

        public float[] UnknownC;

        public int IndiceCount;

        public ushort[] Indices; 

        public float[] BoundsUpper;

        public float[] BoundsLower;

        public ulong NameHash;
    }
}
