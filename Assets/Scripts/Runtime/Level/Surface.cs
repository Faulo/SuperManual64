using Slothsoft.UnityExtensions;
using UnityEngine;

namespace SuperManual64.Level {
    [CreateAssetMenu]
    sealed class Surface : ScriptableObject {
        [SerializeField, Expandable]
        public PhysicMaterial material;
        [SerializeField]
        public ESurface type;
    }
}