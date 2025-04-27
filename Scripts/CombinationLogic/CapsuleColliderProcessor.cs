#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class CapsuleColliderProcessor : IColliderProcessor
    {
        private readonly PrimitiveColliderMeshCache _meshCache;

        public CapsuleColliderProcessor(PrimitiveColliderMeshCache meshCache)
        {
            _meshCache = meshCache;
        }

        public bool CanProcess(Collider collider)
        {
            return collider is CapsuleCollider;
        }

        public CombineInstance CreateCombineInstance(Collider collider, Transform transform)
        {
            var capsuleCollider = (CapsuleCollider)collider;

            float radius = Mathf.Max(0.01f, capsuleCollider.radius);
            float height = Mathf.Max(radius * 2.01f, capsuleCollider.height);
            int direction = Mathf.Clamp(capsuleCollider.direction, 0, 2);

            Mesh capsuleMesh = _meshCache.GetCapsuleMesh(
                radius,
                height,
                direction
            );

            Matrix4x4 colliderMatrix = Matrix4x4.TRS(
                capsuleCollider.center,
                Quaternion.identity,
                Vector3.one
            );

            return new CombineInstance
            {
                mesh = capsuleMesh,
                subMeshIndex = 0,
                transform = transform.localToWorldMatrix * colliderMatrix
            };
        }

        public int GetApproximateVertexCount(Collider collider)
        {
            return 256;
        }
    }
}
#endif