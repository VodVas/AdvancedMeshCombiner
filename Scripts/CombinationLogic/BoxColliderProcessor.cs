#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class BoxColliderProcessor : IColliderProcessor
    {
        private readonly PrimitiveColliderMeshCache _meshCache;

        public BoxColliderProcessor(PrimitiveColliderMeshCache meshCache)
        {
            _meshCache = meshCache;
        }

        public bool CanProcess(Collider collider)
        {
            return collider is BoxCollider;
        }

        public CombineInstance CreateCombineInstance(Collider collider, Transform transform)
        {
            var boxCollider = (BoxCollider)collider;
            Mesh boxMesh = _meshCache.GetBoxMesh(boxCollider.size);
            Matrix4x4 colliderMatrix = Matrix4x4.TRS(
                boxCollider.center,
                Quaternion.identity,
                Vector3.one
            );

            return new CombineInstance
            {
                mesh = boxMesh,
                subMeshIndex = 0,
                transform = transform.localToWorldMatrix * colliderMatrix
            };
        }

        public int GetApproximateVertexCount(Collider collider)
        {
            return 8;
        }
    }
}
#endif