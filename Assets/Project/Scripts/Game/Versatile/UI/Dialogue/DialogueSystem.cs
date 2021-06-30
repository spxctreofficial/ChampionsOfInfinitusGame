using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class DialogueSystem : MonoBehaviour {
	private Queue<Dialogue> dialogues = new Queue<Dialogue>();
	private Dialogue currentDialogue;
	private UnityAction endOfConversationAction;

	[SerializeField]
	private TMP_Text characterName;
	[SerializeField]
	private Image characterAvatar;
	[SerializeField]
	private TMP_Text dialogueBox;
	[SerializeField]
	private Button continueButton, skipButton;
	[SerializeField]
	private AudioClip beep;

	public static DialogueSystem Create(DialogueSession dialogueSession, Vector2 vector2, UnityAction endOfConversationAction = null) {
		var dialogueSystemPrefab = MainMenuController.instance == null ? GameController.instance.dialogueSystemPrefab : MainMenuController.instance.dialogueSystemPrefab;

		float bottomOfScreen = -540 - (dialogueSystemPrefab.GetComponent<RectTransform>().rect.height / 2);
		var dialogueSystem = Instantiate(dialogueSystemPrefab, new Vector2(vector2.x, bottomOfScreen), Quaternion.identity).GetComponent<DialogueSystem>();

		foreach (var dialogue in dialogueSession.dialogues) dialogueSystem.dialogues.Enqueue(dialogue);
		if (endOfConversationAction != null) dialogueSystem.endOfConversationAction = endOfConversationAction;

		LeanTween.move(dialogueSystem.GetComponent<RectTransform>(), vector2, 0.5f).setEaseOutQuad();
		dialogueSystem.LoadNextSentence();
		return dialogueSystem;
	}

	public void OnContinueButtonClick() {
		if (dialogues.Count == 0) {
			Debug.Log("End of conversation.");
			LeanTween.move(GetComponent<RectTransform>(), new Vector2(GetComponent<RectTransform>().localPosition.x, -540 - (GetComponent<RectTransform>().rect.height / 2)), 0.5f).setEaseOutQuad().setOnComplete(() => {
				if (endOfConversationAction != null) endOfConversationAction.Invoke();
			});
			return;
		}

		LoadNextSentence();
	}
	public void OnSkipButtonClick() {
		skipButton.gameObject.SetActive(false);
		continueButton.gameObject.SetActive(true);

		StopAllCoroutines();
		dialogueBox.text = currentDialogue.sentence;
		AudioController.instance.Play(beep, false, 0.5f);
	}

	private void LoadNextSentence() {
		continueButton.gameObject.SetActive(false);
		skipButton.gameObject.SetActive(true);

		currentDialogue = dialogues.Dequeue();

		characterName.text = currentDialogue.speakingChampion.championName;
		characterAvatar.sprite = currentDialogue.speakingChampion.avatar;
		dialogueBox.text = "";
		StartCoroutine(TypeSentence(currentDialogue.sentence));
	}
	private IEnumerator TypeSentence(string sentence) {
		foreach (char c in sentence.ToCharArray()) {
			var waitTime = char.IsWhiteSpace(c) || char.IsPunctuation(c) ? Random.Range(0.1f, 0.16f) : Random.Range(0.04f, 0.07f);

			dialogueBox.text += c;
			AudioController.instance.Play(beep, false, 0.5f);
			yield return new WaitForSeconds(waitTime);
		}
		continueButton.gameObject.SetActive(true);
		skipButton.gameObject.SetActive(false);
	}
}
