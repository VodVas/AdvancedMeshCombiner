#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IColliderProcessor
    {
        bool CanProcess(Collider collider);
        int GetApproximateVertexCount(Collider collider);
        CombineInstance CreateCombineInstance(Collider collider, Transform transform);
    }
}
#endif