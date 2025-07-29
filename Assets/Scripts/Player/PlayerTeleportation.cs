using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PlayerTeleportation : MonoBehaviour {
    [SerializeField] private List<string> _levelNames;

    private int _currentLevel = 0;
    private bool _isSwitching = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("NextLevelTrigger") && !_isSwitching) {
            StartCoroutine(NextLevelCoroutine());
        }
    }

    private IEnumerator NextLevelCoroutine() {
        _isSwitching = true;

        string levelName = _levelNames[_currentLevel + 1];

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(GameObject.Find("Canvas"));

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelName);

        yield return new WaitUntil(() => loadOp.isDone);

        Scene scene = SceneManager.GetSceneByName(levelName);
        SceneManager.SetActiveScene(scene);

        ConfigureNewSceneCamera();

        _currentLevel += 1;
        _isSwitching = false;
    }

    private void ConfigureNewSceneCamera() {
        CinemachineCamera cam = FindAnyObjectByType<CinemachineCamera>();

        if (cam != null) {
            cam.Follow = transform;
            return;
        }
    }
}
