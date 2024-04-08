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
            case TriggerAction.LoadScene:
                SceneManager.LoadScene(target, LoadSceneMode.Single);
                break;
        }
    }
}