using TMPro;
using UnityEngine;

namespace SuperManual64 {
    [ExecuteAlways]
    sealed class PrintVersion : MonoBehaviour {
        [SerializeField]
        TextMeshProUGUI _textComponent;

        void Start() {
            UpdateText();
        }
#if UNITY_EDITOR
        void Update() {
            UpdateText();
        }
#endif
        void UpdateText() {
            if (_textComponent) {
                _textComponent.text = $"{Application.productName} v{Application.version}";
            }
        }
    }
}
