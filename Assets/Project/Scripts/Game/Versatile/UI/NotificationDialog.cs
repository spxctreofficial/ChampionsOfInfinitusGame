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
		GameObject notificationDialogPrefab = PrefabManager.instance.notificationDialogPrefab;
		NotificationDialog notificationDialog = Instantiate(notificationDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<NotificationDialog>();
		
		notificationDialog.Setup(title, description);
		notificationDialog.Show();

		LeanTween.delayedCall(duration, () => {
			notificationDialog.Hide();
		});

		return notificationDialog;
	}
	public NotificationDialog SetAfterShow(UnityAction unityAction) {
		afterShowAction = unityAction;
		return this;
	}
	
	private void Setup(string title, string description) {
		titleText.text = title;
		descriptionText.text = description;
	}

	private void Show() {
		GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.one, 0.25f).setEaseInOutQuad().setOnComplete(() => afterShowAction.Invoke());
	}
	private void Hide() {
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad().setDestroyOnComplete(true);
	}
}
