#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshChunk
    {
        public MeshChunk(Material material)
        {
            Material = material;
            VertexCount = 0;
            SubMeshCount = 0;
        }

        public List<CombineInstance> Renderers { get; } = new List<CombineInstance>();
        public List<CombineInstance> Colliders { get; } = new List<CombineInstance>();
        public Material Material { get; }
        public int VertexCount { get; set; }
        public int SubMeshCount { get; set; }
    }
}
#endif