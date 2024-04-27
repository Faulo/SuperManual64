using System;
using UnityEngine;

namespace SuperManual64.Player {
    [CreateAssetMenu]
    sealed class MarioState : ScriptableObject {
        [Header("Input")]
        [SerializeField]
        public EInput input;
        [SerializeField, Range(0, 64)]
        public float intendedMag;
        [SerializeField]
        public int intendedYaw;

        public void UpdateIntentions(Vector2 stick, Camera camera) {
            input = EInput.INPUT_NONE;

            intendedMag = Mathf.Clamp(64 * stick.sqrMagnitude, 0, 64);

            if (stick.sqrMagnitude > 0) {
                intendedYaw = Mathf.RoundToInt((Mathf.Rad2Deg * Mathf.Atan2(-stick.y, stick.x)) + camera.transform.eulerAngles.y + 90);
                input |= EInput.INPUT_NONZERO_ANALOG;
            } else {
                intendedYaw = faceAngle.y;
                input |= EInput.INPUT_UNKNOWN_5;
            }
        }

        public bool analogStickHeldBack {
            get {
                int intendedDYaw = intendedYaw - faceAngle[1];
                return intendedDYaw is < (-0x471C) or > 0x471C;
            }
        }

        [Header("Actions")]
        [SerializeField]
        public int actionArg;
        [SerializeField]
        public int actionState;
        [SerializeField]
        public int actionTimer;
        [NonSerialized]
        public EAction action;
        [NonSerialized]
        public EAction prevAction;
        [NonSerialized]
        public EFlags flags;

        [Header("Physics")]
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
        public int floorAngle;

        public bool isFacingDownhill {
            get {
                int faceAngleYaw = faceAngle[1];

                faceAngleYaw = floorAngle - faceAngleYaw;

                return faceAngleYaw is > (-0x4000) and < 0x4000;
            }
        }
        public bool shouldBeginSliding {
            get {
                return false;
            }
        }

        [Header("Misc")]
        [SerializeField]
        public int health = 0x100;

        public void UpdateHitbox() {
            if (action.HasFlag(EAction.ACT_FLAG_SHORT_HITBOX)) {
                // 100
            } else {
                // 160
            }
        }

        [SerializeField]
        float unitMultiplier = 0.01f;

        public EGroundStep PerformGroundStep() {
            var intendedPos = Vector3.zero;

            for (int i = 0; i < 4; i++) {
                intendedPos[0] = pos[0] + (floor.normal.y * unitMultiplier * (vel[0] / 4.0f));
                intendedPos[2] = pos[2] + (floor.normal.y * unitMultiplier * (vel[2] / 4.0f));
                intendedPos[1] = pos[1];

                pos = intendedPos;
            }

            return EGroundStep.GROUND_STEP_NONE;
        }

        public void Spawn(Vector3 position) {
            pos = position;
            vel = Vector3.zero;
        }
    }
}