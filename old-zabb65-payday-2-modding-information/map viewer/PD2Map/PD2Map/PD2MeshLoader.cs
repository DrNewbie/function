using System;
using Axiom.Core;
using Axiom.Graphics;

namespace PD2Map
{
    using Axiom.Math;

    class PD2MeshLoader : IManualResourceLoader
    {
        private DieselUnit.DieselUnit unit;

        private readonly string subModelName;

        public PD2MeshLoader(DieselUnit.DieselUnit unit, string subModelName)
        {
            this.unit = unit;
            this.subModelName = subModelName;
        }

        private void SetBoundingBox(Mesh mesh)
        {
            var unitSubMesh = unit.SubModels[this.subModelName];
            var boundsLower = new Vector3(
                unitSubMesh.BoundsLower[0],
                unitSubMesh.BoundsLower[1],
                unitSubMesh.BoundsLower[2]);
            boundsLower *= 50000.0; //The "I'm tired of messing with this" factor.
            var boundsUpper = new Vector3(
                unitSubMesh.BoundsUpper[0],
                unitSubMesh.BoundsUpper[1],
                unitSubMesh.BoundsUpper[2]);
            boundsUpper *= 50000.0;
            mesh.BoundingBox = new AxisAlignedBox(boundsLower, boundsUpper);
        }

        public void LoadResource(Resource newMesh)
        {
            if (!unit.SubModels.ContainsKey(this.subModelName)) return;
            Mesh mesh = (Mesh)newMesh;
            SubMesh subMesh = mesh.CreateSubMesh(this.subModelName);
            subMesh.useSharedVertices = false;
            subMesh.MaterialName = "default_pd2";
            this.CreateIndexBuffer(subMesh);
            this.CreateVertexData(subMesh);
            subMesh.LodFaceList.Add(subMesh.indexData);
            SetBoundingBox(mesh);
        }

        void CreateIndexBuffer(SubMesh mesh)
        {
            var unitSubModel = unit.SubModels[this.subModelName];
            var indiceCount = unitSubModel.IndiceCount;
            mesh.indexData.indexStart = 0;
            mesh.indexData.indexCount = indiceCount;
            mesh.indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, indiceCount, BufferUsage.StaticWriteOnly);
            var hwBuffer = mesh.indexData.indexBuffer;
            IntPtr dest = hwBuffer.Lock(BufferLocking.Discard);
            unsafe
            {
                ushort* pointer = (ushort*)dest.ToPointer();
                for (int i = 0; i < indiceCount; i++)
                {
                    pointer[i] = unitSubModel.Indices[i];
                }
            }
            hwBuffer.Unlock();
        }

        void FillFloatBuffer(HardwareVertexBuffer buffer, float[] input, int outOffset, int inOffset, int count)
        {
            IntPtr bufferPtr = buffer.Lock(BufferLocking.Discard);
            bufferPtr += outOffset;
            unsafe
            {
                float* dest = (float*)bufferPtr.ToPointer();
                for (uint i = 0; i < count; ++i)
                {
                    dest[i] = input[i+inOffset];
                }
            }
            buffer.Unlock();
        }

        HardwareVertexBuffer CreateStaticBuffer(int vertexSize, int vertexCount)
        {
            return HardwareBufferManager.Instance.CreateVertexBuffer(vertexSize, vertexCount, BufferUsage.StaticWriteOnly);
        }
        
        void CreateVertexData(SubMesh mesh)
        {
            var unitSubModel = unit.SubModels[this.subModelName];
            int vertexCount = unitSubModel.VertexCount;
            mesh.vertexData = new VertexData();
            VertexData vd = mesh.vertexData;
            vd.vertexCount = vertexCount;

            short currentBuffer = 0;
            if(unitSubModel.Vertices != null)
            {
                vd.vertexDeclaration.AddElement(currentBuffer, 0, VertexElementType.Float3, VertexElementSemantic.Position, 0);
                var posBuffer = CreateStaticBuffer(12, vertexCount);
                FillFloatBuffer(posBuffer, unitSubModel.Vertices, 0, 0, 3*vertexCount);
                vd.vertexBufferBinding.SetBinding(currentBuffer++, posBuffer);
            }
            if (unitSubModel.Normals != null)
            {
                vd.vertexDeclaration.AddElement(currentBuffer, 0, VertexElementType.Float3, VertexElementSemantic.Normal, 0);
                var normalBuffer = CreateStaticBuffer(12, vertexCount);
                FillFloatBuffer(normalBuffer, unitSubModel.Normals, 0, 0, 3*vertexCount);
                vd.vertexBufferBinding.SetBinding(currentBuffer++, normalBuffer);
            }
            /*
            if (unitSubModel.UnknownB != null)
            {
                vd.vertexDeclaration.AddElement(currentBuffer, 0, VertexElementType.Float3, VertexElementSemantic.Diffuse, 0);
                var diffuseBuffer = CreateStaticBuffer(12, vertexCount);
                FillFloatBuffer(diffuseBuffer, unitSubModel.UnknownB, 0, 0, 3 * vertexCount);
                vd.vertexBufferBinding.SetBinding(currentBuffer++, diffuseBuffer);
            }
            /*
            if (unitSubModel.UnknownC != null)
            {
                vd.vertexDeclaration.AddElement(currentBuffer, 0, VertexElementType.Float3, VertexElementSemantic.Diffuse, 0);
                var specularBuffer = CreateStaticBuffer(12, vertexCount);
                FillFloatBuffer(specularBuffer, unitSubModel.UnknownC, 0, 0, 3 * vertexCount);
                vd.vertexBufferBinding.SetBinding(currentBuffer++, specularBuffer);
            }
            */
        }
    }
}
