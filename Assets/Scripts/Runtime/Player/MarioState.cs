using System;
using Slothsoft.UnityExtensions;
using SuperManual64.Level;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperManual64.Player {
    [CreateAssetMenu]
    sealed class MarioState : ScriptableObject {
        [Header("Input")]
        [SerializeField]
        public EInput input;
        [SerializeField]
        public int framesSinceA;
        [SerializeField]
        public int framesSinceB;
        [SerializeField, Range(0, 32)]
        public float intendedMag;
        [SerializeField]
        public float intendedYaw;

        InputActionMap actions;
        Camera camera;

        public void SetIntentions(InputActionMap actions, Camera camera) {
            this.actions = actions;
            this.camera = camera;
        }

        public void UpdateIntentions() {
            //particleFlags = 0;
            input = EInput.INPUT_NONE;
            flags &= (EFlags)0xFFFFFF;

            var stick = actions["Move"].ReadValue<Vector2>();

            float maximumMag = squishTimer == 0
                ? 32
                : 8;
            intendedMag = maximumMag * stick.sqrMagnitude;

            if (stick.sqrMagnitude > 0) {
                intendedYaw = (Mathf.Atan2(-stick.y, stick.x) * Mathf.Rad2Deg) + camera.transform.eulerAngles.y + 90;
                input |= EInput.INPUT_NONZERO_ANALOG;
            } else {
                intendedYaw = faceAngleYaw;
            }

            if (actions["A"].WasPressedThisFrame()) {
                input |= EInput.INPUT_A_PRESSED;
            }

            if (actions["A"].IsPressed()) {
                input |= EInput.INPUT_A_DOWN;
            }

            // Don't update for these buttons if squished.
            if (squishTimer == 0) {
                if (actions["B"].WasPressedThisFrame()) {
                    //input |= EInput.INPUT_B_PRESSED;
                }

                if (actions["Z"].IsPressed()) {
                    //input |= EInput.INPUT_Z_DOWN;
                }

                if (actions["Z"].WasPressedThisFrame()) {
                    //input |= EInput.INPUT_Z_PRESSED;
                }
            }

            if (!input.HasFlag(EInput.INPUT_NONZERO_ANALOG) && !input.HasFlag(EInput.INPUT_A_PRESSED)) {
                input |= EInput.INPUT_NEITHER_STICK_NOR_A;
            }

            if (input.HasFlag(EInput.INPUT_A_PRESSED)) {
                framesSinceA = 0;
            } else if (framesSinceA < 0xFF) {
                framesSinceA++;
            }

            if (input.HasFlag(EInput.INPUT_B_PRESSED)) {
                framesSinceB = 0;
            } else if (framesSinceB < 0xFF) {
                framesSinceB++;
            }

            if (wallKickTimer > 0) {
                wallKickTimer--;
            }

            if (doubleJumpTimer > 0) {
                doubleJumpTimer--;
            }

            if (surfaces.TryFindFloor(pos, out var newFloor)) {
                floor = newFloor;
            }

            if (pos[1] > floorHeight + (100.0f * unitMultiplier)) {
                // causes wild bonks
                // input |= EInput.INPUT_OFF_FLOOR;
            }
        }

        public float deltaYaw => Mathf.DeltaAngle(intendedYaw, faceAngleYaw);

        public bool analogStickHeldBack {
            get {
                return deltaYaw is < -100 or > 100;
            }
        }

        [Header("Actions")]
        [SerializeField]
        public string actionName;
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
        [SerializeField, Expandable]
        SurfaceManager surfaces;
        [SerializeField]
        public Vector3 faceAngle;
        public float faceAngleYaw {
            get => faceAngle.y;
            set => faceAngle.y = value;
        }
        [SerializeField]
        public Vector3Int angleVel;
        [SerializeField]
        public float slideYaw;
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

                slideVelX = Mathf.Sin(faceAngleYaw * Mathf.Deg2Rad) * value;
                slideVelZ = Mathf.Cos(faceAngleYaw * Mathf.Deg2Rad) * value;

                vel.x = slideVelX;
                vel.z = slideVelZ;
            }
        }
        [SerializeField]
        public float slideVelX;
        [SerializeField]
        public float slideVelZ;
        [SerializeField]
        public SurfacePoint wall;
        [SerializeField]
        public SurfacePoint ceil;
        public float ceilHeight => ceil is null
            ? floorHeight + 1000
            : ceil.position.y;
        [SerializeField]
        public SurfacePoint floor;
        [SerializeField]
        public float floorAngle;

        public float floorHeight => floor.height;

        [SerializeField]
        public SurfacePoint ledge;

        public float ledgeHeight => ledge.height;

        public bool isFacingDownhill {
            get {
                return Mathf.DeltaAngle(floorAngle, faceAngleYaw) is > -90 and < 90;
            }
        }
        public bool shouldBeginSliding {
            get {
                return false;
            }
        }

        public bool shouldGetStuckInGround => false;

        [Header("Config")]
        [SerializeField]
        public bool hasSpecialTripleJump = false;
        [SerializeField]
        public float gettingBlownGravity = 0;
        [SerializeField]
        public float unitMultiplier = 0.01f;

        public void UpdateHitbox() {
            if (action.HasFlag(EAction.ACT_FLAG_SHORT_HITBOX)) {
                // 100
            } else {
                // 160
            }
        }

        public EGroundStep StationaryGroundStep() {
            forwardVel = 0;
            pos.y = floorHeight;
            return EGroundStep.GROUND_STEP_NONE;
        }

        public EGroundStep PerformGroundStep() {
            var intendedPos = Vector3.zero;

            for (int i = 0; i < 4; i++) {
                intendedPos[0] = pos[0] + (floor.normal.y * unitMultiplier * (vel[0] / 4.0f));
                intendedPos[2] = pos[2] + (floor.normal.y * unitMultiplier * (vel[2] / 4.0f));
                intendedPos[1] = pos[1];

                if (perform_ground_quarter_step(intendedPos) is EGroundStep.GROUND_STEP_LEFT_GROUND or EGroundStep.GROUND_STEP_HIT_WALL_STOP_QSTEPS) {
                    break;
                }
            }

            return EGroundStep.GROUND_STEP_NONE;
        }
        EGroundStep perform_ground_quarter_step(Vector3 nextPos) {
            _ = surfaces.ResolveWallCollisions(ref nextPos, 30.0f * unitMultiplier, 24.0f * unitMultiplier);
            wall = surfaces.ResolveWallCollisions(ref nextPos, 60.0f * unitMultiplier, 50.0f * unitMultiplier);

            if (!surfaces.TryFindFloor(nextPos, out var floor)) {
                return EGroundStep.GROUND_STEP_HIT_WALL_STOP_QSTEPS;
            }

            bool hasCeiling = surfaces.TryFindCeiling(nextPos.WithY(floor.height), out var ceil);

            if (nextPos.y > floor.height + (100.0f * unitMultiplier)) {
                if (hasCeiling && (nextPos[1] + (160.0f * unitMultiplier) >= ceil.height)) {
                    return EGroundStep.GROUND_STEP_HIT_WALL_STOP_QSTEPS;
                }

                pos = nextPos;
                this.floor = floor;
                return EGroundStep.GROUND_STEP_LEFT_GROUND;
            }

            if (hasCeiling && (floor.height + (160.0f * unitMultiplier) >= ceil.height)) {
                return EGroundStep.GROUND_STEP_HIT_WALL_STOP_QSTEPS;
            }

            pos = nextPos.WithY(floor.height);

            this.floor = floor;

            if (wall is not null) {
                /*
                s16 wallDYaw = atan2s(upperWall->normal.z, upperWall->normal.x) - faceAngle[1];

                if (wallDYaw >= 0x2AAA && wallDYaw <= 0x5555) {
                    return EGroundStep.GROUND_STEP_NONE;
                }
                if (wallDYaw <= -0x2AAA && wallDYaw >= -0x5555) {
                    return EGroundStep.GROUND_STEP_NONE;
                }

                //*/
                return EGroundStep.GROUND_STEP_HIT_WALL_CONTINUE_QSTEPS;
            }

            return EGroundStep.GROUND_STEP_NONE;
        }

        public EAirStep PerformAirStep(EAirStep stepArg) {
            var intendedPos = Vector3.zero;
            EAirStep quarterStepResult;
            var stepResult = EAirStep.AIR_STEP_NONE;

            wall = null;

            for (int i = 0; i < 4; i++) {
                intendedPos[0] = pos[0] + (unitMultiplier * vel[0] / 4.0f);
                intendedPos[1] = pos[1] + (unitMultiplier * vel[1] / 4.0f);
                intendedPos[2] = pos[2] + (unitMultiplier * vel[2] / 4.0f);

                quarterStepResult = perform_air_quarter_step(intendedPos, stepArg);

                //! On one qf, hit OOB/ceil/wall to store the 2 return value, and continue
                // getting 0s until your last qf. Graze a wall on your last qf, and it will
                // return the stored 2 with a sharply angled reference wall. (some gwks)

                if (quarterStepResult != EAirStep.AIR_STEP_NONE) {
                    stepResult = quarterStepResult;
                }

                if (quarterStepResult is EAirStep.AIR_STEP_LANDED
                    or EAirStep.AIR_STEP_GRABBED_LEDGE
                    or EAirStep.AIR_STEP_GRABBED_CEILING
                    or EAirStep.AIR_STEP_HIT_LAVA_WALL) {
                    break;
                }
            }

            if (vel[1] >= 0.0f) {
                peakHeight = pos[1];
            }

            if (action != EAction.ACT_FLYING) {
                ApplyGravity();
            }

            return stepResult;
        }

        EAirStep perform_air_quarter_step(Vector3 nextPos, EAirStep stepArg) {
            var upperWall = surfaces.ResolveWallCollisions(ref nextPos, 150.0f * unitMultiplier, 50.0f * unitMultiplier);
            var lowerWall = surfaces.ResolveWallCollisions(ref nextPos, 30.0f * unitMultiplier, 50.0f * unitMultiplier);

            if (!surfaces.TryFindFloor(nextPos, out var newFloor)) {
                // we hit OOB
                pos.y = nextPos.y;
                return EAirStep.AIR_STEP_HIT_WALL;
            }

            floor = newFloor;
            pos = nextPos;
            if (pos.y < floorHeight) {
                pos.y = floorHeight;
                return EAirStep.AIR_STEP_LANDED;
            }

            //! When the wall is not completely vertical or there is a slight wall
            // misalignment, you can activate these conditions in unexpected situations
            if (stepArg.HasFlag(EAirStep.AIR_STEP_CHECK_LEDGE_GRAB) && upperWall is null && lowerWall is not null) {
                if (check_ledge_grab(lowerWall, nextPos)) {
                    return EAirStep.AIR_STEP_GRABBED_LEDGE;
                }
            }

            if (upperWall is not null || lowerWall is not null) {
                wall = upperWall ?? lowerWall;

                if (wall.type == ESurface.SURFACE_BURNING) {
                    return EAirStep.AIR_STEP_HIT_LAVA_WALL;
                }

                float wallDYaw = Mathf.DeltaAngle(Mathf.Atan2(wall.normal.z, wall.normal.x) * Mathf.Rad2Deg, faceAngleYaw);

                if (wallDYaw is < -135 or > 135) {
                    flags |= EFlags.MARIO_UNKNOWN_30;
                    return EAirStep.AIR_STEP_HIT_WALL;
                }
            }

            _ = stepArg;

            return EAirStep.AIR_STEP_NONE;
        }

        bool check_ledge_grab(SurfacePoint lowerWall, Vector3 nextPos) {
            if (vel[1] > 0) {
                return false;
            }

            var ledgePos = nextPos;
            ledgePos.x -= lowerWall.normal.x * 60.0f * unitMultiplier;
            ledgePos.y += 160.0f * unitMultiplier;
            ledgePos.z -= lowerWall.normal.z * 60.0f * unitMultiplier;
            if (!surfaces.TryFindFloor(ledgePos, out var ledgeFloor)) {
                return false;
            }

            ledgePos.y = ledgeFloor.height;
            floor = ledgeFloor;
            ledge = ledgeFloor;

            floorAngle = Mathf.Atan2(ledgeFloor.normal.z, ledgeFloor.normal.x) * Mathf.Rad2Deg;
            faceAngleYaw = (Mathf.Atan2(lowerWall.normal.z, lowerWall.normal.x) * Mathf.Rad2Deg) + 180;

            return true;
        }

        [Header("Runtime Counters")]
        [SerializeField]
        public int health = 0x100;
        [SerializeField]
        public int hurtCounter;
        [SerializeField]
        public int doubleJumpTimer;
        [SerializeField]
        public int wallKickTimer;
        [SerializeField]
        public int squishTimer;
        [SerializeField]
        public float peakHeight;

        void ApplyGravity() {
            switch (action) {
                case EAction.ACT_TWIRLING when vel[1] < 0.0f:
                    float terminalVelocity;
                    float heaviness = 1.0f;

                    if (angleVel[1] > 1024) {
                        heaviness = 1024.0f / angleVel[1];
                    }

                    terminalVelocity = -75.0f * heaviness;

                    vel[1] -= 4.0f * heaviness;
                    if (vel[1] < terminalVelocity) {
                        vel[1] = terminalVelocity;
                    }

                    break;
                case EAction.ACT_SHOT_FROM_CANNON:
                    vel[1] -= 1.0f;
                    if (vel[1] < -75.0f) {
                        vel[1] = -75.0f;
                    }

                    break;
                case EAction.ACT_LONG_JUMP:
                case EAction.ACT_SLIDE_KICK:
                case EAction.ACT_BBH_ENTER_SPIN:
                    vel[1] -= 2.0f;
                    if (vel[1] < -75.0f) {
                        vel[1] = -75.0f;
                    }

                    break;
                case EAction.ACT_LAVA_BOOST:
                case EAction.ACT_FALL_AFTER_STAR_GRAB:
                    vel[1] -= 3.2f;
                    if (vel[1] < -65.0f) {
                        vel[1] = -65.0f;
                    }

                    break;
                case EAction.ACT_GETTING_BLOWN:
                    vel[1] -= gettingBlownGravity;
                    if (vel[1] < -75.0f) {
                        vel[1] = -75.0f;
                    }

                    break;
                default:
                    switch (true) {
                        case true when should_strengthen_gravity_for_jump_ascent():
                            vel[1] /= 4.0f;
                            break;
                        case true when action.HasFlag(EAction.ACT_FLAG_METAL_WATER):
                            vel[1] -= 1.6f;
                            if (vel[1] < -16.0f) {
                                vel[1] = -16.0f;
                            }

                            break;
                        case true when flags.HasFlag(EFlags.MARIO_WING_CAP) && vel[1] < 0.0f && input.HasFlag(EInput.INPUT_A_DOWN):
                            vel[1] -= 2.0f;
                            if (vel[1] < -37.5f) {
                                if ((vel[1] += 4.0f) > -37.5f) {
                                    vel[1] = -37.5f;
                                }
                            }

                            break;
                        default:
                            vel[1] -= 4.0f;
                            if (vel[1] < -75.0f) {
                                vel[1] = -75.0f;
                            }

                            break;
                    }

                    break;
            }
        }

        bool should_strengthen_gravity_for_jump_ascent() {
            if (!flags.HasFlag(EFlags.MARIO_UNKNOWN_08)) {
                return false;
            }

            if (action.HasFlag(EAction.ACT_FLAG_INTANGIBLE) || action.HasFlag(EAction.ACT_FLAG_INVULNERABLE)) {
                return false;
            }

            if (!input.HasFlag(EInput.INPUT_A_DOWN) && vel[1] > 20.0f) {
                return (action & EAction.ACT_FLAG_CONTROL_JUMP_HEIGHT) != 0;
            }

            return false;
        }

        public void Spawn(Vector3 position, Vector3 rotation) {
            pos = position;
            faceAngle = rotation;
            vel = Vector3.zero;
            action = EAction.ACT_IDLE;
        }
    }
}