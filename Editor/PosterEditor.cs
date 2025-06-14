using UnityEditor;
using UnityEngine;

namespace Nomlas.Poster
{
    [CustomEditor(typeof(Poster))]
    public class PosterEditor : Editor
    {
        private bool openDefault;
        public override void OnInspectorGUI()
        {
            Poster poster = target as Poster;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("2023年10月VRC同期会ポスター " + PosterVersion.GetVersion(), EditorStyles.boldLabel);
            poster.language = (Language)EditorGUILayout.EnumPopup(JPENText(poster.language, "言語", "Language"), poster.language);
            EditorGUILayout.Space();

            poster.slideTime = EditorGUILayout.IntField(JPENText(poster.language, "スライドショーのインターバル", "Interval"), poster.slideTime);
            if (poster.slideTime <= 0)
            {
                EditorGUILayout.HelpBox("インターバルが0秒以下になっています！", MessageType.Error);
            }
            EditorGUILayout.Space();

            poster.startDelayTime = EditorGUILayout.IntField(JPENText(poster.language, "開始遅延", "Start delay time"), poster.startDelayTime);
            if (poster.startDelayTime < 0)
            {
                EditorGUILayout.HelpBox("開始遅延が0秒未満になっています！", MessageType.Error);
            }
            EditorGUILayout.Space();

            poster.aspectRaito = EditorGUILayout.FloatField(JPENText(poster.language, "アスペクト比", "Aspect Ratio"), poster.aspectRaito);
            if (poster.aspectRaito <= 0)
            {
                EditorGUILayout.HelpBox("アスペクト比が0以下になっています！", MessageType.Error);
            }
            if (GUILayout.Button(JPENText(poster.language, "アスペクト比をリセット", "Reset")))
            {
                poster.aspectRaito = 0.7071f;
            }
            if (poster.picture != null)
            {
                Material material = poster.picture.GetComponent<MeshRenderer>().sharedMaterial;
                material.SetFloat("_Aspect", poster.aspectRaito);
            }
            EditorGUILayout.Space();

            if (poster.picture == null)
            {
                EditorGUILayout.HelpBox("ポスターを表示するGameObjectが設定されていません！", MessageType.Error);
            }
            else
            {
                if (!poster.picture.GetComponent<MeshRenderer>())
                {
                    EditorGUILayout.HelpBox("ポスターを表示するGameObjectにMesh Rendererがありません！", MessageType.Error);
                }
            }
            if (poster.animator == null)
            {
                EditorGUILayout.HelpBox("アニメータが設定されていません！", MessageType.Error);
            }

            EditorGUILayout.Space();
            openDefault = EditorGUILayout.BeginFoldoutHeaderGroup(openDefault, JPENText(poster.language, "値", "Values"));
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (openDefault)
            {
                EditorGUI.indentLevel++;
                DrawDefaultInspector();
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(poster, "Poster Value Change");
                EditorUtility.SetDirty(poster);
            }
        }

        private string JPENText(Language japaneseMode, string japaneseText, string englishText)
        {
            return japaneseMode == Language.日本語 ? japaneseText : englishText;
        }
    }
}