using Slothsoft.UnityExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperManual64.Player {
    sealed class MarioInput : MonoBehaviour {
        [SerializeField, Expandable]
        MarioState state;
        [SerializeField, Expandable]
        Camera attachedCamera;

        public void OnMove(InputValue stick) {
            state.UpdateIntentions(stick.Get<Vector2>(), attachedCamera);
        }

        public void OnA(InputValue button) {
            Debug.Log($"A: {button.isPressed}");
        }

        public void OnB(InputValue button) {
            Debug.Log($"B: {button.isPressed}");
        }

        public void OnZ(InputValue button) {
            Debug.Log($"Z: {button.isPressed}");
        }
    }
}
