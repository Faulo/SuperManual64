using System;
using UnityEngine;

namespace SuperManual64.Player {
    [Serializable]
    struct AnimInfo {
        [SerializeField]
        public EAnim animID;

        public bool isAnimAtEnd => true;
    }
}
