using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioObject : MonoBehaviour {
        [SerializeField]
        public int oMarioWalkingPitch;
        [SerializeField]
        public int oMarioSteepJumpYaw;
        [SerializeField]
        public AnimInfo animInfo = new();

        public void SetAnimation(EAnim anim) {
            animInfo.animID = anim;
        }
    }
}
