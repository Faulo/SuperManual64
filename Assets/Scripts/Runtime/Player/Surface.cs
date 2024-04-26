
using UnityEngine;

namespace SuperManual64.Player {
    [CreateAssetMenu]
    sealed class Surface : ScriptableObject {
        [SerializeField]
        public Vector3 normal = Vector3.up;
    }
}
