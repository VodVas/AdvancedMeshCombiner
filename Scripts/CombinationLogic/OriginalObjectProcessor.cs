#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class OriginalObjectProcessor : IOriginalObjectProcessor
    {
        private readonly IUndoService _undoService;

        public OriginalObjectProcessor(IUndoService undoService)
        {
            _undoService = undoService;
        }

        public void ProcessOriginalObjects(IReadOnlyList<Transform> children, GameObject result, MeshCombineSettings settings)
        {
            int group = _undoService.BeginUndoGroup("Process Original Meshes");

            CleanupExistingCopies(children);

            foreach (Transform t in children)
            {
                if (t == null) continue;

                if (settings.RemoveOriginals)
                {
                    _undoService.RecordObject(t.gameObject, "Disable Original");
                    t.gameObject.SetActive(false);
                    _undoService.SetTransformParent(t, result.transform, "Move Original");
                    RemoveComponents(t);
                }
            }

            _undoService.EndUndoGroup(group);
        }

        private void CleanupExistingCopies(IReadOnlyList<Transform> children)
        {
            foreach (Transform child in children)
            {
                if (child == null) continue;

                Transform parent = child.parent;
                if (parent == null) continue;

                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    Transform sibling = parent.GetChild(i);

                    if (sibling != null && sibling.name.EndsWith("_Copy"))
                    {
                        string baseName = sibling.name.Substring(0, sibling.name.Length - 5);
                        if (child.name == baseName)
                        {
                            _undoService.RecordObject(parent.gameObject, "Remove Copy");
                            Object.DestroyImmediate(sibling.gameObject);
                        }
                    }
                }
            }
        }

        private void RemoveComponents(Transform t)
        {
            Component[] components = {
                t.GetComponent<MeshRenderer>(),
                t.GetComponent<MeshFilter>(),
                t.GetComponent<MeshCollider>()
            };

            foreach (Component c in components)
            {
                if (c != null)
                {
                    _undoService.DestroyComponent(c);
                }
            }
        }
    }
}
#endif