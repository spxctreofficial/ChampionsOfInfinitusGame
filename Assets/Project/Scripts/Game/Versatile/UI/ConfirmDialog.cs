using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ConfirmDialog : MonoBehaviour {

    public static ConfirmDialog instance;
    
    [SerializeField]
    private TMP_Text titleText;
    [SerializeField]
    private TMP_Text descriptionText;
    
    [SerializeField]
    private Button negativeButton;
    [SerializeField]
    private Button positiveButton;

	/// <summary>
	/// Create a new Confirm Dialog with these properties pre-set.
	/// Also sets this Confirm Dialog as the static instance of the Confirm Dialog.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="description"></param>
	/// <param name="negativeButtonAction"></param>
	/// <param name="positiveButtonAction"></param>
	/// <param name="tweenToView"></param>
	/// <param name="negativeButtonText"></param>
	/// <param name="positiveButtonText"></param>
	/// <returns></returns>
	public static ConfirmDialog CreateNew(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true) {
        GameObject confirmDialogPrefab = MainMenuController.instance == null ? GameController.instance.confirmDialogPrefab : MainMenuController.instance.confirmDialogPrefab;
        ConfirmDialog confirmDialog = Instantiate(confirmDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<ConfirmDialog>();
        
        confirmDialog.Setup(title, description, negativeButtonAction, positiveButtonAction, negativeButtonText, positiveButtonText, tweenToView);
        instance = confirmDialog;
        return confirmDialog;
    }
    public static ConfirmDialog CreateNew_Mini(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true) {
        GameObject confirmDialogPrefab = MainMenuController.instance == null ? GameController.instance.miniConfirmDialogPrefab : MainMenuController.instance.miniConfirmDialogPrefab;
        ConfirmDialog confirmDialog = Instantiate(confirmDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<ConfirmDialog>();
        
        confirmDialog.Setup(title, description, negativeButtonAction, positiveButtonAction, negativeButtonText, positiveButtonText, tweenToView);
        instance = confirmDialog;
        return confirmDialog;
    }
    
    /// <summary>
    /// Sets up a Confirm Dialog's properties.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="negativeButtonAction"></param>
    /// <param name="positiveButtonAction"></param>
    /// <param name="tweenToView"></param>
    /// <param name="negativeButtonText"></param>
    /// <param name="positiveButtonText"></param>
    public void Setup(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true) {
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
    /// <summary>
    /// Set an UnityAction to be triggered by the Negative Button (left).
    /// </summary>
    /// <param name="call"></param>
    private void SetNegativeButton(UnityAction call) {
        negativeButton.onClick.RemoveAllListeners();
        negativeButton.onClick.AddListener(call);
    }
    /// <summary>
    /// Set an UnityAction to be triggered by the Positive Button (right).
    /// </summary>
    /// <param name="call"></param>
    private void SetPositiveButton(UnityAction call) {
        positiveButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(call);
    }

    /// <summary>
    /// Shows this Confirm Dialog.
    /// </summary>
    public void Show() {
        transform.localScale = Vector2.zero;
        LeanTween.scale(GetComponent<RectTransform>(), Vector2.one, 0.25f).setEaseInOutQuad();
    }
    /// <summary>
    /// Hides this Confirm Dialog.
    /// </summary>
    public void Hide() {
        LeanTween.scale(GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad().setDestroyOnComplete(true);
        if (this == instance) instance = null;
    }
}
