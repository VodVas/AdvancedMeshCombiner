#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshCombineResult
    {
        public MeshCombineResult(GameObject combinedObject, ChunkStatistics statistics, List<GameObject> chunkObjects)
        {
            CombinedObject = combinedObject;
            Statistics = statistics;
            ChunkObjects = chunkObjects;
        }

        public GameObject CombinedObject { get; }
        public ChunkStatistics Statistics { get; }
        public List<GameObject> ChunkObjects { get; }
    }
}
#endif