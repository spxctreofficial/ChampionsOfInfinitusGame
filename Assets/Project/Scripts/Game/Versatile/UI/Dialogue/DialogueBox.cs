using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBox : MonoBehaviour {
	public Champion speakingChampion;

	[SerializeField]
	private TMP_Text characterName;
	[SerializeField]
	private Image characterAvatar;
	[SerializeField]
	private TMP_Text dialogueBox;
	[SerializeField]
	private Button nextLineButton;
}
