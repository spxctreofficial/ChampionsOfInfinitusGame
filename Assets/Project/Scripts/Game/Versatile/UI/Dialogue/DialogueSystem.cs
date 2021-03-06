using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
	private Queue<Dialogue> dialogues = new Queue<Dialogue>();
	private Dialogue currentDialogue;
	private UnityAction endOfConversationAction;
	private bool endOfConversationAnimation;

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

	public static DialogueSystem Create(DialogueSession dialogueSession, Vector2 vector2, UnityAction endOfConversationAction = null, bool startOfConversationAnimation = true, bool endOfConversationAnimation = true)
	{
		GameObject dialogueSystemPrefab = PrefabManager.instance.dialogueSystemPrefab;

		float bottomOfScreen = -540 - (dialogueSystemPrefab.GetComponent<RectTransform>().rect.height / 2);
		DialogueSystem dialogueSystem = Instantiate(dialogueSystemPrefab, new Vector2(vector2.x, bottomOfScreen), Quaternion.identity).GetComponent<DialogueSystem>();

		foreach (Dialogue dialogue in dialogueSession.dialogues) dialogueSystem.dialogues.Enqueue(dialogue);
		if (endOfConversationAction != null) dialogueSystem.endOfConversationAction = endOfConversationAction;
		dialogueSystem.endOfConversationAnimation = endOfConversationAnimation;
		Debug.Log(dialogueSystem.endOfConversationAction);

		if (startOfConversationAnimation)
			LeanTween.move(dialogueSystem.GetComponent<RectTransform>(), vector2, 0.75f).setEaseInOutQuart();
		else
			dialogueSystem.GetComponent<RectTransform>().localPosition = vector2;

		dialogueSystem.LoadNextSentence();
		return dialogueSystem;
	}

	public void OnContinueButtonClick()
	{
		LoadNextSentence();
	}
	public void OnSkipButtonClick()
	{
		skipButton.gameObject.SetActive(false);
		continueButton.gameObject.SetActive(true);

		StopAllCoroutines();
		dialogueBox.text = currentDialogue.sentence;
		AudioManager.instance.Play(beep, false, 0.5f);
	}

	private void LoadNextSentence()
	{
		if (dialogues.Count == 0)
		{
			Debug.Log("End of conversation.");
			Vector2 bottomOfScreen = new Vector2(GetComponent<RectTransform>().localPosition.x, -540 - GetComponent<RectTransform>().rect.height / 2);

			if (endOfConversationAnimation)
			{
				LeanTween.move(GetComponent<RectTransform>(), new Vector2(GetComponent<RectTransform>().localPosition.x, -540 - GetComponent<RectTransform>().rect.height / 2), 0.75f).setEaseInOutQuart().setOnComplete(() =>
				{
					if (endOfConversationAction is {}) endOfConversationAction.Invoke();
					Destroy(gameObject, 1f);
				});
			}
			else
			{
				if (endOfConversationAction is {}) endOfConversationAction.Invoke();
				GetComponent<RectTransform>().localPosition = bottomOfScreen;
				Destroy(gameObject, 1f);
			}
			return;
		}

		currentDialogue = dialogues.Dequeue();
		continueButton.gameObject.SetActive(false);
		if (!currentDialogue.continueWithoutInput) skipButton.gameObject.SetActive(true);

		characterName.text = currentDialogue.speakingChampion.championName;
		characterAvatar.sprite = currentDialogue.speakingChampion.avatar;
		dialogueBox.text = string.Empty;
		StartCoroutine(TypeSentence());
	}
	private IEnumerator TypeSentence()
	{
		foreach (char c in currentDialogue.sentence)
		{
			float waitTime;
			switch (currentDialogue.caretBehaviour)
			{
				case Dialogue.CaretBehaviour.Natural:
					waitTime = char.IsWhiteSpace(c) || char.IsPunctuation(c) ? Random.Range(0.08f, 0.11f) : Random.Range(0.05f, 0.07f);
					break;
				case Dialogue.CaretBehaviour.LongPause:
					switch (c)
					{
						case '.':
						case '!':
						case '?':
							waitTime = Random.Range(1f, 1.5f);
							break;
						case ',':
							waitTime = Random.Range(0.08f, 0.11f);
							break;
						default:
							waitTime = Random.Range(0.05f, 0.07f);
							break;
					}
					break;
				case Dialogue.CaretBehaviour.Fast:
					waitTime = char.IsWhiteSpace(c) || char.IsPunctuation(c) ? Random.Range(0.04f, 0.055f) : Random.Range(0.025f, 0.035f);
					break;
				default:
					waitTime = 0.05f;
					break;
			}

			dialogueBox.text += c;
			AudioManager.instance.Play(beep, false, 0.5f);
			yield return new WaitForSeconds(waitTime);
		}

		if (currentDialogue.continueWithoutInput)
		{
			LoadNextSentence();
		}
		else
		{
			continueButton.gameObject.SetActive(true);
			skipButton.gameObject.SetActive(false);
		}
	}
}
