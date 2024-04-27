using Slothsoft.UnityExtensions;
using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioController : MonoBehaviour {
        [SerializeField, Expandable]
        MarioState state;
        [SerializeField, Expandable]
        MarioObject marioObj;

        void Start() {
            state.Spawn(transform.position);
        }

        void FixedUpdate() {
            for (bool inLoop = true; inLoop;) {
                inLoop = (state.action & EAction.ACT_GROUP_MASK) switch {
                    EAction.ACT_GROUP_STATIONARY => mario_execute_stationary_action(),
                    EAction.ACT_GROUP_MOVING => mario_execute_moving_action(),
                    _ => false,
                };
            }
        }

        bool mario_execute_stationary_action() {
            Debug.Log("mario_execute_stationary_action");

            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                state.faceAngle.y = state.intendedYaw;
                state.forwardVel = Mathf.Min(state.intendedMag, 8);
                state.action = EAction.ACT_WALKING;
                return true;
            }

            return false;
        }

        bool mario_execute_moving_action() {
            Debug.Log("mario_execute_moving_action");

            if (check_common_moving_cancels()) {
                return true;
            }

            return state.action switch {
                EAction.ACT_WALKING => act_walking(),
                EAction.ACT_HOLD_WALKING => act_hold_walking(),
                EAction.ACT_HOLD_HEAVY_WALKING => act_hold_heavy_walking(),
                EAction.ACT_TURNING_AROUND => act_turning_around(),
                EAction.ACT_FINISH_TURNING_AROUND => act_finish_turning_around(),
                EAction.ACT_BRAKING => act_braking(),
                EAction.ACT_RIDING_SHELL_GROUND => act_riding_shell_ground(),
                EAction.ACT_CRAWLING => act_crawling(),
                EAction.ACT_BURNING_GROUND => act_burning_ground(),
                EAction.ACT_DECELERATING => act_decelerating(),
                EAction.ACT_HOLD_DECELERATING => act_hold_decelerating(),
                EAction.ACT_BUTT_SLIDE => act_butt_slide(),
                EAction.ACT_STOMACH_SLIDE => act_stomach_slide(),
                EAction.ACT_HOLD_BUTT_SLIDE => act_hold_butt_slide(),
                EAction.ACT_HOLD_STOMACH_SLIDE => act_hold_stomach_slide(),
                EAction.ACT_DIVE_SLIDE => act_dive_slide(),
                EAction.ACT_MOVE_PUNCHING => act_move_punching(),
                EAction.ACT_CROUCH_SLIDE => act_crouch_slide(),
                EAction.ACT_SLIDE_KICK_SLIDE => act_slide_kick_slide(),
                EAction.ACT_HARD_BACKWARD_GROUND_KB => act_hard_backward_ground_kb(),
                EAction.ACT_HARD_FORWARD_GROUND_KB => act_hard_forward_ground_kb(),
                EAction.ACT_BACKWARD_GROUND_KB => act_backward_ground_kb(),
                EAction.ACT_FORWARD_GROUND_KB => act_forward_ground_kb(),
                EAction.ACT_SOFT_BACKWARD_GROUND_KB => act_soft_backward_ground_kb(),
                EAction.ACT_SOFT_FORWARD_GROUND_KB => act_soft_forward_ground_kb(),
                EAction.ACT_GROUND_BONK => act_ground_bonk(),
                EAction.ACT_DEATH_EXIT_LAND => act_death_exit_land(),
                EAction.ACT_JUMP_LAND => act_jump_land(),
                EAction.ACT_FREEFALL_LAND => act_freefall_land(),
                EAction.ACT_DOUBLE_JUMP_LAND => act_double_jump_land(),
                EAction.ACT_SIDE_FLIP_LAND => act_side_flip_land(),
                EAction.ACT_HOLD_JUMP_LAND => act_hold_jump_land(),
                EAction.ACT_HOLD_FREEFALL_LAND => act_hold_freefall_land(),
                EAction.ACT_TRIPLE_JUMP_LAND => act_triple_jump_land(),
                EAction.ACT_BACKFLIP_LAND => act_backflip_land(),
                EAction.ACT_QUICKSAND_JUMP_LAND => act_quicksand_jump_land(),
                EAction.ACT_HOLD_QUICKSAND_JUMP_LAND => act_hold_quicksand_jump_land(),
                EAction.ACT_LONG_JUMP_LAND => act_long_jump_land(),
                _ => false,
            };
        }

        bool check_common_action_exits() {
            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return set_mario_action(EAction.ACT_JUMP, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_OFF_FLOOR)) {
                return set_mario_action(EAction.ACT_FREEFALL, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                return set_mario_action(EAction.ACT_WALKING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_ABOVE_SLIDE)) {
                return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
            }

            return false;
        }

        bool check_common_moving_cancels() {
            if (!state.action.HasFlag(EAction.ACT_FLAG_INVULNERABLE) && state.input.HasFlag(EInput.INPUT_STOMPED)) {
                return drop_and_set_mario_action(EAction.ACT_SHOCKWAVE_BOUNCE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_SQUISHED)) {
                return drop_and_set_mario_action(EAction.ACT_SQUISHED, 0);
            }

            if (!state.action.HasFlag(EAction.ACT_FLAG_INVULNERABLE)) {
                if (state.health < 0x100) {
                    return drop_and_set_mario_action(EAction.ACT_STANDING_DEATH, 0);
                }
            }

            return false;
        }

        bool act_walking() {
            int startYaw = state.faceAngle[1];

            mario_drop_held_object();

            if (state.shouldBeginSliding) {
                return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_FIRST_PERSON)) {
                return begin_braking_action();
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return set_jump_from_landing();
            }

            if (check_ground_dive_or_punch()) {
                return true;
            }

            if (state.input.HasFlag(EInput.INPUT_UNKNOWN_5)) {
                return begin_braking_action();
            }

            if (state.analogStickHeldBack && state.forwardVel >= 16.0f) {
                return set_mario_action(EAction.ACT_TURNING_AROUND, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_CROUCH_SLIDE, 0);
            }

            state.actionState = 0;

            var startPos = state.pos;
            update_walking_speed();

            switch (state.PerformGroundStep()) {
                case EGroundStep.GROUND_STEP_LEFT_GROUND:
                    set_mario_action(EAction.ACT_FREEFALL, 0);
                    break;

                case EGroundStep.GROUND_STEP_NONE:
                    if (state.intendedMag - state.forwardVel > 16.0f) {
                        //state.particleFlags |= PARTICLE_DUST;
                    }

                    break;

                case EGroundStep.GROUND_STEP_HIT_WALL:
                    //push_or_sidle_wall(EAction. startPos);
                    state.actionTimer = 0;
                    break;
            }

            //check_ledge_climb_down(m);
            //tilt_body_walking(EAction. startYaw);
            return false;
        }

        bool set_jump_from_landing() {
            return true;
        }

        bool begin_braking_action() {
            mario_drop_held_object();

            if (state.actionState == 1) {
                state.faceAngle[1] = state.actionArg;
                return set_mario_action(EAction.ACT_STANDING_AGAINST_WALL, 0);
            }

            if (state.forwardVel >= 16.0f && state.floor.normal.y >= 0.17364818f) {
                return set_mario_action(EAction.ACT_BRAKING, 0);
            }

            return set_mario_action(EAction.ACT_DECELERATING, 0);
        }

        void mario_drop_held_object() {
        }

        bool check_ground_dive_or_punch() {
            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                //! Speed kick (shoutouts to SimpleFlips)
                if (state.forwardVel >= 29.0f && state.intendedMag > 48.0f) {
                    state.vel[1] = 20.0f;
                    return set_mario_action(EAction.ACT_DIVE, 1);
                }

                return set_mario_action(EAction.ACT_MOVE_PUNCHING, 0);
            }

            return false;
        }

        void update_walking_speed() {
            float maxTargetSpeed;
            float targetSpeed;

            if (state.floor.type == ESurface.SURFACE_SLOW) {
                maxTargetSpeed = 24.0f;
            } else {
                maxTargetSpeed = 32.0f;
            }

            targetSpeed = state.intendedMag < maxTargetSpeed ? state.intendedMag : maxTargetSpeed;

            if (state.forwardVel <= 0.0f) {
                state.forwardVel += 1.1f;
            } else if (state.forwardVel <= targetSpeed) {
                state.forwardVel += 1.1f - (state.forwardVel / 43.0f);
            } else if (state.floor.normal.y >= 0.95f) {
                state.forwardVel -= 1.0f;
            }

            if (state.forwardVel > 48.0f) {
                state.forwardVel = 48.0f;
            }

            state.faceAngle[1] = state.intendedYaw - MathUtil.ApproachInt(state.intendedYaw - state.faceAngle[1], 0, 0x800, 0x800);

            apply_slope_accel();
        }

        void apply_slope_accel() {
            float slopeAccel;
            var floor = state.floor;
            float steepness = Mathf.Sqrt((floor.normal.x * floor.normal.x) + (floor.normal.z * floor.normal.z));

            int floorDYaw = state.floorAngle - state.faceAngle[1];

            if (state.floor.isSlope) {
                ESurfaceClass slopeClass = 0;

                if (state.action is not EAction.ACT_SOFT_BACKWARD_GROUND_KB and not EAction.ACT_SOFT_FORWARD_GROUND_KB) {
                    slopeClass = state.floor.surfaceClass;
                }

                slopeAccel = slopeClass switch {
                    ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY => 5.3f,
                    ESurfaceClass.SURFACE_CLASS_SLIPPERY => 2.7f,
                    ESurfaceClass.SURFACE_CLASS_NOT_SLIPPERY => 0.0f,
                    _ => 1.7f,
                };

                if (floorDYaw is > (-0x4000) and < 0x4000) {
                    state.forwardVel += slopeAccel * steepness;
                } else {
                    state.forwardVel -= slopeAccel * steepness;
                }
            }

            state.slideYaw = state.faceAngle[1];
        }

        bool apply_slope_decel(float decelCoef) {
            float decel = state.floor.surfaceClass switch {
                ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY => decelCoef * 0.2f,
                ESurfaceClass.SURFACE_CLASS_SLIPPERY => decelCoef * 0.7f,
                ESurfaceClass.SURFACE_CLASS_NOT_SLIPPERY => decelCoef * 3.0f,
                _ => decelCoef * 2.0f,
            };

            state.forwardVel = MathUtil.ApproachFloat(state.forwardVel, 0.0f, decel, decel);

            bool stopped = Mathf.Approximately(state.forwardVel, 0);

            apply_slope_accel();

            return stopped;
        }

        bool act_hold_walking() { return false; }
        bool act_hold_heavy_walking() { return false; }
        bool act_turning_around() {
            Debug.Log("turning");
            return false;
        }
        bool act_finish_turning_around() { return false; }
        bool act_braking() {
            if (!state.input.HasFlag(EInput.INPUT_FIRST_PERSON)
             && (state.input & (EInput.INPUT_NONZERO_ANALOG | EInput.INPUT_A_PRESSED | EInput.INPUT_OFF_FLOOR | EInput.INPUT_ABOVE_SLIDE)) != 0) {
                return check_common_action_exits();
            }

            if (apply_slope_decel(2.0f)) {
                return set_mario_action(EAction.ACT_BRAKING_STOP, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_MOVE_PUNCHING, 0);
            }

            switch (state.PerformGroundStep()) {
                case EGroundStep.GROUND_STEP_LEFT_GROUND:
                    set_mario_action(EAction.ACT_FREEFALL, 0);
                    break;

                case EGroundStep.GROUND_STEP_NONE:
                    //state.particleFlags |= PARTICLE_DUST;
                    break;

                case EGroundStep.GROUND_STEP_HIT_WALL:
                    //slide_bonk(EAction.ACT_BACKWARD_GROUND_KB, ACT_BRAKING_STOP);
                    break;
            }

            return false;
        }

        bool act_riding_shell_ground() { return false; }
        bool act_crawling() { return false; }
        bool act_burning_ground() { return false; }
        bool act_decelerating() {
            var slopeClass = state.floor.surfaceClass;

            if (!state.input.HasFlag(EInput.INPUT_FIRST_PERSON)) {
                if (state.shouldBeginSliding) {
                    return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
                }

                if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                    return set_jump_from_landing();
                }

                if (check_ground_dive_or_punch()) {
                    return true;
                }

                if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                    return set_mario_action(EAction.ACT_WALKING, 0);
                }

                if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                    return set_mario_action(EAction.ACT_CROUCH_SLIDE, 0);
                }
            }

            if (update_decelerating_speed()) {
                return set_mario_action(EAction.ACT_IDLE, 0);
            }

            switch (state.PerformGroundStep()) {
                case EGroundStep.GROUND_STEP_LEFT_GROUND:
                    set_mario_action(EAction.ACT_FREEFALL, 0);
                    break;
                case EGroundStep.GROUND_STEP_HIT_WALL:
                    if (slopeClass == ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY) {
                        // mario_bonk_reflection(m, TRUE);
                    } else {
                        state.forwardVel = 0;
                    }

                    break;
            }

            if (slopeClass == ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY) {
                // state.particleFlags |= PARTICLE_DUST;
            }

            return false;
        }

        bool update_decelerating_speed() {
            state.forwardVel = MathUtil.ApproachFloat(state.forwardVel, 0.0f, 1.0f, 1.0f);

            return Mathf.Approximately(state.forwardVel, 0);
        }

        bool act_hold_decelerating() { return false; }
        bool act_butt_slide() { return false; }
        bool act_stomach_slide() { return false; }
        bool act_hold_butt_slide() { return false; }
        bool act_hold_stomach_slide() { return false; }
        bool act_dive_slide() { return false; }
        bool act_move_punching() { return false; }
        bool act_crouch_slide() { return false; }
        bool act_slide_kick_slide() { return false; }
        bool act_hard_backward_ground_kb() { return false; }
        bool act_hard_forward_ground_kb() { return false; }
        bool act_backward_ground_kb() { return false; }
        bool act_forward_ground_kb() { return false; }
        bool act_soft_backward_ground_kb() { return false; }
        bool act_soft_forward_ground_kb() { return false; }
        bool act_ground_bonk() { return false; }
        bool act_death_exit_land() { return false; }
        bool act_jump_land() { return false; }
        bool act_freefall_land() { return false; }
        bool act_double_jump_land() { return false; }
        bool act_side_flip_land() { return false; }
        bool act_hold_jump_land() { return false; }
        bool act_hold_freefall_land() { return false; }
        bool act_triple_jump_land() { return false; }
        bool act_backflip_land() { return false; }
        bool act_quicksand_jump_land() { return false; }
        bool act_hold_quicksand_jump_land() { return false; }
        bool act_long_jump_land() { return false; }

        bool drop_and_set_mario_action(EAction action, int actionArg) {
            mario_stop_riding_and_holding();
            return set_mario_action(action, actionArg);
        }

        void mario_stop_riding_and_holding() {
        }

        bool set_mario_action(EAction action, int actionArg) {
            switch (action & EAction.ACT_GROUP_MASK) {
                case EAction.ACT_GROUP_MOVING:
                    action = set_mario_action_moving(action, actionArg);
                    break;

                case EAction.ACT_GROUP_AIRBORNE:
                    action = set_mario_action_airborne(action, actionArg);
                    break;

                case EAction.ACT_GROUP_SUBMERGED:
                    action = set_mario_action_submerged(action, actionArg);
                    break;

                case EAction.ACT_GROUP_CUTSCENE:
                    action = set_mario_action_cutscene(action, actionArg);
                    break;
            }

            // Resets the sound played flags, meaning Mario can play those sound types again.
            state.flags &= ~(EFlags.MARIO_ACTION_SOUND_PLAYED | EFlags.MARIO_MARIO_SOUND_PLAYED);

            if (!state.action.HasFlag(EAction.ACT_FLAG_AIR)) {
                state.flags &= ~EFlags.MARIO_UNKNOWN_18;
            }

            // Initialize the action information.
            state.prevAction = state.action;
            state.action = action;
            state.actionArg = actionArg;
            state.actionState = 0;
            state.actionTimer = 0;

            return true;
        }

        EAction set_mario_action_moving(EAction action, int actionArg) {
            var floorClass = state.floor.surfaceClass;
            float forwardVel = state.forwardVel;
            float mag = Mathf.Min(state.intendedMag, 8.0f);

            switch (action) {
                case EAction.ACT_WALKING:
                    if (floorClass != ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY) {
                        if (0.0f <= forwardVel && forwardVel < mag) {
                            state.forwardVel = mag;
                        }
                    }

                    marioObj.oMarioWalkingPitch = 0;
                    break;

                case EAction.ACT_HOLD_WALKING:
                    if (0.0f <= forwardVel && forwardVel < mag / 2.0f) {
                        state.forwardVel = mag / 2.0f;
                    }

                    break;

                case EAction.ACT_BEGIN_SLIDING:
                    if (state.isFacingDownhill) {
                        action = EAction.ACT_BUTT_SLIDE;
                    } else {
                        action = EAction.ACT_STOMACH_SLIDE;
                    }

                    break;

                case EAction.ACT_HOLD_BEGIN_SLIDING:
                    if (state.isFacingDownhill) {
                        action = EAction.ACT_HOLD_BUTT_SLIDE;
                    } else {
                        action = EAction.ACT_HOLD_STOMACH_SLIDE;
                    }

                    break;
            }

            return action;
        }

        EAction set_mario_action_airborne(EAction action, int actionArg) {
            return action;
        }
        EAction set_mario_action_submerged(EAction action, int actionArg) {
            return action;
        }
        EAction set_mario_action_cutscene(EAction action, int actionArg) {
            return action;
        }
    }
}
