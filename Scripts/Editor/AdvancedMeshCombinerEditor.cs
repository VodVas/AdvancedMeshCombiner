#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace VodVas.AdvancedMeshCombiner
{
    public sealed class AdvancedMeshCombinerEditor : EditorWindow
    {
        private MeshCombinerUI _ui;
        private MeshCombineProcessor _processor;
        private IMeshChunkBuilder _chunkBuilder;
        private IColliderGenerator _colliderGenerator;
        private IMeshTransformer _meshTransformer;
        private IOriginalObjectProcessor _originalProcessor;
        private IUndoService _undoService;
        private MeshCombineSettings _settings;

        [MenuItem("Tools/VodVas/Advanced Mesh Combiner")]
        private static void Init()
        {
            GetWindow<AdvancedMeshCombinerEditor>("Mesh Combiner").minSize =
                new Vector2(MeshCombinerStyles.MIN_WIDTH, MeshCombinerStyles.MIN_HEIGHT);
        }

        private void OnEnable()
        {
            InitializeServices();
            InitializeSettings();
            InitializeUI();
        }

        private void InitializeServices()
        {
            _meshTransformer = new MeshTransformer();
            _undoService = new EditorUndoService();
            _chunkBuilder = new MeshChunkBuilder();
            _colliderGenerator = new ColliderGenerator(_meshTransformer);
            _originalProcessor = new OriginalObjectProcessor(_undoService);

            _processor = new MeshCombineProcessor(
                _chunkBuilder,
                _colliderGenerator,
                _meshTransformer,
                _originalProcessor,
                _undoService
            );
        }

        private void InitializeSettings()
        {
            _settings = new MeshCombineSettings(
                null,
                removeOriginals: true,
                createAsChild: true,
                generateColliders: true,
                createSharedCollider: true,
                combineByMaterial: true,
                chunkMode: ChunkMode.Auto,
                maxVerticesPerChunk: MeshCombinerStyles.MAX_VERTICES_PER_CHUNK
            );
        }

        private void InitializeUI()
        {
            _ui = new MeshCombinerUI(_settings);
            _ui.OnCombineClicked += ExecuteCombine;
        }

        private void OnGUI()
        {
            if (_ui == null)
            {
                InitializeUI();
            }

            _ui.DrawUI();
        }

        private void ExecuteCombine(MeshCombineSettings settings)
        {
            if (!ValidateSettings(settings))
                return;

            _settings = settings;

            var sw = Stopwatch.StartNew();
            MeshCombineResult result = _processor.CombineMeshes(settings);
            sw.Stop();

            if (result != null)
            {
                string resultMessage = _ui.FormatCombineResultString(result.Statistics, settings);
                _ui.SetResultMessage(resultMessage);
                Debug.Log($"[Mesh Combiner] Operation completed in {sw.ElapsedMilliseconds}ms\n{resultMessage}");
            }
            else
            {
                _ui.SetResultMessage("Error: Failed to combine meshes. See console for details.");
            }

            Repaint();
        }

        private bool ValidateSettings(MeshCombineSettings settings)
        {
            if (settings.ParentObject == null)
            {
                _ui.SetResultMessage("Error: Parent object not assigned!");
                return false;
            }

            if (settings.ParentObject.childCount == 0)
            {
                _ui.SetResultMessage("Error: Parent has no children!");
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            if (_ui != null)
            {
                _ui.OnCombineClicked -= ExecuteCombine;
            }
        }
    }
}
#endif