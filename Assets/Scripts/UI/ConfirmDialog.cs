using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour {
    
    [SerializeField]
    private TMP_Text titleText;
    [SerializeField]
    private TMP_Text descriptionText;
    
    [SerializeField]
    private Button negativeButton;
    [SerializeField]
    private Button positiveButton;
    
    public void Setup(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, bool tweenToView = true, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM") {
        // Title & Description
        titleText.text = title;
        descriptionText.text = description;

        // Negative & Positive Button Texts
        negativeButton.transform.GetChild(0).GetComponent<TMP_Text>().text = negativeButtonText;
        positiveButton.transform.GetChild(0).GetComponent<TMP_Text>().text = positiveButtonText;
        
        // Negative & Positive Button Actions
        SetNegativeButton(negativeButtonAction);
        SetPositiveButton(positiveButtonAction);

        switch (tweenToView) {
            case true:
                Show();
                break;
            case false:
                break;
        }
    }

    public void SetNegativeButton(UnityAction call) {
        negativeButton.onClick.RemoveAllListeners();
        negativeButton.onClick.AddListener(call);
    }
    public void SetPositiveButton(UnityAction call) {
        positiveButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(call);
    }

    public void Show() {
        transform.localScale = Vector2.zero;
        LeanTween.scale(GetComponent<RectTransform>(), Vector2.one, 0.25f).setEaseInOutQuad();
    }
    public void Hide() {
        LeanTween.scale(GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad().setDestroyOnComplete(true);
    }
}
