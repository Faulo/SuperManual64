using Slothsoft.UnityExtensions;
using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioVisualizer : MonoBehaviour {
        [SerializeField, Expandable]
        MarioState state;

        void Update() {
            transform.SetPositionAndRotation(state.pos, Quaternion.Euler(state.twirlYaw, state.slideYaw, 0));
        }
    }
}
