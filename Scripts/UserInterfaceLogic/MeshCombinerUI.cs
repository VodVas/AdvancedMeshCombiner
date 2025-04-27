#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text;

namespace VodVas.AdvancedMeshCombiner
{
    public class MeshCombinerUI
    {
        private Vector2 _scrollPosition;
        private bool _showAdvancedOptions;
        private bool _showColliderOptions;
        private MeshCombineSettings _settings;
        private string _resultMessage = "";

        public MeshCombinerUI(MeshCombineSettings initialSettings)
        {
            _settings = initialSettings;
        }

        public void DrawUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(MeshCombinerStyles.MIN_HEIGHT));

            DrawTitle();
            DrawMainControls();
            DrawOptions();
            DrawAdvancedOptions();
            DrawColliderOptions();
            DrawActionButton();
            DrawResult();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle()
        {
            Rect titleRect = EditorGUILayout.GetControlRect(
                GUILayout.Height(MeshCombinerStyles.TitleStyle.fixedHeight),
                GUILayout.ExpandWidth(true)
            );

            GUI.Label(
                new Rect(titleRect.x - 4, titleRect.y, titleRect.width + 8, titleRect.height),
                "ADVANCED MESH COMBINER",
                MeshCombinerStyles.TitleStyle
            );

            GUILayout.Space(10);
        }

