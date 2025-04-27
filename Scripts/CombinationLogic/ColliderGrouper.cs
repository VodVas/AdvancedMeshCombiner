#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class ColliderGrouper
    {
        private readonly ColliderProcessorFactory _processorFactory;
        private readonly ColliderGroupingStrategy _strategy;

        public ColliderGrouper(ColliderProcessorFactory processorFactory, ColliderGroupingStrategy strategy)
        {
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _strategy = strategy;
        }

        public Dictionary<string, List<CombineInstance>> GroupColliders(List<ColliderInfo> colliderInfos, int maxVerticesPerGroup)
        {
            Dictionary<string, List<CombineInstance>> colliderGroups = new Dictionary<string, List<CombineInstance>>();
            Dictionary<string, int> vertexCounts = new Dictionary<string, int>();

            foreach (var colliderInfo in colliderInfos)
            {
                if (colliderInfo.Collider == null || colliderInfo.Transform == null)
                    continue;

                IColliderProcessor processor = _processorFactory.GetProcessorFor(colliderInfo.Collider);
                if (processor == null)
                    continue;

                string groupKey = GetGroupKey(colliderInfo, processor);

                int approximateVertexCount = processor.GetApproximateVertexCount(colliderInfo.Collider);

                if (vertexCounts.TryGetValue(groupKey, out int currentVertexCount) &&
                    currentVertexCount + approximateVertexCount > maxVerticesPerGroup)
                {
                    int groupNumber = 1;
                    string newGroupKey;
                    do
                    {
                        newGroupKey = $"{groupKey}_{groupNumber++}";
                    } while (vertexCounts.ContainsKey(newGroupKey));

                    groupKey = newGroupKey;
                    vertexCounts[groupKey] = 0;
                }

                if (!colliderGroups.ContainsKey(groupKey))
                {
                    colliderGroups[groupKey] = new List<CombineInstance>();
                    vertexCounts[groupKey] = 0;
                }

                CombineInstance ci = processor.CreateCombineInstance(colliderInfo.Collider, colliderInfo.Transform);
                colliderGroups[groupKey].Add(ci);

                vertexCounts[groupKey] += approximateVertexCount;
            }

            return colliderGroups;
        }

        private string GetGroupKey(ColliderInfo info, IColliderProcessor processor)
        {
            switch (_strategy)
            {
                case ColliderGroupingStrategy.TypeBased:
                    return info.Collider.GetType().Name;

                case ColliderGroupingStrategy.Mixed:
                    return "MixedColliders";

                default:
                    throw new ArgumentOutOfRangeException(nameof(_strategy), $"Unknown grouping strategy: {_strategy}");
            }
        }
    }
}
#endif