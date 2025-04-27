#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public interface IMeshChunkBuilder
    {
        List<MeshChunk> BuildChunks(IReadOnlyList<Transform> children, MeshCombineSettings settings);
    }
}
#endif