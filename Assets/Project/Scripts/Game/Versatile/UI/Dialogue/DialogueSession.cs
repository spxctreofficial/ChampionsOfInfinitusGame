using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Session", menuName = "LandOfHeroesGame/Game/New Dialogue Session")]
public class DialogueSession : ScriptableObject {
	public Dialogue[] dialogues;
}

[Serializable]
public class Dialogue {
	public enum CaretBehaviour { Unfiltered, Natural, LongPause, Fast }
	
	public Champion speakingChampion;
	[TextArea(3, 10)]
	public string sentence;
	public CaretBehaviour caretBehaviour;
	public bool continueWithoutInput;
}
