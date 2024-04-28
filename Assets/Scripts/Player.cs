using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public event Action OnDeath;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

#if !UNITY_EDITOR
        SceneManager.LoadScene("Level0");
#endif
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Temp code to "respawn" player
        if (transform.position.y < -5)
        {
            transform.position = new Vector3(0, 2, 0);
            OnDeath?.Invoke();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (!scene.name.StartsWith("Level"))
        {
            return;
        }
        OnDeath?.Invoke();

        transform.position = Vector3.zero;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        int levelNumber = int.Parse(scene.name[5..]);
        if (levelNumber >= 1)
        {
            GetComponent<PlayerMovement>().canDoubleJump = true;
        }
        if (levelNumber >= 2)
        {
            GetComponentInChildren<GrapplingHook>(true).gameObject.SetActive(true); 
        }
    }
}
