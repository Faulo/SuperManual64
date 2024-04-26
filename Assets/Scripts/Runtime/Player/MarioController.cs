using Slothsoft.UnityExtensions;
using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioController : MonoBehaviour {
        [SerializeField, Expandable]
        MarioState state;

        void Start() {
            state.pos = Vector3.zero;
        }

        void FixedUpdate() {
            for (bool inLoop = true; inLoop;) {
                inLoop = (state.action & EAction.ACT_GROUP_MASK) switch {
                    EAction.ACT_GROUP_STATIONARY => HandleStationary(),
                    EAction.ACT_GROUP_MOVING => HandleMoving(),
                    _ => false,
                };
            }
        }

        bool HandleStationary() {
            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                state.faceAngle.y = state.intendedYaw;
                state.forwardVel = Mathf.Min(state.intendedMag, 8);
                state.action = EAction.ACT_WALKING;
                return true;
            }

            return false;
        }

        bool HandleMoving() {
            if (!state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                state.forwardVel = 0;
                state.action = EAction.ACT_IDLE;
                return true;
            }

            state.faceAngle.y = state.intendedYaw;

            float maxTargetSpeed = 32.0f;
            float targetSpeed = Mathf.Min(state.intendedMag, maxTargetSpeed);

            if (state.forwardVel <= 0.0f) {
                state.forwardVel += 1.1f;
            } else if (state.forwardVel <= targetSpeed) {
                state.forwardVel += 1.1f - (state.forwardVel / 43.0f);
            } else {
                state.forwardVel = state.forwardVel;
            }

            if (state.forwardVel > 48.0f) {
                state.forwardVel = 48.0f;
            }

            state.PerformGroundStep(Time.deltaTime);

            return false;
        }
    }
}
