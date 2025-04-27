#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class SphereColliderProcessor : IColliderProcessor
    {
        private readonly PrimitiveColliderMeshCache _meshCache;

        public SphereColliderProcessor(PrimitiveColliderMeshCache meshCache)
        {
            _meshCache = meshCache;
        }

        public bool CanProcess(Collider collider)
        {
            return collider is SphereCollider;
        }

        public CombineInstance CreateCombineInstance(Collider collider, Transform transform)
        {
            var sphereCollider = (SphereCollider)collider;

            Mesh sphereMesh = _meshCache.GetSphereMesh(sphereCollider.radius);

            Matrix4x4 colliderMatrix = Matrix4x4.TRS(
                sphereCollider.center,
                Quaternion.identity,
                Vector3.one
            );

            return new CombineInstance
            {
                mesh = sphereMesh,
                subMeshIndex = 0,
                transform = transform.localToWorldMatrix * colliderMatrix
            };
        }

        public int GetApproximateVertexCount(Collider collider)
        {
            return 162;
        }
    }
}
#endif