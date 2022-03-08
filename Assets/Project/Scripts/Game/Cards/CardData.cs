using UnityEngine;

public enum CardColor
{
	Light,
	Dark,
	Super,
	NoPref
}

[CreateAssetMenu(fileName = "Card", menuName = "ChampionsOfInfinitusGame/Game/New Card")]
public class CardData : ScriptableObject
{
	[Header("Card Information")]
	public string cardName;
	[TextArea(5, 20)]
	public string cardDescription;

	public string cornerText;
	public Sprite cardFront, cardBack, cardIcon;

	[Header("Functional Variables")]
	public CardFunctions cardFunctions;
	public CardColor cardColor;
	[Range(0, 10)]
	public int staminaRequirement;
	[Range(0, 100)]
	[Tooltip("How important a bot deems this card. 0 is highest priority, while 100 is lowest priority.")]
	public int cardImportanceFactor;

	[System.Serializable]
	public struct CardFunctions
	{
		public string primaryFunction, secondaryFunction, tertiaryFunction, quaternaryFunction;
	}
}
