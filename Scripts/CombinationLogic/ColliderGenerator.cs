#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class ColliderGenerator : IColliderGenerator
    {
        private readonly IMeshTransformer _meshTransformer;
        private readonly PrimitiveColliderMeshCache _meshCache;
        private readonly ColliderProcessorFactory _processorFactory;

        public ColliderGenerator(IMeshTransformer meshTransformer)
        {
            _meshTransformer = meshTransformer ?? throw new ArgumentNullException(nameof(meshTransformer));
            _meshCache = new PrimitiveColliderMeshCache();
            _processorFactory = new ColliderProcessorFactory(_meshCache);
        }

        public void PreserveOriginalColliders(GameObject parentObject, List<Transform> originalObjects, Transform originalParent, IUndoService undoService)
        {
            GameObject collidersContainer = new GameObject("Original_Colliders");
            collidersContainer.transform.SetParent(parentObject.transform);
            collidersContainer.transform.localPosition = Vector3.zero;
            collidersContainer.transform.localRotation = Quaternion.identity;
            collidersContainer.transform.localScale = Vector3.one;

            undoService.RegisterCreatedObject(collidersContainer, "Original Colliders Container");

            Dictionary<Transform, GameObject> objectMap = new Dictionary<Transform, GameObject>();

            foreach (Transform original in originalObjects)
            {
                Collider[] colliders = original.GetComponents<Collider>();
                if (colliders.Length == 0) continue;

                GameObject colliderObject = new GameObject(original.name + "_Collider");
                colliderObject.transform.SetParent(collidersContainer.transform);

                colliderObject.transform.position = original.position;
                colliderObject.transform.rotation = original.rotation;
                colliderObject.transform.localScale = original.lossyScale;

                undoService.RegisterCreatedObject(colliderObject, "Collider Object");

                objectMap[original] = colliderObject;

                foreach (Collider collider in colliders)
                {
                    CopyColliderToObject(collider, colliderObject, undoService);
                }
            }

            foreach (Transform original in originalObjects)
            {
                if (!objectMap.TryGetValue(original, out GameObject colliderObject)) continue;

                if (original.parent != null && objectMap.TryGetValue(original.parent, out GameObject parentColliderObject))
                {
                    undoService.SetTransformParent(colliderObject.transform, parentColliderObject.transform, "Restore Hierarchy");

                    colliderObject.transform.localPosition = original.localPosition;
                    colliderObject.transform.localRotation = original.localRotation;
                    colliderObject.transform.localScale = original.localScale;
                }
            }
        }

        private void CopyColliderToObject(Collider source, GameObject target, IUndoService undoService)
        {
            Collider copy = null;

            if (source is BoxCollider boxSource)
            {
                BoxCollider boxCopy = target.AddComponent<BoxCollider>();
                boxCopy.center = boxSource.center;
                boxCopy.size = boxSource.size;
                copy = boxCopy;
            }
            else if (source is SphereCollider sphereSource)
            {
                SphereCollider sphereCopy = target.AddComponent<SphereCollider>();
                sphereCopy.center = sphereSource.center;
                sphereCopy.radius = sphereSource.radius;
                copy = sphereCopy;
            }
            else if (source is CapsuleCollider capsuleSource)
            {
                CapsuleCollider capsuleCopy = target.AddComponent<CapsuleCollider>();
                capsuleCopy.center = capsuleSource.center;
                capsuleCopy.radius = capsuleSource.radius;
                capsuleCopy.height = capsuleSource.height;
                capsuleCopy.direction = capsuleSource.direction;
                copy = capsuleCopy;
            }
            else if (source is MeshCollider meshSource)
            {
                MeshCollider meshCopy = target.AddComponent<MeshCollider>();
                meshCopy.sharedMesh = meshSource.sharedMesh;
                meshCopy.convex = meshSource.convex;
#if UNITY_2020_1_OR_NEWER
                meshCopy.cookingOptions = meshSource.cookingOptions;
#endif
                copy = meshCopy;
            }

            if (copy != null)
            {
                copy.isTrigger = source.isTrigger;
                copy.sharedMaterial = source.sharedMaterial;
                copy.enabled = source.enabled;

                undoService.RecordObject(copy, "Copy Collider Properties");
            }
        }

        public Mesh GenerateCollider(GameObject target, MeshChunk chunk, Transform parent)
        {
            if (chunk.Colliders.Count == 0) return null;

            Mesh colliderMesh = new Mesh();

            colliderMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            colliderMesh.CombineMeshes(chunk.Colliders.ToArray(), true);
            colliderMesh.vertices = _meshTransformer.TransformVertices(colliderMesh.vertices, parent.localToWorldMatrix.inverse);
            colliderMesh.RecalculateBounds();
            colliderMesh.Optimize();
            colliderMesh.UploadMeshData(true);

            MeshCollider collider = target.AddComponent<MeshCollider>();

            collider.sharedMesh = colliderMesh;
            collider.convex = colliderMesh.vertexCount < 255;

            return colliderMesh;
        }

        public Mesh GenerateSharedCollider(IReadOnlyList<CombineInstance> allColliders, Transform parent)
        {
            if (allColliders.Count == 0) return null;

            Mesh colliderMesh = new Mesh();
            colliderMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            colliderMesh.CombineMeshes(allColliders.ToArray(), true);
            colliderMesh.vertices = _meshTransformer.TransformVertices(
                colliderMesh.vertices, parent.localToWorldMatrix.inverse);
            colliderMesh.UploadMeshData(true);

            return colliderMesh;
        }

        public void GenerateGroupedColliders(
            GameObject parentObject,
            Dictionary<string, List<CombineInstance>> colliderGroups,
            Transform originalParent,
            MeshCombineSettings settings)
        {
            if (settings.CreateSharedCollider && colliderGroups.Count > 0)
            {
                var allColliders = colliderGroups.Values
                    .SelectMany(list => list)
                    .ToList();

                Mesh colliderMesh = GenerateSharedCollider(allColliders, originalParent);

                if (colliderMesh != null)
                {
                    MeshCollider collider = parentObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = colliderMesh;
                    collider.convex = colliderMesh.vertexCount < 255;
                }

                return;
            }

            foreach (var group in colliderGroups)
            {
                if (group.Value.Count == 0)
                    continue;

                GameObject colliderObject = new GameObject($"Collider_{group.Key}");
                colliderObject.transform.SetParent(parentObject.transform);
                colliderObject.transform.localPosition = Vector3.zero;
                colliderObject.transform.localRotation = Quaternion.identity;

                Mesh colliderMesh = new Mesh();
                colliderMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                colliderMesh.CombineMeshes(group.Value.ToArray(), true);
                colliderMesh.vertices = _meshTransformer.TransformVertices(
                    colliderMesh.vertices, originalParent.localToWorldMatrix.inverse);
                colliderMesh.RecalculateBounds();
                colliderMesh.Optimize();
                colliderMesh.UploadMeshData(true);

                MeshCollider collider = colliderObject.AddComponent<MeshCollider>();
                collider.sharedMesh = colliderMesh;
                collider.convex = colliderMesh.vertexCount < 255;
            }
        }

        public List<ColliderInfo> CollectAllColliders(IEnumerable<Transform> children)
        {
            List<ColliderInfo> result = new List<ColliderInfo>();

            foreach (Transform child in children)
            {
                Collider[] colliders = child.GetComponents<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider != null)
                    {
                        result.Add(new ColliderInfo(collider, child));
                    }
                }
            }

            return result;
        }

        public void ClearCache()
        {
            _meshCache.ClearCache();
        }
    }
}
#endif