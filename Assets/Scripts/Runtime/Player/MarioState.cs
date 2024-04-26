using System;
using UnityEngine;

namespace SuperManual64.Player {
    [CreateAssetMenu]
    sealed class MarioState : ScriptableObject {
        [SerializeField]
        public EInput input;
        [NonSerialized]
        public EAction action;

        [Space]
        [SerializeField]
        public Vector3Int faceAngle;
        [SerializeField]
        public Vector3Int angleVel;
        [SerializeField]
        public int slideYaw;
        [SerializeField]
        public int twirlYaw;
        [SerializeField]
        public Vector3 pos;
        [SerializeField]
        public Vector3 vel;
        [SerializeField]
        float m_forwardVel;
        public float forwardVel {
            get => m_forwardVel;
            set {
                m_forwardVel = value;

                slideVelX = Mathf.Sin(faceAngle.y * Mathf.Deg2Rad) * value;
                slideVelZ = Mathf.Cos(faceAngle.y * Mathf.Deg2Rad) * value;

                vel.x = slideVelX;
                vel.z = slideVelZ;
            }
        }
        [SerializeField]
        public float slideVelX;
        [SerializeField]
        public float slideVelZ;
        [SerializeField]
        public Surface wall;
        [SerializeField]
        public Surface ceil;
        [SerializeField]
        public Surface floor;

        [SerializeField]
        public float intendedMag;
        [SerializeField]
        public int intendedYaw;

        public void UpdateIntentions(Vector2 stick, Camera camera) {
            input = EInput.INPUT_NONE;

            intendedMag = 64 * stick.sqrMagnitude;

            if (stick.sqrMagnitude > 0) {
                intendedYaw = Mathf.RoundToInt((Mathf.Rad2Deg * Mathf.Atan2(-stick.y, stick.x)) + camera.transform.eulerAngles.y + 90);
                input |= EInput.INPUT_NONZERO_ANALOG;
            } else {
                intendedYaw = faceAngle.y;
            }
        }

        public void UpdateHitbox() {
            if (action.HasFlag(EAction.ACT_FLAG_SHORT_HITBOX)) {
                // 100
            } else {
                // 160
            }
        }

        public void PerformGroundStep(float deltaTime) {
            var intendedPos = Vector3.zero;

            for (int i = 0; i < 4; i++) {
                intendedPos[0] = pos[0] + (floor.normal.y * deltaTime * (vel[0] / 4.0f));
                intendedPos[2] = pos[2] + (floor.normal.y * deltaTime * (vel[2] / 4.0f));
                intendedPos[1] = pos[1];

                pos = intendedPos;
            }
        }
    }
}