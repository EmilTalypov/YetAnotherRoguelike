using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour {
    public void StartGame(string nextSceneName) => SceneManager.LoadScene(nextSceneName);
}
