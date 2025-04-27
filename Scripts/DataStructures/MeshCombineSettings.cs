#if UNITY_EDITOR
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public struct MeshCombineSettings
    {
        public Transform ParentObject;
        public bool RemoveOriginals;
        public bool CreateAsChild;
        public bool GenerateColliders;
        public bool CreateSharedCollider;
        public bool CombineByMaterial;
        public ChunkMode ChunkMode;
        public int MaxVerticesPerChunk;
        public bool ProcessPrimitiveColliders;
        public ColliderGroupingStrategy ColliderGroupStrategy;
        public DetailLevel PrimitiveColliderDetail;
        public bool PreserveIndividualColliders;

        public MeshCombineSettings(
            Transform parentObject,
            bool removeOriginals = true,
            bool createAsChild = true,
            bool generateColliders = true,
            bool createSharedCollider = true,
            bool combineByMaterial = true,
            ChunkMode chunkMode = ChunkMode.Auto,
            int maxVerticesPerChunk = 65000,
            bool processPrimitiveColliders = true,
            ColliderGroupingStrategy colliderGroupStrategy = ColliderGroupingStrategy.TypeBased,
            DetailLevel primitiveColliderDetail = DetailLevel.Medium,
            bool preserveIndividualColliders = false)
        {
            ParentObject = parentObject;
            RemoveOriginals = removeOriginals;
            CreateAsChild = createAsChild;
            GenerateColliders = generateColliders;
            CreateSharedCollider = createSharedCollider;
            CombineByMaterial = combineByMaterial;
            ChunkMode = chunkMode;
            MaxVerticesPerChunk = maxVerticesPerChunk;
            ProcessPrimitiveColliders = processPrimitiveColliders;
            ColliderGroupStrategy = colliderGroupStrategy;
            PrimitiveColliderDetail = primitiveColliderDetail;
            PreserveIndividualColliders = preserveIndividualColliders;
        }
    }
}
#endif