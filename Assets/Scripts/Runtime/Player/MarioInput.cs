using Slothsoft.UnityExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperManual64.Player {
    sealed class MarioInput : MonoBehaviour {
        [SerializeField, Expandable]
        InputActionAsset input;
        [SerializeField, Expandable]
        MarioState state;
        [SerializeField, Expandable]
        Camera attachedCamera;

        InputActionMap map;

        void OnEnable() {
            map = input.FindActionMap("Player");
            map.Enable();
        }

        void OnDisable() {
            map.Disable();
        }

        public void Update() {
            state.UpdateIntentions(map, attachedCamera);
        }
    }
}
