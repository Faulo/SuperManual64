namespace SuperManual64.Player {
    enum EAirStep {
        AIR_STEP_CHECK_LEDGE_GRAB = 0x00000001,
        AIR_STEP_CHECK_HANG = 0x00000002,

        AIR_STEP_NONE = 0,
        AIR_STEP_LANDED = 1,
        AIR_STEP_HIT_WALL = 2,
        AIR_STEP_GRABBED_LEDGE = 3,
        AIR_STEP_GRABBED_CEILING = 4,
        AIR_STEP_HIT_LAVA_WALL = 6,
    }
}