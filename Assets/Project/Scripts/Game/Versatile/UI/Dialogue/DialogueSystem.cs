using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class DialogueSystem : MonoBehaviour {
	private Queue<Dialogue> dialogues = new Queue<Dialogue>();
	private UnityAction endOfConversationAction;

	[SerializeField]
	private TMP_Text characterName;
	[SerializeField]
	private Image characterAvatar;
	[SerializeField]
	private TMP_Text dialogueBox;
	[SerializeField]
	private Button nextLineButton;
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

	public void OnButtonClick() {
		if (dialogues.Count == 0) {
			Debug.Log("End of conversation.");
			LeanTween.move(GetComponent<RectTransform>(), new Vector2(GetComponent<RectTransform>().localPosition.x, -540 - (GetComponent<RectTransform>().rect.height / 2)), 0.5f).setEaseOutQuad().setOnComplete(() => {
				if (endOfConversationAction != null) endOfConversationAction.Invoke();
			});
			return;
		}

		LoadNextSentence();
	}

	private void LoadNextSentence() {
		var dialogue = dialogues.Dequeue();

		characterName.text = dialogue.speakingChampion.championName;
		characterAvatar.sprite = dialogue.speakingChampion.avatar;
		StartCoroutine(TypeSentence(dialogue.sentence));
	}
	private IEnumerator TypeSentence(string sentence) {
		dialogueBox.text = "";
		foreach (char c in sentence.ToCharArray()) {
			var waitTime = char.IsWhiteSpace(c) ? Random.Range(0.1f, 0.16f) : Random.Range(0.06f, 0.09f);

			dialogueBox.text += c;
			AudioController.instance.Play(beep, false, 0.5f);
			yield return new WaitForSeconds(waitTime);
		}
	}
}
