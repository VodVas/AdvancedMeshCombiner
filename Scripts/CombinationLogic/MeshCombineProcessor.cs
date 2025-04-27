#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshCombineProcessor
    {
        private readonly IMeshChunkBuilder _chunkBuilder;
        private readonly IColliderGenerator _colliderGenerator;
        private readonly IMeshTransformer _meshTransformer;
        private readonly IOriginalObjectProcessor _originalProcessor;
        private readonly IUndoService _undoService;
        private PrimitiveColliderMeshCache _meshCache;

        public MeshCombineProcessor(
            IMeshChunkBuilder chunkBuilder,
            IColliderGenerator colliderGenerator,
            IMeshTransformer meshTransformer,
            IOriginalObjectProcessor originalProcessor,
            IUndoService undoService)
        {
            _chunkBuilder = chunkBuilder;
            _colliderGenerator = colliderGenerator as ColliderGenerator;
            _meshTransformer = meshTransformer;
            _originalProcessor = originalProcessor;
            _undoService = undoService;
            _meshCache = new PrimitiveColliderMeshCache();
        }

        public MeshCombineResult CombineMeshes(MeshCombineSettings settings)
        {
            List<Transform> allChildren = GetChildrenRecursive(settings.ParentObject);

            if (allChildren.Count == 0)
            {
                Debug.LogError("No valid child objects found!");
                return null;
            }

            List<MeshChunk> chunks = _chunkBuilder.BuildChunks(allChildren, settings);

            if (chunks.Count == 0)
            {
                Debug.LogError("No valid meshes found to combine!");
                return null;
            }

            GameObject chunkParent = new GameObject($"{settings.ParentObject.name}_Combined");
            chunkParent.transform.position = settings.ParentObject.position;
            chunkParent.transform.rotation = settings.ParentObject.rotation;
            chunkParent.transform.parent = settings.CreateAsChild ? settings.ParentObject : settings.ParentObject.parent;

            _undoService.RegisterCreatedObject(chunkParent, "Combined Mesh Parent");

            ChunkStatistics stats = new ChunkStatistics(chunkParent.name);
            stats.TotalChunks = chunks.Count;
            List<GameObject> chunkObjects = new List<GameObject>(chunks.Count);
            List<CombineInstance> allColliders = new List<CombineInstance>();

            for (int i = 0; i < chunks.Count; i++)
            {
                MeshChunk chunk = chunks[i];

                GameObject chunkObject = CreateChunkObject(chunk, i, chunkParent, settings.ParentObject, ref stats);
                chunkObjects.Add(chunkObject);

                if (settings.GenerateColliders && !settings.PreserveIndividualColliders)
                {
                    if (settings.CreateSharedCollider)
                    {
                        allColliders.AddRange(chunk.Colliders);
                    }
                    else if (chunk.Colliders.Count > 0)
                    {
                        Mesh colliderMesh = ((ColliderGenerator)_colliderGenerator).GenerateCollider(chunkObject, chunk, settings.ParentObject);
                        if (colliderMesh != null)
                        {
                            stats.TotalColliderVertices += colliderMesh.vertexCount;
                        }
                    }
                }

                if (chunk.Material != null && !stats.MaterialGroups.Contains(chunk.Material.name))
                {
                    stats.MaterialGroups.Add(chunk.Material.name);
                }
            }

            if (settings.GenerateColliders)
            {
                if (settings.PreserveIndividualColliders)
                {
                    ((ColliderGenerator)_colliderGenerator).PreserveOriginalColliders(
                        chunkParent,
                        allChildren,
                        settings.ParentObject,
                        _undoService
                    );

                    int colliderCount = 0;
                    foreach (Transform child in allChildren)
                    {
                        colliderCount += child.GetComponents<Collider>().Length;
                    }

                    stats.IndividualCollidersPreserved = true;
                    stats.PreservedColliderCount = colliderCount;
                }
                else if (settings.CreateSharedCollider && allColliders.Count > 0 && !settings.ProcessPrimitiveColliders)
                {
                    Mesh colliderMesh = _colliderGenerator.GenerateSharedCollider(allColliders, settings.ParentObject);
                    if (colliderMesh != null)
                    {
                        MeshCollider collider = chunkParent.AddComponent<MeshCollider>();
                        collider.sharedMesh = colliderMesh;
                        collider.convex = false;

                        stats.TotalColliderVertices = colliderMesh.vertexCount;
                    }
                }

                if (settings.ProcessPrimitiveColliders && !settings.PreserveIndividualColliders)
                {
                    ProcessPrimitiveColliders(chunkParent, allChildren, settings);
                }
            }

            stats.AvgChunkVertices = stats.TotalVertices / chunks.Count;
            stats.TotalDrawCalls = chunks.Count;

            _originalProcessor.ProcessOriginalObjects(allChildren, chunkParent, settings);

            return new MeshCombineResult(chunkParent, stats, chunkObjects);
        }

        private void ProcessPrimitiveColliders(GameObject chunkParent, List<Transform> allChildren, MeshCombineSettings settings)
        {
            _meshCache = new PrimitiveColliderMeshCache(settings.PrimitiveColliderDetail);
            List<ColliderInfo> colliderInfos = ((ColliderGenerator)_colliderGenerator).CollectAllColliders(allChildren);
            var processorFactory = new ColliderProcessorFactory(_meshCache);
            var colliderGrouper = new ColliderGrouper(processorFactory, settings.ColliderGroupStrategy);

            Dictionary<string, List<CombineInstance>> colliderGroups =
                colliderGrouper.GroupColliders(colliderInfos, settings.MaxVerticesPerChunk);

            ((ColliderGenerator)_colliderGenerator).GenerateGroupedColliders(
                chunkParent,
                colliderGroups,
                settings.ParentObject,
                settings
            );
        }

        private GameObject CreateChunkObject(MeshChunk chunk, int index, GameObject parent, Transform originalParent, ref ChunkStatistics stats)
        {
            Mesh chunkMesh = CreateChunkMesh(chunk, originalParent);

            stats.MaxChunkVertices = Mathf.Max(stats.MaxChunkVertices, chunkMesh.vertexCount);
            stats.MinChunkVertices = Mathf.Min(stats.MinChunkVertices, chunkMesh.vertexCount);
            stats.TotalVertices += chunkMesh.vertexCount;
            stats.TotalTriangles += chunkMesh.triangles.Length / 3;

            GameObject chunkObject = new GameObject($"Chunk_{index + 1}_{chunk.Material?.name ?? "NullMaterial"}");
            chunkObject.transform.parent = parent.transform;
            chunkObject.transform.localPosition = Vector3.zero;
            chunkObject.transform.localRotation = Quaternion.identity;

            MeshFilter mf = chunkObject.AddComponent<MeshFilter>();
            mf.sharedMesh = chunkMesh;

            MeshRenderer mr = chunkObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = chunk.Material;

            _undoService.RegisterCreatedObject(chunkObject, "Chunk Object");

            return chunkObject;
        }

        private Mesh CreateChunkMesh(MeshChunk chunk, Transform parent)
        {
            Mesh mesh = new Mesh();

            if (chunk.VertexCount > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.CombineMeshes(chunk.Renderers.ToArray(), true);
            mesh.vertices = _meshTransformer.TransformVertices(mesh.vertices, parent.localToWorldMatrix.inverse);
            mesh.RecalculateBounds();
            mesh.Optimize();
            mesh.UploadMeshData(false);

            return mesh;
        }

        private List<Transform> GetChildrenRecursive(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            int childCount = parent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                children.Add(child);

                if (child.childCount > 0)
                {
                    children.AddRange(GetChildrenRecursive(child));
                }
            }

            return children;
        }
    }
}
#endif