using System;
using System.Collections.Generic;
using SuperManual64.Player;
using UnityEditor;
using UnityEngine;

namespace SuperManual64.Editor {
    [CustomEditor(typeof(MarioState))]
    sealed class MarioStateEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var marioState = target as MarioState;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Runtime Values", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField("Flags", PrintFlag(marioState.flags));

            EditorGUILayout.LabelField("Current Action");
            EditorGUILayout.TextArea(PrintFlag(marioState.action), EditorStyles.textArea, GUILayout.Height(60));

            EditorGUILayout.LabelField("Previous Action");
            EditorGUILayout.TextArea(PrintFlag(marioState.prevAction), EditorStyles.textArea, GUILayout.Height(60));

            EditorGUI.EndDisabledGroup();
        }

        static string PrintFlag<T>(T value) where T : Enum {
            return string.Join(", ", GetFlags(value));
        }

        static IEnumerable<string> GetFlags<T>(T value) where T : Enum {
            var values = Enum.GetValues(typeof(T));
            string[] names = Enum.GetNames(typeof(T));
            for (int i = 0; i < values.Length; i++) {
                if (value.HasFlag((T)values.GetValue(i))) {
                    yield return names[i];
                }
            }
        }
    }
}
