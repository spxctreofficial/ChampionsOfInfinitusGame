using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 defaultScale;
    bool isScalingUp, isScalingDown;

    void Start()
    {
        defaultScale = new Vector3(1f, 1f, 1f);
    }

    [HideInInspector]
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ScaleUp(new Vector3(1.1f, 1.1f, 1.1f)));
    }
    [HideInInspector]
    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ScaleDown(defaultScale));
    }
    public IEnumerator ScaleUp(Vector3 targetScale)
    {
        isScalingUp = true;
        isScalingDown = false;

        float scaleDuration = 1;
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            if (isScalingDown) break;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            yield return null;
        }
    }
    public IEnumerator ScaleDown(Vector3 targetScale)
    {
        isScalingUp = false;
        isScalingDown = true;

        float scaleDuration = 1;
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            if (isScalingUp) break;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            yield return null;
        }
    }
}
