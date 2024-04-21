using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class Trigger : MonoBehaviour
{
    public TriggerAction action;
    public string target;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        switch (action)
        {
            case TriggerAction.LoadSceneSingle:
                SceneManager.LoadScene(target, LoadSceneMode.Single);
                break;
            case TriggerAction.LoadSceneAdditive:
                SceneManager.LoadScene(target, LoadSceneMode.Additive);
                break;
        }
    }
}