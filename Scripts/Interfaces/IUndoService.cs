#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IUndoService
    {
        void RegisterCreatedObject(GameObject obj, string operationName);
        void SetTransformParent(Transform child, Transform parent, string operationName);
        void DestroyComponent(Component component);
        void RecordObject(Object obj, string operationName);
        int BeginUndoGroup(string name);
        void EndUndoGroup(int group);
    }
}
#endif