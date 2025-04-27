#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IOriginalObjectProcessor
    {
        void ProcessOriginalObjects(IReadOnlyList<Transform> children, GameObject result, MeshCombineSettings settings);
    }
}
#endif