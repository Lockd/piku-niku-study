using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool isPlaying = true;
    public static GameController instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            reloadScene();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - 0.1f, 0.1f, 10f);
            Debug.Log("Time scale: " + Time.timeScale);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Time.timeScale = 1f;
            Debug.Log("Time scale reset to: " + Time.timeScale);
        }
    }

    public void reloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
