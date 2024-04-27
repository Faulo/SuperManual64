using UnityEngine;
using UnityEngine.Rendering;

namespace SuperManual64.UI {
    sealed class FaceCamera : MonoBehaviour {
        void OnEnable() {
            RenderPipelineManager.beginCameraRendering += UpdateTransform;
        }
        void OnDisable() {
            RenderPipelineManager.beginCameraRendering -= UpdateTransform;
        }

        void UpdateTransform(ScriptableRenderContext context, Camera camera) {
            transform.rotation = camera.transform.rotation;
        }
    }
}
