#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IColliderGenerator
    {
        Mesh GenerateCollider(GameObject target, MeshChunk chunk, Transform parent);
        Mesh GenerateSharedCollider(IReadOnlyList<CombineInstance> allColliders, Transform parent);
        void GenerateGroupedColliders(
            GameObject parentObject,
            Dictionary<string, List<CombineInstance>> colliderGroups,
            Transform originalParent,
            MeshCombineSettings settings);
        List<ColliderInfo> CollectAllColliders(IEnumerable<Transform> children);
        void PreserveOriginalColliders(GameObject parentObject, List<Transform> originalObjects, Transform originalParent, IUndoService undoService);
        void ClearCache();
    }
}
#endif