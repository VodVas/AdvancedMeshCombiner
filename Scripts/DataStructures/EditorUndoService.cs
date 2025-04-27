#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VodVas.AdvancedMeshCombiner
{
    public class EditorUndoService : IUndoService
    {
        public void RegisterCreatedObject(GameObject obj, string operationName)
        {
            Undo.RegisterCreatedObjectUndo(obj, operationName);
        }

        public void SetTransformParent(Transform child, Transform parent, string operationName)
        {
            Undo.SetTransformParent(child, parent, operationName);
        }

        public void DestroyComponent(Component component)
        {
            Undo.DestroyObjectImmediate(component);
        }

        public void RecordObject(Object obj, string operationName)
        {
            Undo.RecordObject(obj, operationName);
        }

        public int BeginUndoGroup(string name)
        {
            Undo.SetCurrentGroupName(name);
            return Undo.GetCurrentGroup();
        }

        public void EndUndoGroup(int group)
        {
            Undo.CollapseUndoOperations(group);
        }
    }
}
#endif