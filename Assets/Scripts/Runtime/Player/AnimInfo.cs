using System;
using UnityEngine;

namespace SuperManual64.Player {
    [Serializable]
    struct AnimInfo {
        [SerializeField]
        public EAnim animID;
        [SerializeField]
        public int animYTrans;
        [SerializeField]
        public int animFrame;
        [SerializeField]
        public int animTimer;
        [SerializeField]
        public int animFrameAccelAssist;
        [SerializeField]
        public int animAccel;

        public bool isAnimAtEnd => animFrame >= 4;
    }
}
