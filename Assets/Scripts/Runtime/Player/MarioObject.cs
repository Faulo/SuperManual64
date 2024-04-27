using UnityEngine;

namespace SuperManual64.Player {
    sealed class MarioObject : MonoBehaviour {
        [SerializeField]
        public int oMarioWalkingPitch;
        [SerializeField]
        public float oMarioSteepJumpYaw;
        [SerializeField]
        public AnimInfo animInfo = new();
        [SerializeField]
        public bool oMarioLongJumpIsSlow;

        public void SetAnimation(EAnim anim) {
            if (animInfo.animID != anim) {
                animInfo.animID = anim;
                animInfo.animFrame = 0;
            }
        }

        public void AdvanceAnimation() {
            animInfo.animFrame++;
        }
    }
}
