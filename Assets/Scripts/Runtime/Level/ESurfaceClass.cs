using System;

namespace SuperManual64.Level {
    [Flags]
    enum ESurfaceClass {
        SURFACE_CLASS_DEFAULT = 0x0000,
        SURFACE_CLASS_VERY_SLIPPERY = 0x0013,
        SURFACE_CLASS_SLIPPERY = 0x0014,
        SURFACE_CLASS_NOT_SLIPPERY = 0x0015,

        SURFACE_FLAG_DYNAMIC = 1 << 0,
        SURFACE_FLAG_NO_CAM_COLLISION = 1 << 1,
        SURFACE_FLAG_X_PROJECTION = 1 << 3,
    }
}
