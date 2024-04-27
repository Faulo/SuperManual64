namespace SuperManual64.Player {
    record LandingAction(int numFrames, int unk02, EAction verySteepAction, EAction endAction, EAction aPressedAction, EAction offFloorAction, EAction slideAction) {
        public static readonly LandingAction sJumpLandAction
            = new(4, 5, EAction.ACT_FREEFALL, EAction.ACT_JUMP_LAND_STOP, EAction.ACT_DOUBLE_JUMP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sFreefallLandAction
            = new(4, 5, EAction.ACT_FREEFALL, EAction.ACT_FREEFALL_LAND_STOP, EAction.ACT_DOUBLE_JUMP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sSideFlipLandAction
            = new(4, 5, EAction.ACT_FREEFALL, EAction.ACT_SIDE_FLIP_LAND_STOP, EAction.ACT_DOUBLE_JUMP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sHoldJumpLandAction
            = new(4, 5, EAction.ACT_HOLD_FREEFALL, EAction.ACT_HOLD_JUMP_LAND_STOP, EAction.ACT_HOLD_JUMP, EAction.ACT_HOLD_FREEFALL, EAction.ACT_HOLD_BEGIN_SLIDING);

        public static readonly LandingAction sHoldFreefallLandAction
            = new(4, 5, EAction.ACT_HOLD_FREEFALL, EAction.ACT_HOLD_FREEFALL_LAND_STOP, EAction.ACT_HOLD_JUMP, EAction.ACT_HOLD_FREEFALL, EAction.ACT_HOLD_BEGIN_SLIDING);

        public static readonly LandingAction sLongJumpLandAction
            = new(6, 5, EAction.ACT_FREEFALL, EAction.ACT_LONG_JUMP_LAND_STOP, EAction.ACT_LONG_JUMP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sDoubleJumpLandAction
            = new(4, 5, EAction.ACT_FREEFALL, EAction.ACT_DOUBLE_JUMP_LAND_STOP, EAction.ACT_JUMP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sTripleJumpLandAction
            = new(4, 0, EAction.ACT_FREEFALL, EAction.ACT_TRIPLE_JUMP_LAND_STOP, EAction.ACT_UNINITIALIZED, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);

        public static readonly LandingAction sBackflipLandAction
            = new(4, 0, EAction.ACT_FREEFALL, EAction.ACT_BACKFLIP_LAND_STOP, EAction.ACT_BACKFLIP, EAction.ACT_FREEFALL, EAction.ACT_BEGIN_SLIDING);
    }
}
