using MyBox;
using UnityEngine;

namespace SuperManual64 {
    [CreateAssetMenu]
    sealed class GameManager : ScriptableObject {
        [SerializeField]
        SceneReference gameScene = new();

        public void LoadGame() {
            gameScene.LoadScene();
        }

        [SerializeField]
        SceneReference menuScene = new();

        public void LoadMenu() {
            menuScene.LoadScene();
        }

        public void Quit() {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }
    }
}
