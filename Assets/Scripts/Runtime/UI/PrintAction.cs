using Slothsoft.UnityExtensions;
using SuperManual64.Player;
using TMPro;
using UnityEngine;

namespace SuperManual64.UI {
    [ExecuteAlways]
    sealed class PrintAction : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI textComponent;
        [SerializeField, Expandable]
        MarioState state;

        void Update() {
            if (textComponent && state) {
                textComponent.text = state.actionName;
            }
        }
    }
}
