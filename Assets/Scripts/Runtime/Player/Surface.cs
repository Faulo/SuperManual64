using UnityEngine;

namespace SuperManual64.Player {
    [CreateAssetMenu]
    sealed class Surface : ScriptableObject {
        [SerializeField]
        public Vector3 normal = Vector3.up;
        [SerializeField]
        public ESurface type;

        public ESurfaceClass surfaceClass => type switch {
            ESurface.SURFACE_NOT_SLIPPERY => ESurfaceClass.SURFACE_CLASS_NOT_SLIPPERY,
            ESurface.SURFACE_HARD_NOT_SLIPPERY => ESurfaceClass.SURFACE_CLASS_NOT_SLIPPERY,
            ESurface.SURFACE_SWITCH => ESurfaceClass.SURFACE_CLASS_NOT_SLIPPERY,

            ESurface.SURFACE_SLIPPERY => ESurfaceClass.SURFACE_CLASS_SLIPPERY,
            ESurface.SURFACE_NOISE_SLIPPERY => ESurfaceClass.SURFACE_CLASS_SLIPPERY,
            ESurface.SURFACE_HARD_SLIPPERY => ESurfaceClass.SURFACE_CLASS_SLIPPERY,
            ESurface.SURFACE_NO_CAM_COL_SLIPPERY => ESurfaceClass.SURFACE_CLASS_SLIPPERY,

            ESurface.SURFACE_VERY_SLIPPERY => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_ICE => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_HARD_VERY_SLIPPERY => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_NOISE_VERY_SLIPPERY_73 => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_NOISE_VERY_SLIPPERY_74 => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_NOISE_VERY_SLIPPERY => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,
            ESurface.SURFACE_NO_CAM_COL_VERY_SLIPPERY => ESurfaceClass.SURFACE_CLASS_VERY_SLIPPERY,

            _ => ESurfaceClass.SURFACE_CLASS_DEFAULT,
        };

        public bool isSlope {
            get {
                return false;
            }
        }

        public bool isSteep {
            get {
                return false;
            }
        }

        public bool isSlippery {
            get {
                float normY = type switch {
                    ESurface.SURFACE_VERY_SLIPPERY => 0.9848077f,//~cos(10 deg)
                    ESurface.SURFACE_SLIPPERY => 0.9396926f,//~cos(20 deg)
                    ESurface.SURFACE_NOT_SLIPPERY => 0.0f,
                    _ => 0.7880108f,//~cos(38 deg)
                };
                return normal.y <= normY;
            }
        }
    }
}