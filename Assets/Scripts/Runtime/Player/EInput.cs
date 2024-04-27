using System;

namespace SuperManual64.Player {
    [Flags]
    enum EInput {
        INPUT_NONE = 0,
        INPUT_NONZERO_ANALOG = 0x0001,
        INPUT_A_PRESSED = 0x0002,
        INPUT_OFF_FLOOR = 0x0004,
        INPUT_ABOVE_SLIDE = 0x0008,
        INPUT_FIRST_PERSON = 0x0010,
        INPUT_NEITHER_STICK_NOR_A = 0x0020,
        INPUT_SQUISHED = 0x0040,
        INPUT_A_DOWN = 0x0080,
        INPUT_IN_POISON_GAS = 0x0100,
        INPUT_IN_WATER = 0x0200,
        INPUT_STOMPED = 0x0400,
        INPUT_INTERACT_OBJ_GRABBABLE = 0x0800,
        INPUT_UNKNOWN_12 = 0x1000,
        INPUT_B_PRESSED = 0x2000,
        INPUT_Z_DOWN = 0x4000,
        INPUT_Z_PRESSED = 0x8000,
    }
}