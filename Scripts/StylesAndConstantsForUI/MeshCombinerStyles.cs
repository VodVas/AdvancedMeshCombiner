#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VodVas.AdvancedMeshCombiner
{
    public static class MeshCombinerStyles
    {
        public const float MIN_WIDTH = 450f;
        public const float MIN_HEIGHT = 600f;
        public const int MAX_VERTICES_PER_CHUNK = 65000;

        private static GUIStyle _titleStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _boldLabel;
        private static GUIStyle _resultStyle;
        private static GUIStyle _warningStyle;

        public static readonly Color SuccessColor = Color.green;
        public static readonly Color WarningColor = Color.yellow;
        public static readonly Color ErrorColor = Color.red;

        public static GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle()
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 22,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 60,
                        stretchWidth = true,
                        normal =
                    {
                        textColor = new Color(1f, 0.5f, 0.2f),
                        background = CreateColoredTex(new Color(0f, 0f, 0f))
                    },
                        richText = true
                    };
                }
                return _titleStyle;
            }
        }

        private static Texture2D CreateColoredTex(Color col)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        public static GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(4, 4, 6, 6)
                    };
                }
                return _headerStyle;
            }
        }

        public static GUIStyle BoldLabel
        {
            get
            {
                if (_boldLabel == null)
                {
                    _boldLabel = new GUIStyle(EditorStyles.boldLabel)
                    {
                        richText = true
                    };
                }
                return _boldLabel;
            }
        }

        public static GUIStyle ResultStyle
        {
            get
            {
                if (_resultStyle == null)
                {
                    _resultStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        richText = true,
                        wordWrap = true,
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }
                return _resultStyle;
            }
        }

        public static GUIStyle WarningStyle
        {
            get
            {
                if (_warningStyle == null)
                {
                    _warningStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        richText = true,
                        wordWrap = true,
                        padding = new RectOffset(10, 10, 10, 10),
                        normal = { textColor = Color.yellow }
                    };
                }
                return _warningStyle;
            }
        }

    }
}
#endif