using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Session", menuName = "LandOfHeroesGame/Game/New Dialogue Session")]
public class DialogueSession : ScriptableObject {
	public Dialogue[] dialogues;
}

[Serializable]
public class Dialogue {
	public Champion speakingChampion;
	[TextArea(3, 10)]
	public string sentence;
}