        private void DrawMainControls()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _settings.ParentObject = (Transform)EditorGUILayout.ObjectField(
                "Parent Object:",
                _settings.ParentObject,
                typeof(Transform),
                true
            );
            EditorGUILayout.EndVertical();
        }

        private void DrawOptions()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Options:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            _settings.RemoveOriginals = EditorGUILayout.ToggleLeft(
                new GUIContent(" Remove Original Objects", "Delete original meshes after combining"),
                _settings.RemoveOriginals
            );

            _settings.CreateAsChild = EditorGUILayout.ToggleLeft(
                new GUIContent(" Create As Child Object", "Place combined mesh under original parent"),
                _settings.CreateAsChild
            );

            _settings.GenerateColliders = EditorGUILayout.ToggleLeft(
                new GUIContent(" Generate Colliders", "Create colliders for combined meshes"),
                _settings.GenerateColliders
            );

            if (_settings.GenerateColliders)
            {
                EditorGUI.indentLevel++;
                _settings.CreateSharedCollider = EditorGUILayout.ToggleLeft(
                    new GUIContent(" Create Shared Collider", "Use single collider for all chunks (improves physics performance)"),
                    _settings.CreateSharedCollider
                );
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedOptions()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _showAdvancedOptions = EditorGUILayout.Foldout(_showAdvancedOptions, "Advanced Options");

            if (_showAdvancedOptions)
            {
                EditorGUI.indentLevel++;

                _settings.ChunkMode = (ChunkMode)EditorGUILayout.EnumPopup(
                    new GUIContent("Chunk Mode", "Controls how meshes are split into chunks"),
                    _settings.ChunkMode
                );

                if (_settings.ChunkMode != ChunkMode.NoSplit)
                {
                    _settings.MaxVerticesPerChunk = EditorGUILayout.IntSlider(
                        new GUIContent("Max Vertices Per Chunk", "Maximum vertices in a single chunk (65k limit for WebGL)"),
                        _settings.MaxVerticesPerChunk, 10000, MeshCombinerStyles.MAX_VERTICES_PER_CHUNK
                    );
                }

                if (_settings.ChunkMode != ChunkMode.ForceSplit)
                {
                    _settings.CombineByMaterial = EditorGUILayout.ToggleLeft(
                        new GUIContent(" Combine By Material", "Group meshes by material to reduce draw calls"),
                        _settings.CombineByMaterial
                    );
                }
                else
                {
                    _settings.CombineByMaterial = true;
                }

                if (_settings.ChunkMode == ChunkMode.NoSplit)
                {
                    EditorGUILayout.HelpBox(
                        "NoSplit mode ignores vertex limits and may not work in WebGL 1.0. Only use for WebGL 2.0 or standalone builds.",
                        MessageType.Warning
                    );
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawColliderOptions()
        {
            if (!_settings.GenerateColliders)
                return;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            _showColliderOptions = EditorGUILayout.Foldout(_showColliderOptions, "Collider Options");

            if (_showColliderOptions)
            {
                EditorGUI.indentLevel++;

                _settings.PreserveIndividualColliders = EditorGUILayout.ToggleLeft(
                    new GUIContent(" Preserve Individual Colliders", "Keep original collider structure (better physical interaction)"),
                    _settings.PreserveIndividualColliders
                );

                EditorGUI.BeginDisabledGroup(_settings.PreserveIndividualColliders);

                _settings.CreateSharedCollider = EditorGUILayout.ToggleLeft(
                    new GUIContent(" Create Shared Collider", "Use single collider for all chunks (improves physics performance)"),
                    _settings.CreateSharedCollider
                );

                _settings.ProcessPrimitiveColliders = EditorGUILayout.ToggleLeft(
                    new GUIContent(" Process Primitive Colliders", "Convert Box, Sphere, and Capsule colliders to meshes"),
                    _settings.ProcessPrimitiveColliders
                );

                if (_settings.ProcessPrimitiveColliders)
                {
                    EditorGUI.indentLevel++;

                    _settings.ColliderGroupStrategy = (ColliderGroupingStrategy)EditorGUILayout.EnumPopup(
                        new GUIContent("Grouping Strategy", "How to group different types of colliders"),
                        _settings.ColliderGroupStrategy
                    );

                    _settings.PrimitiveColliderDetail = (DetailLevel)EditorGUILayout.EnumPopup(
                        new GUIContent("Detail Level", "Detail level for primitive collider meshes"),
                        _settings.PrimitiveColliderDetail
                    );

                    if (_settings.PrimitiveColliderDetail == DetailLevel.High)
                    {
                        EditorGUILayout.HelpBox(
                            "High detail level may generate large meshes, which could impact performance on mobile devices.",
                            MessageType.Info
                        );
                    }

                    EditorGUI.indentLevel--;
                }

                if (_settings.PreserveIndividualColliders)
                {
                    EditorGUILayout.HelpBox(
                        "Individual colliders preserve exact physics but may impact performance. For best WebGL performance, use shared colliders instead.",
                        MessageType.Info
                    );
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButton()
        {
            EditorGUILayout.Space(15);
            EditorGUI.BeginDisabledGroup(_settings.ParentObject == null);
            if (GUILayout.Button("Combine Children", GUILayout.Height(32)))
            {
                OnCombineClicked?.Invoke(_settings);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawResult()
        {
            if (!string.IsNullOrEmpty(_resultMessage))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox(_resultMessage, MessageType.Info);
            }
        }

        public void SetResultMessage(string message)
        {
            _resultMessage = message;
        }

        public string FormatCombineResultString(ChunkStatistics stats, MeshCombineSettings settings)
        {
            StringBuilder sb = new StringBuilder(512);

            sb.AppendLine($"<b>Combined Result:</b> {stats.ResultName}");
            sb.AppendLine($"<b>Combine Mode:</b> {settings.ChunkMode}");
            sb.AppendLine($"<b>Total Chunks:</b> {stats.TotalChunks}");
            sb.AppendLine($"<b>Total Vertices:</b> {stats.TotalVertices:N0}");
            sb.AppendLine($"<b>Total Triangles:</b> {stats.TotalTriangles:N0}");

            if (settings.GenerateColliders)
            {
                if (stats.IndividualCollidersPreserved)
                {
                    sb.AppendLine($"<b>Colliders:</b> {stats.PreservedColliderCount} individual colliders preserved");
                }
                else
                {
                    sb.AppendLine($"<b>Collider Vertices:</b> {stats.TotalColliderVertices:N0}");

                    if (settings.ProcessPrimitiveColliders)
                    {
                        sb.AppendLine($"<b>Collider Strategy:</b> {settings.ColliderGroupStrategy}");
                        sb.AppendLine($"<b>Primitive Detail:</b> {settings.PrimitiveColliderDetail}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("<b>Chunk Statistics:</b>");
            sb.AppendLine($"· Max Vertices: {stats.MaxChunkVertices:N0}");
            sb.AppendLine($"· Min Vertices: {stats.MinChunkVertices:N0}");
            sb.AppendLine($"· Avg Vertices: {stats.AvgChunkVertices:N0}");
            sb.AppendLine($"· Draw Calls: ~{stats.TotalDrawCalls}");
            sb.AppendLine();
            sb.AppendLine($"<b>Material Groups ({stats.MaterialGroups.Count}):</b>");

            foreach (string material in stats.MaterialGroups)
            {
                sb.AppendLine($"· {material}");
            }

            if (settings.ChunkMode != ChunkMode.NoSplit && stats.MaxChunkVertices > 65535)
            {
                sb.AppendLine();
                sb.AppendLine("<color=red><b>WARNING:</b> Some chunks exceed the 65k vertex limit for WebGL 1.0!</color>");
                sb.AppendLine("Consider using NoSplit mode only for WebGL 2.0 targets.");
            }
            else if (settings.ChunkMode == ChunkMode.NoSplit && stats.TotalVertices > 65535)
            {
                sb.AppendLine();
                sb.AppendLine("<color=orange><b>NOTE:</b> Using NoSplit mode with >65k vertices. Ensure your target platform supports large meshes (WebGL 2.0).</color>");
            }

            return sb.ToString();
        }

        public delegate void CombineClickedHandler(MeshCombineSettings settings);
        public event CombineClickedHandler OnCombineClicked;
    }
}
#endif