#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VodVas.AdvancedMeshCombiner
{
    public class PrimitiveColliderMeshCache
    {
        private const int LOW_DETAIL_SPHERE_SEGMENTS = 8;
        private const int MED_DETAIL_SPHERE_SEGMENTS = 12;
        private const int HIGH_DETAIL_SPHERE_SEGMENTS = 16;

        private readonly Dictionary<Vector3, Mesh> _boxMeshCache = new Dictionary<Vector3, Mesh>();
        private readonly Dictionary<float, Mesh> _sphereMeshCache = new Dictionary<float, Mesh>();
        private readonly Dictionary<int, Mesh> _capsuleMeshCache = new Dictionary<int, Mesh>();
        private readonly int _sphereSegments;
        private readonly int _hemisphereRings;

        public PrimitiveColliderMeshCache(DetailLevel detailLevel = DetailLevel.Medium)
        {
            _sphereSegments = detailLevel switch
            {
                DetailLevel.Low => LOW_DETAIL_SPHERE_SEGMENTS,
                DetailLevel.Medium => MED_DETAIL_SPHERE_SEGMENTS,
                DetailLevel.High => HIGH_DETAIL_SPHERE_SEGMENTS,
                _ => MED_DETAIL_SPHERE_SEGMENTS
            };
            _hemisphereRings = _sphereSegments / 4;
        }

        public Mesh GetBoxMesh(Vector3 size)
        {
            Vector3 key = new Vector3(
                Mathf.Round(size.x * 1000) / 1000,
                Mathf.Round(size.y * 1000) / 1000,
                Mathf.Round(size.z * 1000) / 1000
            );

            if (_boxMeshCache.TryGetValue(key, out Mesh cachedMesh))
                return cachedMesh;

            Mesh boxMesh = CreateBoxMesh(key);
            boxMesh.name = $"BoxCollider_{key}";
            _boxMeshCache[key] = boxMesh;
            return boxMesh;
        }

        public Mesh GetSphereMesh(float radius)
        {
            float key = Mathf.Round(radius * 1000) / 1000;

            if (_sphereMeshCache.TryGetValue(key, out Mesh cachedMesh))
                return cachedMesh;

            Mesh sphereMesh = CreateSphereMesh(key);
            sphereMesh.name = $"SphereCollider_{key}";
            _sphereMeshCache[key] = sphereMesh;
            return sphereMesh;
        }

        public Mesh GetCapsuleMesh(float radius, float height, int direction)
        {
            int key = CalculateCapsuleHash(radius, height, direction);
            if (_capsuleMeshCache.TryGetValue(key, out Mesh cachedMesh))
                return cachedMesh;

            Mesh capsuleMesh = CreateCapsuleMesh(radius, height, direction);
            capsuleMesh.name = $"CapsuleCollider_{radius}_{height}_{direction}";
            _capsuleMeshCache[key] = capsuleMesh;
            return capsuleMesh;
        }

        private int CalculateCapsuleHash(float radius, float height, int direction)
        {
            int r = Mathf.RoundToInt(radius * 1000);
            int h = Mathf.RoundToInt(height * 1000);
            return (r << 20) | (h << 8) | direction;
        }

        private Mesh CreateBoxMesh(Vector3 size)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[8];
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            float hz = size.z * 0.5f;

            vertices[0] = new Vector3(-hx, -hy, -hz);
            vertices[1] = new Vector3(hx, -hy, -hz);
            vertices[2] = new Vector3(hx, -hy, hz);
            vertices[3] = new Vector3(-hx, -hy, hz);
            vertices[4] = new Vector3(-hx, hy, -hz);
            vertices[5] = new Vector3(hx, hy, -hz);
            vertices[6] = new Vector3(hx, hy, hz);
            vertices[7] = new Vector3(-hx, hy, hz);

            int[] triangles = {
                0,2,1, 0,3,2,  // Bottom
                4,5,6, 4,6,7,  // Top
                0,1,5, 0,5,4,  // Front
                2,3,7, 2,7,6,  // Back
                0,4,7, 0,7,3,  // Left
                1,2,6, 1,6,5   // Right
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            return mesh;
        }

        private Mesh CreateSphereMesh(float radius)
        {
            Mesh mesh = new Mesh();
            int latitudes = _sphereSegments;
            int longitudes = _sphereSegments * 2;
            int vertexCount = (latitudes + 1) * (longitudes + 1);

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            int[] triangles = new int[latitudes * longitudes * 6];

            int index = 0;
            for (int lat = 0; lat <= latitudes; lat++)
            {
                float theta = lat * Mathf.PI / latitudes;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= longitudes; lon++)
                {
                    float phi = lon * 2 * Mathf.PI / longitudes;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    vertices[index] = new Vector3(
                        cosPhi * sinTheta,
                        cosTheta,
                        sinPhi * sinTheta
                    ) * radius;

                    uv[index] = new Vector2(
                        (float)lon / longitudes,
                        1 - (float)lat / latitudes
                    );

                    index++;
                }
            }

            index = 0;
            for (int lat = 0; lat < latitudes; lat++)
            {
                for (int lon = 0; lon < longitudes; lon++)
                {
                    int current = lat * (longitudes + 1) + lon;
                    int next = current + longitudes + 1;

                    triangles[index++] = current;
                    triangles[index++] = next + 1;
                    triangles[index++] = current + 1;

                    triangles[index++] = current;
                    triangles[index++] = next;
                    triangles[index++] = next + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            return mesh;
        }

        private Mesh CreateCapsuleMesh(float radius, float height, int direction)
        {
            Mesh mesh = new Mesh();
            float cylinderHeight = Mathf.Max(0, height - 2 * radius);
            Vector3 axis = GetDirectionVector(direction);
            Quaternion rotation = GetAxisRotation(direction);

            List<Vector3> vertices = new List<Vector3>(512);
            List<int> triangles = new List<int>(1024);

            // Lower Hemisphere
            GenerateHemisphere(radius, -cylinderHeight * 0.5f, axis, rotation, false, vertices, triangles);

            // Cylinder
            GenerateCylinder(radius, cylinderHeight, axis, rotation, vertices, triangles);

            // Upper Hemisphere
            GenerateHemisphere(radius, cylinderHeight * 0.5f, axis, rotation, true, vertices, triangles);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            return mesh;
        }

        private void GenerateHemisphere(float radius, float baseY, Vector3 axis, Quaternion rotation,
            bool isTop, List<Vector3> vertices, List<int> triangles)
        {
            int baseIndex = vertices.Count;
            float verticalSign = isTop ? 1f : -1f;

            for (int ring = 0; ring <= _hemisphereRings; ring++)
            {
                float theta = ring * Mathf.PI * 0.5f / _hemisphereRings;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                float y = cosTheta * verticalSign;
                float ringRadius = sinTheta;

                for (int seg = 0; seg <= _sphereSegments; seg++)
                {
                    float phi = seg * 2 * Mathf.PI / _sphereSegments;
                    float cosPhi = Mathf.Cos(phi);
                    float sinPhi = Mathf.Sin(phi);

                    Vector3 point = new Vector3(
                        ringRadius * cosPhi,
                        y,
                        ringRadius * sinPhi
                    ) * radius;

                    point += axis * baseY;
                    vertices.Add(rotation * point);
                }
            }

            for (int ring = 0; ring < _hemisphereRings; ring++)
            {
                for (int seg = 0; seg < _sphereSegments; seg++)
                {
                    int current = baseIndex + ring * (_sphereSegments + 1) + seg;
                    int next = current + _sphereSegments + 1;

                    if (isTop)
                    {
                        triangles.Add(current);
                        triangles.Add(next + 1);
                        triangles.Add(current + 1);

                        triangles.Add(current);
                        triangles.Add(next);
                        triangles.Add(next + 1);
                    }
                    else
                    {
                        triangles.Add(current);
                        triangles.Add(current + 1);
                        triangles.Add(next + 1);

                        triangles.Add(current);
                        triangles.Add(next + 1);
                        triangles.Add(next);
                    }
                }
            }
        }

        private void GenerateCylinder(float radius, float height, Vector3 axis, Quaternion rotation,
            List<Vector3> vertices, List<int> triangles)
        {
            if (height <= 0) return;

            int baseIndex = vertices.Count;
            float halfHeight = height * 0.5f;
            Vector3 baseCenter = axis * -halfHeight;
            Vector3 topCenter = axis * halfHeight;

            for (int seg = 0; seg <= _sphereSegments; seg++)
            {
                float phi = seg * 2 * Mathf.PI / _sphereSegments;
                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);

                Vector3 circlePoint = new Vector3(cosPhi, 0, sinPhi) * radius;
                vertices.Add(rotation * (circlePoint + baseCenter));
                vertices.Add(rotation * (circlePoint + topCenter));
            }

            for (int seg = 0; seg < _sphereSegments; seg++)
            {
                int offset = baseIndex + seg * 2;
                triangles.Add(offset);
                triangles.Add(offset + 1);
                triangles.Add(offset + 2);

                triangles.Add(offset + 2);
                triangles.Add(offset + 1);
                triangles.Add(offset + 3);
            }
        }

        private Vector3 GetDirectionVector(int direction)
        {
            return direction switch
            {
                0 => Vector3.right,
                2 => Vector3.forward,
                _ => Vector3.up
            };
        }

        private Quaternion GetAxisRotation(int direction)
        {
            return direction switch
            {
                0 => Quaternion.Euler(0, 0, -90),
                2 => Quaternion.Euler(90, 0, 0),
                _ => Quaternion.identity
            };
        }

        public void ClearCache()
        {
            foreach (var mesh in _boxMeshCache.Values) Object.DestroyImmediate(mesh);
            foreach (var mesh in _sphereMeshCache.Values) Object.DestroyImmediate(mesh);
            foreach (var mesh in _capsuleMeshCache.Values) Object.DestroyImmediate(mesh);

            _boxMeshCache.Clear();
            _sphereMeshCache.Clear();
            _capsuleMeshCache.Clear();
        }
    }

    public enum DetailLevel
    {
        Low,
        Medium,
        High
    }
}
#endif