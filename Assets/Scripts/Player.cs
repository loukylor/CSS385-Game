using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Level0");
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (!scene.name.StartsWith("Level"))
        {
            return;
        }

        transform.position = Vector3.zero;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        int levelNumber = int.Parse(scene.name[5..]);
        if (levelNumber >= 1)
        {
            GetComponent<PlayerMovement>().canDoubleJump = true;
        }
        if (levelNumber >= 2)
        {
            GetComponentInChildren<GrapplingHook>().gameObject.SetActive(true); 
        }
    }
}
