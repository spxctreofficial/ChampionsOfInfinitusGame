using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 cachedScale;
    bool isScalingUp, isScalingDown;

    void Start()
    {
        cachedScale = transform.localScale;
    }

    [HideInInspector]
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ScaleUp());
    }
    [HideInInspector]
    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ScaleDown());
    }
    IEnumerator ScaleUp()
    {
        isScalingUp = true;
        isScalingDown = false;

        float scaleDuration = 1;
        Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            if (isScalingDown) break;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            yield return null;
        }
    }
    IEnumerator ScaleDown()
    {
        isScalingUp = false;
        isScalingDown = true;

        float scaleDuration = 1;
        Vector3 targetScale = cachedScale;
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            if (isScalingUp) break;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            yield return null;
        }
    }
}
