#if UNITY_EDITOR
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshTransformer : IMeshTransformer
    {
        public Vector3[] TransformVertices(Vector3[] vertices, Matrix4x4 matrix)
        {
            int length = vertices.Length;

            if (length == 0) return vertices;

            NativeArray<float3> inputVertices = new NativeArray<float3>(length, Allocator.TempJob);
            NativeArray<float3> outputVertices = new NativeArray<float3>(length, Allocator.TempJob);

            for (int i = 0; i < length; i++)
            {
                inputVertices[i] = vertices[i];
            }

            float4x4 jobMatrix = new float4x4(
                matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                matrix.m30, matrix.m31, matrix.m32, matrix.m33
            );

            var job = new TransformVerticesJob
            {
                InputVertices = inputVertices,
                OutputVertices = outputVertices,
                TransformMatrix = jobMatrix
            };

            JobHandle handle = job.Schedule(length, 64);
            handle.Complete();

            Vector3[] result = new Vector3[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = outputVertices[i];
            }

            inputVertices.Dispose();
            outputVertices.Dispose();

            return result;
        }

        [BurstCompile]
        private struct TransformVerticesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> InputVertices;
            [WriteOnly] public NativeArray<float3> OutputVertices;
            public float4x4 TransformMatrix;

            public void Execute(int index)
            {
                float3 vertex = InputVertices[index];
                float4 transformedVertex = math.mul(TransformMatrix, new float4(vertex, 1));
                OutputVertices[index] = transformedVertex.xyz;
            }
        }
    }
}
#endif