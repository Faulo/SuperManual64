using MyBox;
using UnityEngine;

namespace SuperManual64 {
    [CreateAssetMenu]
    sealed class GameManager : ScriptableObject {
        [SerializeField]
        SceneReference gameScene = new();

        public void LoadGame() {
            gameScene.LoadSceneAsync();
        }

        [SerializeField]
        SceneReference menuScene = new();

        public void LoadMenu() {
            menuScene.LoadSceneAsync();
        }

        public void Quit() {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }
    }
}
