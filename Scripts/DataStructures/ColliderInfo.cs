#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public struct ColliderInfo
    {
        public Collider Collider;
        public Transform Transform;

        public ColliderInfo(Collider collider, Transform transform)
        {
            Collider = collider;
            Transform = transform;
        }
    }
}
#endif