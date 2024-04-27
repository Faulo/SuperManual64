using Slothsoft.UnityExtensions;
using TMPro;
using UnityEngine;

namespace SuperManual64.Player {
    sealed class PrintAction : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI text;
        [SerializeField, Expandable]
        MarioState state;

        void Update() {
            text.text = state.actionName;
        }
    }
}
