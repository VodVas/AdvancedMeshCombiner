#if UNITY_EDITOR
using System.Collections.Generic;

namespace VodVas.AdvancedMeshCombiner
{
    public struct ChunkStatistics
    {
        public int TotalChunks;
        public int MaxChunkVertices;
        public int MinChunkVertices;
        public int AvgChunkVertices;
        public int TotalDrawCalls;
        public int TotalVertices;
        public int TotalTriangles;
        public int TotalColliderVertices;
        public List<string> MaterialGroups;
        public string ResultName;
        public bool IndividualCollidersPreserved;
        public int PreservedColliderCount;

        public ChunkStatistics(string resultName)
        {
            TotalChunks = 0;
            MaxChunkVertices = 0;
            MinChunkVertices = int.MaxValue;
            AvgChunkVertices = 0;
            TotalDrawCalls = 0;
            TotalVertices = 0;
            TotalTriangles = 0;
            TotalColliderVertices = 0;
            MaterialGroups = new List<string>();
            ResultName = resultName;
            IndividualCollidersPreserved = false;
            PreservedColliderCount = 0;
        }
    }
}
#endif