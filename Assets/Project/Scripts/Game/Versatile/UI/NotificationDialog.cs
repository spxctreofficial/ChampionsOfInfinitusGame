using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class NotificationDialog : MonoBehaviour {
	
	[SerializeField]
	private TMP_Text titleText;
	[SerializeField]
	private TMP_Text descriptionText;

	private UnityAction afterShowAction;

	public static NotificationDialog Create(string title, string description, float duration = 3f) {
		var notificationDialogPrefab = MainMenuController.instance == null ? GameController.instance.notificationDialogPrefab : MainMenuController.instance.notificationDialogPrefab;
		var notificationDialog = Instantiate(notificationDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<NotificationDialog>();
		
		notificationDialog.Setup(title, description);
		notificationDialog.Show();

		LeanTween.delayedCall(duration, () => {
			notificationDialog.Hide();
		});

		return notificationDialog;
	}

	public void Setup(string title, string description) {
		titleText.text = title;
		descriptionText.text = description;
	}

	public NotificationDialog SetAfterShow(UnityAction unityAction) {
		afterShowAction = unityAction;
		return this;
	}

	public void Show() {
		GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.one, 0.25f).setEaseInOutQuad().setOnComplete(() => afterShowAction.Invoke());
	}
	public void SlideFromBottom(Vector2 vector2) {
		GetComponent<RectTransform>().localPosition = new Vector2(0, (float)-Screen.height / 2 - 500);
		LeanTween.move(GetComponent<RectTransform>(), vector2, 0.25f).setEaseInOutQuad();
	}
	public void Hide() {
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad().setDestroyOnComplete(true);
	}
}
