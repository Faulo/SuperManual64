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

            var state = target as MarioState;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Runtime Values", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.FloatField("Delta Yaw", state.deltaYaw);
            EditorGUILayout.Toggle("Analog Stick Held Back", state.analogStickHeldBack);
            EditorGUILayout.Toggle("Is Facing Downhill", state.isFacingDownhill);

            EditorGUILayout.TextField("Flags", PrintFlag(state.flags));

            EditorGUILayout.LabelField("Current Action");
            EditorGUILayout.TextArea(PrintFlag(state.action), EditorStyles.textArea, GUILayout.Height(128));

            EditorGUILayout.LabelField("Previous Action");
            EditorGUILayout.TextArea(PrintFlag(state.prevAction), EditorStyles.textArea, GUILayout.Height(128));

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
