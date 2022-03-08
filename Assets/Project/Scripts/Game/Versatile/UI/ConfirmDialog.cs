using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ConfirmDialog : MonoBehaviour
{

	public static ConfirmDialog instance;

	[SerializeField]
	private TMP_Text titleText;
	[SerializeField]
	private TMP_Text descriptionText;

	[SerializeField]
	private Button negativeButton;
	[SerializeField]
	private Button positiveButton;

	[SerializeField] private CanvasGroup canvasGroup;

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
	public static ConfirmDialog CreateNew(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true)
	{
		GameObject confirmDialogPrefab = PrefabManager.instance.confirmDialogPrefab;
		ConfirmDialog confirmDialog = Instantiate(confirmDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<ConfirmDialog>();

		confirmDialog.Setup(title, description, negativeButtonAction, positiveButtonAction, negativeButtonText, positiveButtonText, tweenToView);
		instance = confirmDialog;
		return confirmDialog;
	}
	public static ConfirmDialog CreateNew_Mini(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true)
	{
		GameObject confirmDialogPrefab = PrefabManager.instance.miniConfirmDialogPrefab;
		ConfirmDialog confirmDialog = Instantiate(confirmDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<ConfirmDialog>();

		confirmDialog.Setup(title, description, negativeButtonAction, positiveButtonAction, negativeButtonText, positiveButtonText, tweenToView);
		instance = confirmDialog;
		return confirmDialog;
	}

	public void Setup(string title, string description, UnityAction negativeButtonAction, UnityAction positiveButtonAction, string negativeButtonText = "CANCEL", string positiveButtonText = "CONFIRM", bool tweenToView = true)
	{
		// Title & Description
		titleText.text = title;
		descriptionText.text = description;

		// Negative & Positive Button Texts
		negativeButton.transform.GetChild(0).GetComponent<TMP_Text>().text = negativeButtonText;
		positiveButton.transform.GetChild(0).GetComponent<TMP_Text>().text = positiveButtonText;

		// Negative & Positive Button Actions
		SetNegativeButton(negativeButtonAction);
		SetPositiveButton(positiveButtonAction);

		switch (tweenToView)
		{
			case true:
				Show();
				break;
			case false:
				break;
		}
	}
	private void SetNegativeButton(UnityAction call)
	{
		negativeButton.onClick.RemoveAllListeners();
		negativeButton.onClick.AddListener(call);
	}
	private void SetPositiveButton(UnityAction call)
	{
		positiveButton.onClick.RemoveAllListeners();
		positiveButton.onClick.AddListener(call);
	}

	public void Show()
	{
		transform.localScale = new Vector2(0.8f, 0.8f);
		canvasGroup.alpha = 0f;
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.one, 0.3f).setEaseInOutQuart();
		LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEaseInOutQuart();
	}
	public void Hide()
	{
		LeanTween.scale(GetComponent<RectTransform>(), new Vector2(0.8f, 0.8f), 0.3f).setEaseInOutQuart().setDestroyOnComplete(true);
		LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setEaseInOutQuart();
		if (this == instance) instance = null;
	}
}
