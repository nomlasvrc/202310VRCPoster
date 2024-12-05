
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Nomlas.Poster
{
    [CustomEditor(typeof(Poster))]
    public class PosterEditor : Editor
    {
        bool openDefault;
        public override void OnInspectorGUI()
        {
            Poster poster = target as Poster;

            EditorGUILayout.LabelField("2023年10月VRC同期会ポスター " + poster.version, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            if (poster.JapaneseMode)
            {
                if (GUILayout.Button("Switch to English"))
                {
                    poster.JapaneseMode = false;
                }
            }
            else
            {
                if (GUILayout.Button("日本語に切り替え"))
                {
                    poster.JapaneseMode = true;
                }
            }
            EditorGUILayout.Space();

            poster.slideTime = EditorGUILayout.IntField(JPENText(poster.JapaneseMode, "スライドショーのインターバル", "Interval"), poster.slideTime);
            if (poster.slideTime <= 0)
            {
                EditorGUILayout.HelpBox("インターバルが0秒以下になっています！", MessageType.Error);
            }
            EditorGUILayout.Space();

            poster.startDelayTime = EditorGUILayout.IntField(JPENText(poster.JapaneseMode, "開始遅延", "Start delay time"), poster.startDelayTime);
            if (poster.startDelayTime < 0)
            {
                EditorGUILayout.HelpBox("開始遅延が0秒未満になっています！", MessageType.Error);
            }
            EditorGUILayout.Space();

            poster.aspectRaito = EditorGUILayout.FloatField(JPENText(poster.JapaneseMode, "アスペクト比", "Aspect Ratio"), poster.aspectRaito);
            if (poster.aspectRaito <= 0)
            {
                EditorGUILayout.HelpBox("アスペクト比が0以下になっています！", MessageType.Error);
            }
            if (GUILayout.Button(JPENText(poster.JapaneseMode, "アスペクト比をリセット", "Reset")))
            {
                poster.aspectRaito = 0.7071f;
            }
            if (poster.picture != null)
            {
                Material material = poster.picture.GetComponent<MeshRenderer>().sharedMaterial;
                material.SetFloat("_Aspect", poster.aspectRaito);
            }
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            openDefault = EditorGUILayout.BeginFoldoutHeaderGroup(openDefault, JPENText(poster.JapaneseMode, "値", "Values"));
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (openDefault)
            {
                EditorGUI.indentLevel++;
                DrawDefaultInspector();
                EditorGUI.indentLevel--;
            }
        }

        private string JPENText(bool japaneseMode, string japaneseText, string englishText)
        {
            return japaneseMode ? japaneseText : englishText;
        }
    }
}