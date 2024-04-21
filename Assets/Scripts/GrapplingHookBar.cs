using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class GrapplingHookBar : MonoBehaviour
{
    public RectTransform innerBar;
    public float maxWidth;
    public float dissapearTime = 0.75f;
    public GrapplingHook hook;

    private CanvasGroup group;
    private Coroutine dissapearCoroutine;
    private float lastHookPercent;

    private void Start()
    {
        group = GetComponent<CanvasGroup>();
        hook.OnHookTimeChange += OnHookTimeChange;
    }

    private void OnHookTimeChange(float percent)
    {
        if (lastHookPercent <= percent)
        {
            // if getting bigger or staying the same, start dissapear timer
            dissapearCoroutine ??= StartCoroutine(Dissapear());
        }
        else
        {
            if (dissapearCoroutine != null)
            {
                StopCoroutine(dissapearCoroutine);
                dissapearCoroutine = null;
            }
            group.alpha = 1;
            innerBar.sizeDelta = new Vector2(percent * maxWidth, innerBar.sizeDelta.y);
        }

        lastHookPercent = percent;
    }

    private IEnumerator Dissapear()
    {
        while (group.alpha >= 0.01)
        {
            group.alpha -= Time.deltaTime / dissapearTime;
            yield return null;
        }

        group.alpha = 0;
    }
}