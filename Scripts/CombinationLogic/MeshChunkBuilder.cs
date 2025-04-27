#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshChunkBuilder : IMeshChunkBuilder
    {
        private readonly Dictionary<Transform, List<Transform>> _childrenCache = new Dictionary<Transform, List<Transform>>();

        public List<MeshChunk> BuildChunks(IReadOnlyList<Transform> children, MeshCombineSettings settings)
        {
            var chunks = new List<MeshChunk>();
            var materialToChunks = new Dictionary<Material, List<MeshChunk>>();

            int maxVertices = GetEffectiveMaxVertices(settings);
            bool combineByMaterial = ShouldCombineByMaterial(settings);

            foreach (Transform child in children)
            {
                if (!TryGetMeshComponents(child, out MeshFilter filter, out MeshRenderer renderer, out MeshCollider collider))
                    continue;

                Mesh mesh = filter.sharedMesh;

                if (mesh == null)
                    continue;

                Material[] materials = GetMaterials(renderer, combineByMaterial);

                int submeshCount = GetSubmeshCount(mesh, materials);
                bool shouldCreateSeparateChunk = ShouldCreateSeparateChunk(mesh, settings);

                for (int i = 0; i < submeshCount; i++)
                {
                    Material material = (i < materials.Length) ? materials[i] : null;

                    CombineInstance ci = new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = i < mesh.subMeshCount ? i : 0,
                        transform = child.localToWorldMatrix
                    };

                    if (shouldCreateSeparateChunk)
                    {
                        MeshChunk separateChunk = new MeshChunk(material);
                        separateChunk.Renderers.Add(ci);
                        separateChunk.VertexCount = mesh.vertexCount;
                        separateChunk.SubMeshCount = 1;
                        chunks.Add(separateChunk);

                        AddColliderToCombineInstance(collider, child, separateChunk);

                        continue;
                    }

                    MeshChunk targetChunk = FindOrCreateChunkForMaterial(
                        material, materialToChunks, chunks, mesh.vertexCount, maxVertices);

                    targetChunk.Renderers.Add(ci);
                    targetChunk.VertexCount += mesh.vertexCount;
                    targetChunk.SubMeshCount++;

                    AddColliderToCombineInstance(collider, child, targetChunk);
                }
            }

            return chunks;
        }

        private int GetEffectiveMaxVertices(MeshCombineSettings settings)
        {
            return settings.ChunkMode == ChunkMode.NoSplit ? int.MaxValue : settings.MaxVerticesPerChunk;
        }

        private bool ShouldCombineByMaterial(MeshCombineSettings settings)
        {
            return settings.ChunkMode == ChunkMode.ForceSplit ? true : settings.CombineByMaterial;
        }

        private Material[] GetMaterials(MeshRenderer renderer, bool combineByMaterial)
        {
            if (renderer != null && renderer.sharedMaterials.Length > 0 && combineByMaterial)
            {
                return renderer.sharedMaterials;
            }
            else
            {
                return new Material[] { null };
            }
        }

        private int GetSubmeshCount(Mesh mesh, Material[] materials)
        {
            int submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);
            return submeshCount == 0 ? 1 : submeshCount;
        }

        private bool ShouldCreateSeparateChunk(Mesh mesh, MeshCombineSettings settings)
        {
            if (settings.ChunkMode == ChunkMode.Auto)
            {
                return mesh.vertexCount > settings.MaxVerticesPerChunk;
            }
            return false;
        }

        private void AddColliderToCombineInstance(MeshCollider collider, Transform child, MeshChunk chunk)
        {
            if (collider != null && collider.sharedMesh != null)
            {
                var colliderCombine = new CombineInstance
                {
                    mesh = collider.sharedMesh,
                    subMeshIndex = 0,
                    transform = child.localToWorldMatrix
                };
                chunk.Colliders.Add(colliderCombine);
            }
        }

        private MeshChunk FindOrCreateChunkForMaterial(
            Material material,
            Dictionary<Material, List<MeshChunk>> materialToChunks,
            List<MeshChunk> chunks,
            int vertexCount,
            int maxVertices)
        {
            if (!materialToChunks.TryGetValue(material, out List<MeshChunk> chunksForMaterial))
            {
                chunksForMaterial = new List<MeshChunk>();
                materialToChunks[material] = chunksForMaterial;
            }

            MeshChunk targetChunk = null;

            for (int i = 0; i < chunksForMaterial.Count; i++)
            {
                var chunk = chunksForMaterial[i];

                if (chunk.VertexCount + vertexCount <= maxVertices)
                {
                    targetChunk = chunk;
                    break;
                }
            }

            if (targetChunk == null)
            {
                targetChunk = new MeshChunk(material);
                chunksForMaterial.Add(targetChunk);
                chunks.Add(targetChunk);
            }

            return targetChunk;
        }

        private bool TryGetMeshComponents(Transform t, out MeshFilter filter, out MeshRenderer renderer, out MeshCollider collider)
        {
            filter = t.GetComponent<MeshFilter>();
            renderer = t.GetComponent<MeshRenderer>();
            collider = t.GetComponent<MeshCollider>();

            return filter != null && filter.sharedMesh != null;
        }
    }
}
#endif