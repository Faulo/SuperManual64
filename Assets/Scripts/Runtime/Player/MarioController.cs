using System;
using Slothsoft.UnityExtensions;
using SuperManual64.Level;
using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioController : MonoBehaviour {
        public event Action<int, int> onRumble;
        public event Action<ECameraShake> onCameraShake;

        [SerializeField, Expandable]
        MarioState state;
        [SerializeField, Expandable]
        MarioObject marioObj;

        void Start() {
            state.Spawn(transform.position);
        }

        void FixedUpdate() {
            marioObj.AdvanceAnimation();

            state.UpdateIntentions();

            for (bool inLoop = true; inLoop;) {
                inLoop = (state.action & EAction.ACT_GROUP_MASK) switch {
                    EAction.ACT_GROUP_STATIONARY => mario_execute_stationary_action(),
                    EAction.ACT_GROUP_MOVING => mario_execute_moving_action(),
                    EAction.ACT_GROUP_AIRBORNE => mario_execute_airborne_action(),
                    _ => false,
                };
            }
        }

        #region stationary

        bool mario_execute_stationary_action() {
            if (check_common_stationary_cancels()) {
                return true;
            }

            return state.action switch {
                EAction.ACT_IDLE => act_idle(),
                EAction.ACT_START_SLEEPING => act_start_sleeping(),
                EAction.ACT_SLEEPING => act_sleeping(),
                EAction.ACT_WAKING_UP => act_waking_up(),
                EAction.ACT_PANTING => act_panting(),
                EAction.ACT_HOLD_PANTING_UNUSED => act_hold_panting_unused(),
                EAction.ACT_HOLD_IDLE => act_hold_idle(),
                EAction.ACT_HOLD_HEAVY_IDLE => act_hold_heavy_idle(),
                EAction.ACT_IN_QUICKSAND => act_in_quicksand(),
                EAction.ACT_STANDING_AGAINST_WALL => act_standing_against_wall(),
                EAction.ACT_COUGHING => act_coughing(),
                EAction.ACT_SHIVERING => act_shivering(),
                EAction.ACT_CROUCHING => act_crouching(),
                EAction.ACT_START_CROUCHING => act_start_crouching(),
                EAction.ACT_STOP_CROUCHING => act_stop_crouching(),
                EAction.ACT_START_CRAWLING => act_start_crawling(),
                EAction.ACT_STOP_CRAWLING => act_stop_crawling(),
                EAction.ACT_SLIDE_KICK_SLIDE_STOP => act_slide_kick_slide_stop(),
                EAction.ACT_SHOCKWAVE_BOUNCE => act_shockwave_bounce(),
                EAction.ACT_FIRST_PERSON => act_first_person(),
                EAction.ACT_JUMP_LAND_STOP => act_jump_land_stop(),
                EAction.ACT_DOUBLE_JUMP_LAND_STOP => act_double_jump_land_stop(),
                EAction.ACT_FREEFALL_LAND_STOP => act_freefall_land_stop(),
                EAction.ACT_SIDE_FLIP_LAND_STOP => act_side_flip_land_stop(),
                EAction.ACT_HOLD_JUMP_LAND_STOP => act_hold_jump_land_stop(),
                EAction.ACT_HOLD_FREEFALL_LAND_STOP => act_hold_freefall_land_stop(),
                EAction.ACT_AIR_THROW_LAND => act_air_throw_land(),
                EAction.ACT_LAVA_BOOST_LAND => act_lava_boost_land(),
                EAction.ACT_TWIRL_LAND => act_twirl_land(),
                EAction.ACT_TRIPLE_JUMP_LAND_STOP => act_triple_jump_land_stop(),
                EAction.ACT_BACKFLIP_LAND_STOP => act_backflip_land_stop(),
                EAction.ACT_LONG_JUMP_LAND_STOP => act_long_jump_land_stop(),
                EAction.ACT_GROUND_POUND_LAND => act_ground_pound_land(),
                EAction.ACT_BRAKING_STOP => act_braking_stop(),
                EAction.ACT_BUTT_SLIDE_STOP => act_butt_slide_stop(),
                EAction.ACT_HOLD_BUTT_SLIDE_STOP => act_hold_butt_slide_stop(),
                _ => throw new NotImplementedException(state.action.ToString()),
            };
        }

        bool check_common_stationary_cancels() {
            if (state.action != EAction.ACT_UNKNOWN_0002020E) {
                if (state.health < 0x100) {
                    return drop_and_set_mario_action(EAction.ACT_STANDING_DEATH, 0);
                }
            }

            return false;
        }

        bool act_idle() {
            if (check_common_idle_cancels()) {
                return true;
            }

            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                state.faceAngleYaw = state.intendedYaw;
                state.forwardVel = Mathf.Min(state.intendedMag, 8);
                state.action = EAction.ACT_WALKING;
                return true;
            }

            state.StationaryGroundStep();

            return false;
        }
        bool check_common_idle_cancels() {
            mario_drop_held_object();

            if (state.floor.normal.y < 0.29237169f) {
                return mario_push_off_steep_floor(EAction.ACT_FREEFALL, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_STOMPED)) {
                return set_mario_action(EAction.ACT_SHOCKWAVE_BOUNCE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return set_jumping_action(EAction.ACT_JUMP, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_OFF_FLOOR)) {
                return set_mario_action(EAction.ACT_FREEFALL, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_ABOVE_SLIDE)) {
                return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_FIRST_PERSON)) {
                return set_mario_action(EAction.ACT_FIRST_PERSON, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                state.faceAngleYaw = state.intendedYaw;
                return set_mario_action(EAction.ACT_WALKING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_PUNCHING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_Z_DOWN)) {
                return set_mario_action(EAction.ACT_START_CROUCHING, 0);
            }

            return false;
        }

        bool mario_push_off_steep_floor(EAction action, int actionArg) {
            float floorDYaw = state.floorAngle - state.faceAngleYaw;

            if (floorDYaw is > (-0x4000) and < 0x4000) {
                state.forwardVel = 16.0f;
                state.faceAngleYaw = state.floorAngle;
            } else {
                state.forwardVel = -16.0f;
                state.faceAngleYaw = state.floorAngle + 180;
            }

            return set_mario_action(action, actionArg);
        }

        bool act_start_sleeping() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_sleeping() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_waking_up() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_panting() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_hold_panting_unused() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_hold_idle() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_hold_heavy_idle() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_in_quicksand() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_standing_against_wall() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_coughing() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_shivering() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_crouching() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_start_crouching() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_stop_crouching() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_start_crawling() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_stop_crawling() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_slide_kick_slide_stop() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_shockwave_bounce() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_first_person() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_jump_land_stop() {
            if (check_common_landing_cancels(EAction.ACT_UNINITIALIZED)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_LAND_FROM_SINGLE_JUMP, EAction.ACT_IDLE);

            return false;
        }

        bool check_common_landing_cancels(EAction action) {
            if (state.input.HasFlag(EInput.INPUT_STOMPED)) {
                return set_mario_action(EAction.ACT_SHOCKWAVE_BOUNCE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_FIRST_PERSON)) {
                return set_mario_action(EAction.ACT_IDLE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                if (action == EAction.ACT_UNINITIALIZED) {
                    return set_jump_from_landing();
                } else {
                    return set_jumping_action(action, 0);
                }
            }

            if ((state.input & (EInput.INPUT_NONZERO_ANALOG | EInput.INPUT_A_PRESSED | EInput.INPUT_OFF_FLOOR | EInput.INPUT_ABOVE_SLIDE)) != 0) {
                return check_common_action_exits();
            }

            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_PUNCHING, 0);
            }

            return false;
        }

        bool landing_step(EAnim animation, EAction action) {
            state.StationaryGroundStep();

            marioObj.SetAnimation(animation);
            if (marioObj.animInfo.isAnimAtEnd) {
                return set_mario_action(action, 0);
            }

            return false;
        }

        bool act_double_jump_land_stop() {
            if (check_common_landing_cancels(EAction.ACT_UNINITIALIZED)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_LAND_FROM_DOUBLE_JUMP, EAction.ACT_IDLE);
            return false;
        }
        bool act_freefall_land_stop() {
            if (check_common_landing_cancels(EAction.ACT_UNINITIALIZED)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_GENERAL_LAND, EAction.ACT_IDLE);
            return false;
        }
        bool act_side_flip_land_stop() {
            if (check_common_landing_cancels(EAction.ACT_UNINITIALIZED)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_GENERAL_LAND, EAction.ACT_IDLE);
            return false;
        }
        bool act_hold_jump_land_stop() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_hold_freefall_land_stop() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_air_throw_land() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_lava_boost_land() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_twirl_land() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_triple_jump_land_stop() {
            if (check_common_landing_cancels(EAction.ACT_JUMP)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_TRIPLE_JUMP_LAND, EAction.ACT_IDLE);
            return false;
        }
        bool act_backflip_land_stop() {
            if (!state.input.HasFlag(EInput.INPUT_Z_DOWN)) {
                state.input &= ~EInput.INPUT_A_PRESSED;
            }

            if (check_common_landing_cancels(EAction.ACT_BACKFLIP)) {
                return true;
            }

            landing_step(EAnim.MARIO_ANIM_TRIPLE_JUMP_LAND, EAction.ACT_IDLE);
            return false;
        }
        bool act_long_jump_land_stop() {
            state.input &= ~EInput.INPUT_B_PRESSED;

            if (check_common_landing_cancels(EAction.ACT_JUMP)) {
                return true;
            }

            landing_step(marioObj.oMarioLongJumpIsSlow ? EAnim.MARIO_ANIM_CROUCH_FROM_SLOW_LONGJUMP : EAnim.MARIO_ANIM_CROUCH_FROM_FAST_LONGJUMP, EAction.ACT_CROUCHING);
            return false;
        }
        bool act_ground_pound_land() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_braking_stop() {
            if (state.input.HasFlag(EInput.INPUT_STOMPED)) {
                return set_mario_action(EAction.ACT_SHOCKWAVE_BOUNCE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_OFF_FLOOR)) {
                return set_mario_action(EAction.ACT_FREEFALL, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_PUNCHING, 0);
            }

            if (!state.input.HasFlag(EInput.INPUT_FIRST_PERSON)
             && (state.input & (EInput.INPUT_NONZERO_ANALOG | EInput.INPUT_A_PRESSED | EInput.INPUT_OFF_FLOOR | EInput.INPUT_ABOVE_SLIDE)) != 0) {
                return check_common_action_exits();
            }

            stopping_step(EAnim.MARIO_ANIM_STOP_SKID, EAction.ACT_IDLE);
            return false;
        }

        void stopping_step(EAnim animation, EAction action) {
            marioObj.SetAnimation(animation);

            state.StationaryGroundStep();

            if (marioObj.animInfo.isAnimAtEnd) {
                set_mario_action(action, 0);
            }
        }

        bool act_butt_slide_stop() {
            state.StationaryGroundStep();
            return false;
        }
        bool act_hold_butt_slide_stop() {
            state.StationaryGroundStep();
            return false;
        }

        #endregion

        #region moving

        bool mario_execute_moving_action() {
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
                _ => throw new NotImplementedException(state.action.ToString()),
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

            if (state.input.HasFlag(EInput.INPUT_NEITHER_STICK_NOR_A)) {
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

            //check_ledge_climb_down();
            return false;
        }

        bool set_jump_from_landing() {
            if (state.floor.isSteep) {
                set_steep_jump_action();
            } else {
                if ((state.doubleJumpTimer == 0) || (state.squishTimer != 0)) {
                    set_mario_action(EAction.ACT_JUMP, 0);
                } else {
                    switch (state.prevAction) {
                        case EAction.ACT_JUMP_LAND:
                            set_mario_action(EAction.ACT_DOUBLE_JUMP, 0);
                            break;

                        case EAction.ACT_FREEFALL_LAND:
                            set_mario_action(EAction.ACT_DOUBLE_JUMP, 0);
                            break;

                        case EAction.ACT_SIDE_FLIP_LAND_STOP:
                            set_mario_action(EAction.ACT_DOUBLE_JUMP, 0);
                            break;

                        case EAction.ACT_DOUBLE_JUMP_LAND:
                            // If Mario has a wing cap, he ignores the typical speed
                            // requirement for a triple jump.
                            if (state.flags.HasFlag(EFlags.MARIO_WING_CAP)) {
                                set_mario_action(EAction.ACT_FLYING_TRIPLE_JUMP, 0);
                            } else if (state.forwardVel > 20.0f) {
                                set_mario_action(EAction.ACT_TRIPLE_JUMP, 0);
                            } else {
                                set_mario_action(EAction.ACT_JUMP, 0);
                            }

                            break;

                        default:
                            set_mario_action(EAction.ACT_JUMP, 0);
                            break;
                    }
                }
            }

            state.doubleJumpTimer = 0;

            return true;
        }

        bool begin_braking_action() {
            mario_drop_held_object();

            if (state.actionState == 1) {
                state.faceAngleYaw = state.actionArg;
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
                if (state.forwardVel >= 29.0f && state.intendedMag > 24.0f) {
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

            state.faceAngleYaw = Mathf.MoveTowardsAngle(state.faceAngleYaw, state.intendedYaw, 11.25f);

            apply_slope_accel();
        }

        void apply_slope_accel() {
            float slopeAccel;
            var floor = state.floor;
            float steepness = Mathf.Sqrt((floor.normal.x * floor.normal.x) + (floor.normal.z * floor.normal.z));

            float floorDYaw = state.floorAngle - state.faceAngleYaw;

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

            state.slideYaw = state.faceAngleYaw;
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
            if (state.input.HasFlag(EInput.INPUT_ABOVE_SLIDE)) {
                return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return set_jumping_action(EAction.ACT_SIDE_FLIP, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_NEITHER_STICK_NOR_A)) {
                return set_mario_action(EAction.ACT_BRAKING, 0);
            }

            if (!state.analogStickHeldBack) {
                return set_mario_action(EAction.ACT_WALKING, 0);
            }

            if (apply_slope_decel(2.0f)) {
                return begin_walking_action(8.0f, EAction.ACT_FINISH_TURNING_AROUND, 0);
            }

            switch (state.PerformGroundStep()) {
                case EGroundStep.GROUND_STEP_LEFT_GROUND:
                    set_mario_action(EAction.ACT_FREEFALL, 0);
                    break;

                case EGroundStep.GROUND_STEP_NONE:
                    //state.particleFlags |= PARTICLE_DUST;
                    break;
            }

            if (state.forwardVel >= 18.0f) {
                marioObj.SetAnimation(EAnim.MARIO_ANIM_TURNING_PART1);
            } else {
                marioObj.SetAnimation(EAnim.MARIO_ANIM_TURNING_PART2);
                if (marioObj.animInfo.isAnimAtEnd) {
                    if (state.forwardVel > 0.0f) {
                        begin_walking_action(-state.forwardVel, EAction.ACT_WALKING, 0);
                    } else {
                        begin_walking_action(8.0f, EAction.ACT_WALKING, 0);
                    }
                }
            }

            return false;
        }
        bool begin_walking_action(float forwardVel, EAction action, int actionArg) {
            state.faceAngleYaw = state.intendedYaw;
            state.forwardVel = forwardVel;
            return set_mario_action(action, actionArg);
        }
        bool act_finish_turning_around() {
            if (state.input.HasFlag(EInput.INPUT_ABOVE_SLIDE)) {
                return set_mario_action(EAction.ACT_BEGIN_SLIDING, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return set_jumping_action(EAction.ACT_SIDE_FLIP, 0);
            }

            update_walking_speed();
            marioObj.SetAnimation(EAnim.MARIO_ANIM_TURNING_PART2);

            if (state.PerformGroundStep() == EGroundStep.GROUND_STEP_LEFT_GROUND) {
                set_mario_action(EAction.ACT_FREEFALL, 0);
            }

            if (marioObj.animInfo.isAnimAtEnd) {
                set_mario_action(EAction.ACT_WALKING, 0);
            }

            return false;
        }
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
                        // mario_bonk_reflection(TRUE);
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
        bool act_jump_land() {
            if (common_landing_cancels(LandingAction.sJumpLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_LAND_FROM_SINGLE_JUMP, EAction.ACT_FREEFALL);

            return false;
        }

        bool common_landing_cancels(LandingAction landingAction, Func<EAction, int, bool> setAPressAction) {
            if (state.floor.normal.y < 0.2923717f) {
                return mario_push_off_steep_floor(landingAction.verySteepAction, 0);
            }

            state.doubleJumpTimer = landingAction.unk02;

            if (state.shouldBeginSliding) {
                return set_mario_action(landingAction.slideAction, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_FIRST_PERSON)) {
                return set_mario_action(landingAction.endAction, 0);
            }

            if (++state.actionTimer >= landingAction.numFrames) {
                return set_mario_action(landingAction.endAction, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_A_PRESSED)) {
                return setAPressAction(landingAction.aPressedAction, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_OFF_FLOOR)) {
                return set_mario_action(landingAction.offFloorAction, 0);
            }

            return false;
        }

        void common_landing_action(EAnim animation, EAction airAction) {
            if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                apply_landing_accel(0.98f);
            } else if (state.forwardVel >= 16.0f) {
                apply_slope_decel(2.0f);
            } else {
                state.vel[1] = 0.0f;
            }

            switch (state.PerformGroundStep()) {
                case EGroundStep.GROUND_STEP_LEFT_GROUND:
                    set_mario_action(airAction, 0);
                    break;

                case EGroundStep.GROUND_STEP_HIT_WALL:
                    break;
            }

            if (state.forwardVel > 16.0f) {
                //state.particleFlags |= PARTICLE_DUST;
            }

            marioObj.SetAnimation(animation);
        }

        bool apply_landing_accel(float frictionFactor) {
            bool stopped = false;

            apply_slope_accel();

            if (!state.floor.isSlope) {
                state.forwardVel *= frictionFactor;
                if (state.forwardVel * state.forwardVel < 1.0f) {
                    state.forwardVel = 0;
                    stopped = true;
                }
            }

            return stopped;
        }

        bool act_freefall_land() {
            if (common_landing_cancels(LandingAction.sFreefallLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_GENERAL_LAND, EAction.ACT_FREEFALL);
            return false;
        }
        bool act_double_jump_land() {
            if (common_landing_cancels(LandingAction.sDoubleJumpLandAction, set_triple_jump_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_LAND_FROM_DOUBLE_JUMP, EAction.ACT_FREEFALL);
            return false;
        }
        bool act_side_flip_land() {
            if (common_landing_cancels(LandingAction.sSideFlipLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_SLIDEFLIP_LAND, EAction.ACT_FREEFALL);
            return false;
        }
        bool act_hold_jump_land() {
            if (common_landing_cancels(LandingAction.sHoldJumpLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_JUMP_LAND_WITH_LIGHT_OBJ, EAction.ACT_HOLD_FREEFALL);
            return false;
        }
        bool act_hold_freefall_land() {
            if (common_landing_cancels(LandingAction.sHoldFreefallLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_FALL_LAND_WITH_LIGHT_OBJ, EAction.ACT_HOLD_FREEFALL);
            return false;
        }
        bool act_triple_jump_land() {
            state.input &= ~EInput.INPUT_A_PRESSED;

            if (common_landing_cancels(LandingAction.sTripleJumpLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_TRIPLE_JUMP_LAND, EAction.ACT_FREEFALL);
            return false;
        }
        bool act_backflip_land() {
            if (!state.input.HasFlag(EInput.INPUT_Z_DOWN)) {
                state.input &= ~EInput.INPUT_A_PRESSED;
            }

            if (common_landing_cancels(LandingAction.sBackflipLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(EAnim.MARIO_ANIM_TRIPLE_JUMP_LAND, EAction.ACT_FREEFALL);
            return false;
        }
        bool act_quicksand_jump_land() {
            return false;
        }
        bool act_hold_quicksand_jump_land() {
            return false;
        }
        bool act_long_jump_land() {
            if (!state.input.HasFlag(EInput.INPUT_Z_DOWN)) {
                state.input &= ~EInput.INPUT_A_PRESSED;
            }

            if (common_landing_cancels(LandingAction.sLongJumpLandAction, set_jumping_action)) {
                return true;
            }

            common_landing_action(marioObj.oMarioLongJumpIsSlow ? EAnim.MARIO_ANIM_CROUCH_FROM_SLOW_LONGJUMP : EAnim.MARIO_ANIM_CROUCH_FROM_FAST_LONGJUMP, EAction.ACT_FREEFALL);
            return false;
        }

        #endregion

        #region airborne
        bool mario_execute_airborne_action() {
            if (check_common_airborne_cancels()) {
                return true;
            }

            return state.action switch {
                EAction.ACT_JUMP => act_jump(),
                EAction.ACT_DOUBLE_JUMP => act_double_jump(),
                EAction.ACT_FREEFALL => act_freefall(),
                EAction.ACT_HOLD_JUMP => act_hold_jump(),
                EAction.ACT_HOLD_FREEFALL => act_hold_freefall(),
                EAction.ACT_SIDE_FLIP => act_side_flip(),
                EAction.ACT_WALL_KICK_AIR => act_wall_kick_air(),
                EAction.ACT_TWIRLING => act_twirling(),
                EAction.ACT_WATER_JUMP => act_water_jump(),
                EAction.ACT_HOLD_WATER_JUMP => act_hold_water_jump(),
                EAction.ACT_STEEP_JUMP => act_steep_jump(),
                EAction.ACT_BURNING_JUMP => act_burning_jump(),
                EAction.ACT_BURNING_FALL => act_burning_fall(),
                EAction.ACT_TRIPLE_JUMP => act_triple_jump(),
                EAction.ACT_BACKFLIP => act_backflip(),
                EAction.ACT_LONG_JUMP => act_long_jump(),
                EAction.ACT_RIDING_SHELL_JUMP => act_riding_shell_air(),
                EAction.ACT_RIDING_SHELL_FALL => act_riding_shell_air(),
                EAction.ACT_DIVE => act_dive(),
                EAction.ACT_AIR_THROW => act_air_throw(),
                EAction.ACT_BACKWARD_AIR_KB => act_backward_air_kb(),
                EAction.ACT_FORWARD_AIR_KB => act_forward_air_kb(),
                EAction.ACT_HARD_FORWARD_AIR_KB => act_hard_forward_air_kb(),
                EAction.ACT_HARD_BACKWARD_AIR_KB => act_hard_backward_air_kb(),
                EAction.ACT_SOFT_BONK => act_soft_bonk(),
                EAction.ACT_AIR_HIT_WALL => act_air_hit_wall(),
                EAction.ACT_FORWARD_ROLLOUT => act_forward_rollout(),
                EAction.ACT_SHOT_FROM_CANNON => act_shot_from_cannon(),
                EAction.ACT_BUTT_SLIDE_AIR => act_butt_slide_air(),
                EAction.ACT_HOLD_BUTT_SLIDE_AIR => act_hold_butt_slide_air(),
                EAction.ACT_LAVA_BOOST => act_lava_boost(),
                EAction.ACT_GETTING_BLOWN => act_getting_blown(),
                EAction.ACT_BACKWARD_ROLLOUT => act_backward_rollout(),
                EAction.ACT_CRAZY_BOX_BOUNCE => act_crazy_box_bounce(),
                EAction.ACT_SPECIAL_TRIPLE_JUMP => act_special_triple_jump(),
                EAction.ACT_GROUND_POUND => act_ground_pound(),
                EAction.ACT_THROWN_FORWARD => act_thrown_forward(),
                EAction.ACT_THROWN_BACKWARD => act_thrown_backward(),
                EAction.ACT_FLYING_TRIPLE_JUMP => act_flying_triple_jump(),
                EAction.ACT_SLIDE_KICK => act_slide_kick(),
                EAction.ACT_JUMP_KICK => act_jump_kick(),
                EAction.ACT_FLYING => act_flying(),
                EAction.ACT_RIDING_HOOT => act_riding_hoot(),
                EAction.ACT_TOP_OF_POLE_JUMP => act_top_of_pole_jump(),
                EAction.ACT_VERTICAL_WIND => act_vertical_wind(),
                _ => throw new NotImplementedException(state.action.ToString()),
            };
        }

        bool act_jump() {
            if (check_kick_or_dive_in_air()) {
                return true;
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_GROUND_POUND, 0);
            }

            common_air_action_step(EAction.ACT_JUMP_LAND, EAnim.MARIO_ANIM_SINGLE_JUMP, EAirStep.AIR_STEP_CHECK_LEDGE_GRAB | EAirStep.AIR_STEP_CHECK_HANG);
            return false;
        }

        bool check_kick_or_dive_in_air() {
            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(state.forwardVel > 28.0f ? EAction.ACT_DIVE : EAction.ACT_JUMP_KICK, 0);
            }

            return false;
        }

        void common_air_action_step(EAction landAction, EAnim animation, EAirStep stepArg) {
            update_air_without_turn();

            switch (state.PerformAirStep(stepArg)) {
                case EAirStep.AIR_STEP_NONE:
                    marioObj.SetAnimation(animation);
                    break;

                case EAirStep.AIR_STEP_LANDED:
                    if (!check_fall_damage_or_get_stuck(EAction.ACT_HARD_BACKWARD_GROUND_KB)) {
                        set_mario_action(landAction, 0);
                    }

                    break;

                case EAirStep.AIR_STEP_HIT_WALL:
                    marioObj.SetAnimation(animation);

                    if (state.forwardVel > 16.0f) {
                        onRumble?.Invoke(5, 40);
                        mario_bonk_reflection(false);
                        state.faceAngleYaw += 180;

                        if (state.wall is not null) {
                            set_mario_action(EAction.ACT_AIR_HIT_WALL, 0);
                        } else {
                            if (state.vel[1] > 0.0f) {
                                state.vel[1] = 0.0f;
                            }

                            //! Hands-free holding. Bonking while no wall is referenced
                            // sets Mario's action to a non-holding action without
                            // dropping the object, causing the hands-free holding
                            // glitch. This can be achieved using an exposed ceiling,
                            // out of bounds, grazing the bottom of a wall while
                            // falling such that the final quarter step does not find a
                            // wall collision, or by rising into the top of a wall such
                            // that the final quarter step detects a ledge, but you are
                            // not able to ledge grab it.
                            if (state.forwardVel >= 38.0f) {
                                //state.particleFlags |= PARTICLE_VERTICAL_STAR;
                                set_mario_action(EAction.ACT_BACKWARD_AIR_KB, 0);
                            } else {
                                if (state.forwardVel > 8.0f) {
                                    state.forwardVel = -8.0f;
                                }

                                set_mario_action(EAction.ACT_SOFT_BONK, 0);
                                return;
                            }
                        }
                    } else {
                        state.forwardVel = 0;
                    }

                    break;

                case EAirStep.AIR_STEP_GRABBED_LEDGE:
                    marioObj.SetAnimation(EAnim.MARIO_ANIM_IDLE_ON_LEDGE);
                    drop_and_set_mario_action(EAction.ACT_LEDGE_GRAB, 0);
                    break;

                case EAirStep.AIR_STEP_GRABBED_CEILING:
                    set_mario_action(EAction.ACT_START_HANGING, 0);
                    break;

                case EAirStep.AIR_STEP_HIT_LAVA_WALL:
                    // lava_boost_on_wall(m);
                    break;
            }
        }

        void mario_bonk_reflection(bool negateSpeed) {
            if (state.wall is not null) {
                float wallAngle = Mathf.Atan2(state.wall.normal.z, state.wall.normal.x) * Mathf.Rad2Deg;
                state.faceAngleYaw = wallAngle - (state.faceAngleYaw - wallAngle);
            }

            if (negateSpeed) {
                state.forwardVel = -state.forwardVel;
            } else {
                state.faceAngleYaw += 180;
            }
        }

        bool check_fall_damage_or_get_stuck(EAction hardFallAction) {
            if (state.shouldGetStuckInGround) {
                //state.particleFlags |= PARTICLE_MIST_CIRCLE;
                drop_and_set_mario_action(EAction.ACT_FEET_STUCK_IN_GROUND, 0);
                onRumble?.Invoke(5, 80);
                return true;
            }

            return check_fall_damage(hardFallAction);
        }

        bool check_fall_damage(EAction hardFallAction) {
            float fallHeight = state.peakHeight - state.pos[1];

            float damageHeight = state.action == EAction.ACT_GROUND_POUND
                ? 600.0f
                : 1150.0f;

            if (state.action != EAction.ACT_TWIRLING && state.floor.type != ESurface.SURFACE_BURNING) {
                if (state.vel[1] < -55.0f) {
                    if (fallHeight > 3000.0f) {
                        state.hurtCounter += state.flags.HasFlag(EFlags.MARIO_CAP_ON_HEAD)
                            ? 16
                            : 24;
                        onRumble?.Invoke(5, 80);
                        onCameraShake?.Invoke(ECameraShake.SHAKE_FALL_DAMAGE);
                        return drop_and_set_mario_action(hardFallAction, 4);
                    } else if (fallHeight > damageHeight && !state.floor.isSlippery) {
                        state.hurtCounter += state.flags.HasFlag(EFlags.MARIO_CAP_ON_HEAD)
                            ? 8
                            : 12;
                        state.squishTimer = 30;
                        onRumble?.Invoke(5, 80);
                        onCameraShake?.Invoke(ECameraShake.SHAKE_FALL_DAMAGE);
                    }
                }
            }

            return false;
        }

        void update_air_without_turn() {
            if (!check_horizontal_wind()) {
                float sidewaysSpeed = 0.0f;
                float dragThreshold = state.action == EAction.ACT_LONG_JUMP
                    ? 48.0f
                    : 32.0f;
                state.forwardVel = MathUtil.ApproachFloat(state.forwardVel, 0.0f, 0.35f, 0.35f);

                if (state.input.HasFlag(EInput.INPUT_NONZERO_ANALOG)) {
                    float intendedDYaw = Mathf.DeltaAngle(state.intendedYaw, state.faceAngleYaw);
                    float intendedMag = state.intendedMag / 32.0f;

                    state.forwardVel += intendedMag * Mathf.Cos(intendedDYaw * Mathf.Deg2Rad) * 1.5f;
                    sidewaysSpeed = intendedMag * Mathf.Sin(intendedDYaw * Mathf.Deg2Rad) * 10.0f;
                }

                //! Uncapped air speed. Net positive when moving forward.
                if (state.forwardVel > dragThreshold) {
                    state.forwardVel -= 1.0f;
                }

                if (state.forwardVel < -16.0f) {
                    state.forwardVel += 2.0f;
                }

                state.slideVelX = state.forwardVel * Mathf.Sin(state.faceAngleYaw * Mathf.Deg2Rad);
                state.slideVelZ = state.forwardVel * Mathf.Cos(state.faceAngleYaw * Mathf.Deg2Rad);

                state.slideVelX += sidewaysSpeed * Mathf.Sin((state.faceAngleYaw - 90) * Mathf.Deg2Rad);
                state.slideVelZ += sidewaysSpeed * Mathf.Cos((state.faceAngleYaw - 90) * Mathf.Deg2Rad);

                state.vel[0] = state.slideVelX;
                state.vel[2] = state.slideVelZ;
            }
        }

        bool check_horizontal_wind() => false;

        bool act_double_jump() {
            var animation = (state.vel[1] >= 0.0f)
                ? EAnim.MARIO_ANIM_DOUBLE_JUMP_RISE
                : EAnim.MARIO_ANIM_DOUBLE_JUMP_FALL;

            if (check_kick_or_dive_in_air()) {
                return true;
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_GROUND_POUND, 0);
            }

            common_air_action_step(EAction.ACT_DOUBLE_JUMP_LAND, animation, EAirStep.AIR_STEP_CHECK_LEDGE_GRAB | EAirStep.AIR_STEP_CHECK_HANG);

            return false;
        }
        bool act_freefall() {
            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_DIVE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_GROUND_POUND, 0);
            }

            var animation = state.actionArg switch {
                1 => EAnim.MARIO_ANIM_FALL_FROM_SLIDE,
                2 => EAnim.MARIO_ANIM_FALL_FROM_SLIDE_KICK,
                _ => EAnim.MARIO_ANIM_GENERAL_FALL,
            };

            common_air_action_step(EAction.ACT_FREEFALL_LAND, animation, EAirStep.AIR_STEP_CHECK_LEDGE_GRAB);
            return false;
        }
        bool act_hold_jump() { return false; }
        bool act_hold_freefall() { return false; }
        bool act_side_flip() {
            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_DIVE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_GROUND_POUND, 0);
            }

            common_air_action_step(EAction.ACT_SIDE_FLIP_LAND, EAnim.MARIO_ANIM_SLIDEFLIP, EAirStep.AIR_STEP_CHECK_LEDGE_GRAB);

            return false;
        }
        bool act_wall_kick_air() { return false; }
        bool act_twirling() { return false; }
        bool act_water_jump() { return false; }
        bool act_hold_water_jump() { return false; }
        bool act_steep_jump() { return false; }
        bool act_burning_jump() { return false; }
        bool act_burning_fall() { return false; }
        bool act_triple_jump() {
            if (state.hasSpecialTripleJump) {
                return set_mario_action(EAction.ACT_SPECIAL_TRIPLE_JUMP, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_B_PRESSED)) {
                return set_mario_action(EAction.ACT_DIVE, 0);
            }

            if (state.input.HasFlag(EInput.INPUT_Z_PRESSED)) {
                return set_mario_action(EAction.ACT_GROUND_POUND, 0);
            }

            common_air_action_step(EAction.ACT_TRIPLE_JUMP_LAND, EAnim.MARIO_ANIM_TRIPLE_JUMP, 0);
            if (state.action == EAction.ACT_TRIPLE_JUMP_LAND) {
                onRumble?.Invoke(5, 40);
            }

            return false;
        }
        bool act_backflip() { return false; }
        bool act_long_jump() { return false; }
        bool act_riding_shell_air() { return false; }
        bool act_dive() { return false; }
        bool act_air_throw() { return false; }
        bool act_backward_air_kb() { return false; }
        bool act_forward_air_kb() { return false; }
        bool act_hard_forward_air_kb() { return false; }
        bool act_hard_backward_air_kb() { return false; }
        bool act_soft_bonk() {
            if (check_wall_kick()) {
                return true;
            }

            common_air_knockback_step(EAction.ACT_FREEFALL_LAND, EAction.ACT_HARD_BACKWARD_GROUND_KB, EAnim.MARIO_ANIM_GENERAL_FALL, state.forwardVel);
            return false;
        }

        void common_air_knockback_step(EAction landAction, EAction hardFallAction, EAnim animation, float speed) {
            state.forwardVel = speed;

            switch (state.PerformAirStep(0)) {
                case EAirStep.AIR_STEP_NONE:
                    marioObj.SetAnimation(animation);
                    break;

                case EAirStep.AIR_STEP_LANDED:
                    onRumble?.Invoke(5, 40);
                    if (!check_fall_damage_or_get_stuck(hardFallAction)) {
                        if (state.action is EAction.ACT_THROWN_FORWARD or EAction.ACT_THROWN_BACKWARD) {
                            set_mario_action(landAction, state.hurtCounter);
                        } else {
                            set_mario_action(landAction, state.actionArg);
                        }
                    }

                    break;

                case EAirStep.AIR_STEP_HIT_WALL:
                    marioObj.SetAnimation(EAnim.MARIO_ANIM_BACKWARD_AIR_KB);
                    mario_bonk_reflection(false);

                    if (state.vel[1] > 0.0f) {
                        state.vel[1] = 0.0f;
                    }

                    state.forwardVel = -speed;
                    break;

                case EAirStep.AIR_STEP_HIT_LAVA_WALL:
                    //lava_boost_on_wall(m);
                    break;
            }

            return;
        }

        bool check_wall_kick() {
            if (state.input.HasFlag(EInput.INPUT_A_PRESSED) && state.wallKickTimer != 0 && state.prevAction == EAction.ACT_AIR_HIT_WALL) {
                state.faceAngleYaw += 180;
                return set_mario_action(EAction.ACT_WALL_KICK_AIR, 0);
            }

            return false;
        }

        bool act_air_hit_wall() { return false; }
        bool act_forward_rollout() { return false; }
        bool act_shot_from_cannon() { return false; }
        bool act_butt_slide_air() { return false; }
        bool act_hold_butt_slide_air() { return false; }
        bool act_lava_boost() { return false; }
        bool act_getting_blown() { return false; }
        bool act_backward_rollout() { return false; }
        bool act_crazy_box_bounce() { return false; }
        bool act_special_triple_jump() { return false; }
        bool act_ground_pound() { return false; }
        bool act_thrown_forward() { return false; }
        bool act_thrown_backward() { return false; }
        bool act_flying_triple_jump() { return false; }
        bool act_slide_kick() { return false; }
        bool act_jump_kick() { return false; }
        bool act_flying() { return false; }
        bool act_riding_hoot() { return false; }
        bool act_top_of_pole_jump() { return false; }
        bool act_vertical_wind() { return false; }

        bool check_common_airborne_cancels() {
            if (state.input.HasFlag(EInput.INPUT_SQUISHED)) {
                return drop_and_set_mario_action(EAction.ACT_SQUISHED, 0);
            }

            if (state.floor.type == ESurface.SURFACE_VERTICAL_WIND && state.action.HasFlag(EAction.ACT_FLAG_ALLOW_VERTICAL_WIND_ACTION)) {
                return drop_and_set_mario_action(EAction.ACT_VERTICAL_WIND, 0);
            }

            return false;
        }
        #endregion

        #region misc

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
            state.actionName = action.ToString().Split("ACT_")[^1].Replace('_', ' ');
            state.actionArg = actionArg;
            state.actionState = 0;
            state.actionTimer = 0;

            return true;
        }

        bool set_jumping_action(EAction action, int actionArg) {
            if (state.floor.isSteep) {
                set_steep_jump_action();
            } else {
                set_mario_action(action, actionArg);
            }

            return true;
        }

        bool set_triple_jump_action(EAction action, int actionArg) {
            if (state.flags.HasFlag(EFlags.MARIO_WING_CAP)) {
                return set_mario_action(EAction.ACT_FLYING_TRIPLE_JUMP, 0);
            }

            if (state.forwardVel > 20.0f) {
                return set_mario_action(EAction.ACT_TRIPLE_JUMP, 0);
            }

            return set_mario_action(action, actionArg);
        }

        void set_steep_jump_action() {
            marioObj.oMarioSteepJumpYaw = state.faceAngleYaw;

            if (state.forwardVel > 0.0f) {
                float angleTemp = state.floorAngle + 22.5f;
                float faceAngleTemp = state.faceAngleYaw - angleTemp;

                float y = Mathf.Sin(faceAngleTemp * Mathf.Deg2Rad) * state.forwardVel;
                float x = Mathf.Cos(faceAngleTemp * Mathf.Deg2Rad) * state.forwardVel * 0.75f;

                state.forwardVel = Mathf.Sqrt((y * y) + (x * x));
                state.faceAngleYaw = (Mathf.Atan2(x, y) * Mathf.Rad2Deg) + angleTemp;
            }

            drop_and_set_mario_action(EAction.ACT_STEEP_JUMP, 0);
        }

        EAction set_mario_action_moving(EAction action, int actionArg) {
            _ = actionArg;

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
            if (state.squishTimer != 0 && action is EAction.ACT_DOUBLE_JUMP or EAction.ACT_TWIRLING) {
                action = EAction.ACT_JUMP;
            }

            switch (action) {
                case EAction.ACT_DOUBLE_JUMP:
                    set_mario_y_vel_based_on_fspeed(52.0f, 0.25f);
                    state.forwardVel *= 0.8f;
                    break;

                case EAction.ACT_BACKFLIP:
                    state.forwardVel = -16.0f;
                    set_mario_y_vel_based_on_fspeed(62.0f, 0.0f);
                    break;

                case EAction.ACT_TRIPLE_JUMP:
                    set_mario_y_vel_based_on_fspeed(69.0f, 0.0f);
                    state.forwardVel *= 0.8f;
                    break;

                case EAction.ACT_FLYING_TRIPLE_JUMP:
                    set_mario_y_vel_based_on_fspeed(82.0f, 0.0f);
                    break;

                case EAction.ACT_WATER_JUMP:
                case EAction.ACT_HOLD_WATER_JUMP:
                    if (actionArg == 0) {
                        set_mario_y_vel_based_on_fspeed(42.0f, 0.0f);
                    }

                    break;

                case EAction.ACT_BURNING_JUMP:
                    state.vel[1] = 31.5f;
                    state.forwardVel = 8.0f;
                    break;

                case EAction.ACT_RIDING_SHELL_JUMP:
                    set_mario_y_vel_based_on_fspeed(42.0f, 0.25f);
                    break;

                case EAction.ACT_JUMP:
                case EAction.ACT_HOLD_JUMP:
                    set_mario_y_vel_based_on_fspeed(42.0f, 0.25f);
                    state.forwardVel *= 0.8f;
                    break;

                case EAction.ACT_WALL_KICK_AIR:
                case EAction.ACT_TOP_OF_POLE_JUMP:
                    set_mario_y_vel_based_on_fspeed(62.0f, 0.0f);
                    if (state.forwardVel < 24.0f) {
                        state.forwardVel = 24.0f;
                    }

                    state.wallKickTimer = 0;
                    break;

                case EAction.ACT_SIDE_FLIP:
                    set_mario_y_vel_based_on_fspeed(62.0f, 0.0f);
                    state.forwardVel = 8.0f;
                    state.faceAngleYaw = state.intendedYaw;
                    break;

                case EAction.ACT_STEEP_JUMP:
                    set_mario_y_vel_based_on_fspeed(42.0f, 0.25f);
                    //state.faceAngle[0] = -0x2000;
                    break;

                case EAction.ACT_LAVA_BOOST:
                    state.vel[1] = 84.0f;
                    if (actionArg == 0) {
                        state.forwardVel = 0.0f;
                    }

                    break;

                case EAction.ACT_DIVE:
                    state.forwardVel = Mathf.Min(state.forwardVel + 15, 48);
                    break;

                case EAction.ACT_LONG_JUMP:
                    set_mario_y_vel_based_on_fspeed(30.0f, 0.0f);
                    marioObj.oMarioLongJumpIsSlow = state.forwardVel <= 16.0f;

                    //! (BLJ's) This properly handles long jumps from getting forward speed with
                    //  too much velocity, but misses backwards longs allowing high negative speeds.
                    if ((state.forwardVel *= 1.5f) > 48.0f) {
                        state.forwardVel = 48.0f;
                    }

                    break;

                case EAction.ACT_SLIDE_KICK:
                    state.vel[1] = 12.0f;
                    if (state.forwardVel < 32.0f) {
                        state.forwardVel = 32.0f;
                    }

                    break;

                case EAction.ACT_JUMP_KICK:
                    state.vel[1] = 20.0f;
                    break;
            }

            state.peakHeight = state.pos[1];
            state.flags |= EFlags.MARIO_UNKNOWN_08;

            return action;
        }

        void set_mario_y_vel_based_on_fspeed(float initialVelY, float multiplier) {
            state.vel[1] = initialVelY + (state.forwardVel * multiplier);

            if (state.squishTimer != 0) {
                state.vel[1] *= 0.5f;
            }
        }

        EAction set_mario_action_submerged(EAction action, int actionArg) {
            _ = actionArg;
            return action;
        }
        EAction set_mario_action_cutscene(EAction action, int actionArg) {
            _ = actionArg;
            return action;
        }

        #endregion
    }
}
