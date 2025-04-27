#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshColliderProcessor : IColliderProcessor
    {
        public bool CanProcess(Collider collider)
        {
            return collider is MeshCollider;
        }

        public CombineInstance CreateCombineInstance(Collider collider, Transform transform)
        {
            var meshCollider = (MeshCollider)collider;
            if (meshCollider.sharedMesh == null)
                return default;

            return new CombineInstance
            {
                mesh = meshCollider.sharedMesh,
                subMeshIndex = 0,
                transform = transform.localToWorldMatrix
            };
        }

        public int GetApproximateVertexCount(Collider collider)
        {
            var meshCollider = (MeshCollider)collider;
            if (meshCollider.sharedMesh == null)
                return 0;

            return meshCollider.sharedMesh.vertexCount;
        }
    }
}
#endif