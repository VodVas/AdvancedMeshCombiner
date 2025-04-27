#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class ColliderProcessorFactory
    {
        private readonly List<IColliderProcessor> _processors = new List<IColliderProcessor>();
        private readonly PrimitiveColliderMeshCache _meshCache;

        public ColliderProcessorFactory(PrimitiveColliderMeshCache meshCache)
        {
            _meshCache = meshCache ?? throw new ArgumentNullException(nameof(meshCache));

            _processors.Add(new MeshColliderProcessor());
            _processors.Add(new BoxColliderProcessor(_meshCache));
            _processors.Add(new SphereColliderProcessor(_meshCache));
            _processors.Add(new CapsuleColliderProcessor(_meshCache));
        }

        public IColliderProcessor GetProcessorFor(Collider collider)
        {
            if (collider == null) return null;

            foreach (var processor in _processors)
            {
                if (processor.CanProcess(collider))
                {
                    return processor;
                }
            }

            return null;
        }
    }
}
#endif