using UnityEngine;

[ExecuteInEditMode]
public class Win : MonoBehaviour
{
    private bool isWin = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isWin)
        {
            isWin = true;
        }
    }

    private void OnGUI()
    {
        if (isWin)
        {
            float labelWidth = Screen.width / 3;
            float labelHeight = Screen.height / 3;
            // Using GUI because I'm too lazy to setup a proper UI
            GUI.Label(
                new Rect(
                    (Screen.width / 2) - (labelWidth / 2), 
                    (Screen.height / 2) - (labelHeight / 2), 
                    labelWidth,
                    labelHeight
                ), 
                "WIN!", 
                new GUIStyle() 
                { 
                    fontSize = 100, 
                    alignment = TextAnchor.MiddleCenter 
                }
            );
        }
    }
}