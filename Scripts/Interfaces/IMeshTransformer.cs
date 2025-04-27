#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IMeshTransformer
    {
        Vector3[] TransformVertices(Vector3[] vertices, Matrix4x4 matrix);
    }
}
#endif