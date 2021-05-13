using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum DamageType { Melee, Ranged, Fire, Lightning, Shadow, Unblockable }

public class ChampionController : MonoBehaviour, IPointerClickHandler
{
	public Champion champion;
	[HideInInspector]
	public Hand hand;
	[HideInInspector]
	public AbilityPanel abilityPanel;

	[HideInInspector]
	public new string name;
	[HideInInspector]
	public Sprite avatar;
	[HideInInspector]
	public Champion.Gender gender;
	[HideInInspector]
	public Champion.Faction faction;
	[HideInInspector]
	public Champion.Race race;

	[HideInInspector]
	public Button championButton;
	[HideInInspector]
	public TMP_Text nameText, healthText, cardsText;

	[HideInInspector]
	public int maxHP;
	public int currentHP;

	[HideInInspector]
	public int attackDamage;
	[HideInInspector]
	public DamageType attackDamageType;
	[HideInInspector]
	public string attackName;

	public List<Ability> abilities;

	[HideInInspector]
	public int discardAmount, spadesBeforeExhaustion, heartsBeforeExhaustion, diamondsBeforeExhaustion;
	[HideInInspector]
	public bool isPlayer, isMyTurn, isAttacking, currentlyTargeted, hasDefended, isDead;
	// [HideInInspector]
	public string team;
	[HideInInspector]
	public ChampionController currentTarget, currentNemesis;
	[HideInInspector]
	public Card attackingCard, defendingCard;
	[HideInInspector]
	public bool isUltReady;

	private void Start()
	{
		ChampionSetup();
	}
	private void Update()
	{
		TextUpdater();
	}

	public void ChampionSetup()
	{
		name = champion.name;
		avatar = champion.avatar;
		gender = champion.gender;
		faction = champion.faction;
		race = champion.race;

		championButton = gameObject.GetComponent<Button>();
		nameText = transform.GetChild(0).GetComponent<TMP_Text>();
		healthText = transform.GetChild(1).GetComponent<TMP_Text>();
		cardsText = transform.GetChild(2).GetComponent<TMP_Text>();

		maxHP = champion.maxHP;
		currentHP = champion.currentHP;

		attackDamage = champion.attackDamage;
		attackDamageType = champion.attackDamageType;
		attackName = champion.attackName;

		abilities = champion.abilities;

		discardAmount = 0;
		ResetExhaustion();
		isPlayer = this == GameController.instance.champions[0];
		isMyTurn = false;
		isAttacking = false;
		hasDefended = false;
		isDead = false;
		// team = null; // this is not needed
		currentTarget = null;
		attackingCard = null;
		isUltReady = false;

		GetComponent<Image>().sprite = avatar;

		/*if (isPlayer || team == "PlayerTeam")
		{
			healthText.transform.localPosition = new Vector3(healthText.transform.localPosition.x, -healthText.transform.localPosition.y, healthText.transform.localPosition.z);
			cardsText.transform.localPosition = new Vector3(cardsText.transform.localPosition.x, -cardsText.transform.localPosition.y, cardsText.transform.localPosition.z);
			cardsText.verticalAlignment = VerticalAlignmentOptions.Bottom;
		}*/
	}

	public IEnumerator Attack(ChampionController target)
	{
		yield return StartCoroutine(target.Damage(attackDamage, attackDamageType, this));
	}
	public IEnumerator Damage(int amount, DamageType damageType, ChampionController source = null, bool abilityCheck = true)
	{
		int healthAfterDamage = Mathf.Max(currentHP - amount, 0);
		foreach (Transform child in abilityPanel.panel.transform)
		{
			AbilityController ability = child.GetComponent<AbilityController>();
			healthAfterDamage -= ability.DamageCalculationBonus(amount, damageType);
		}
		currentHP = healthAfterDamage;

		float magnitude;
		switch (damageType)
		{
			case DamageType.Melee:
				magnitude = 20f;
				AudioController.instance.Play("Sword" + Random.Range(1,3));
				break;
			case DamageType.Ranged:
				magnitude = 12f;
				break;
			case DamageType.Fire:
				magnitude = 8f;
				AudioController.instance.Play("FireDamage1");
				break;
			case DamageType.Lightning:
				magnitude = 15f;
				break;
			case DamageType.Shadow:
				magnitude = 10f;
				AudioController.instance.Play("Unblockable1");
				break;
			case DamageType.Unblockable:
				magnitude = 5f;
				AudioController.instance.Play("Unblockable1");
				break;
			default:
				magnitude = 5f;
				AudioController.instance.Play("Unblockable1");
				break;

		}
		StartCoroutine(ShakeImage(0.2f, magnitude));

		if (abilityCheck == false) yield break;
		foreach (Transform child in abilityPanel.panel.transform)
		{
			AbilityController ability = child.GetComponent<AbilityController>();
			yield return StartCoroutine(ability.OnDamage());
		}
	}
	public IEnumerator Heal(int amount, bool abilityCheck = true)
	{
		currentHP = Mathf.Min(currentHP + amount, maxHP);
		AudioController.instance.Play("Heal");

		if (abilityCheck == false) yield break;
		foreach (Transform child in abilityPanel.panel.transform)
		{
			AbilityController ability = child.GetComponent<AbilityController>();
			yield return StartCoroutine(ability.OnHeal());
		}
	}

	[HideInInspector]
	public void ResetExhaustion()
	{
		spadesBeforeExhaustion = 1;
		heartsBeforeExhaustion = 3;
		diamondsBeforeExhaustion = 1;
	}
	[HideInInspector]
	public void SetHand(Hand hand)
	{
		this.hand = hand;
		hand.owner = this;
	}
	[HideInInspector]
	private void TextUpdater()
	{
		nameText.text = name;
		healthText.text = currentHP.ToString();
		cardsText.text = hand.transform.childCount.ToString();
		if (currentHP == 0)
		{
			healthText.color = new Color32(100, 100, 100, 255);
			healthText.text = "DEAD";
			return;
		}
		if (currentHP <= 0.6f * maxHP)
		{
			healthText.color = currentHP <= 0.3f * maxHP ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 0, 255);
		}
		else
		{
			healthText.color = new Color32(0, 255, 0, 255);
		}
	}

	[HideInInspector]
	public void OnClick()
	{
		foreach (ChampionController champion in GameController.instance.champions)
		{

			if (!champion.isAttacking || !champion.isPlayer || isPlayer || isDead) continue;

			champion.currentTarget = this;
			GameController.instance.playerActionTooltip.text = (champion.team != this.team) ? "Confirm the attack, or change selected card and/or target." : "This champion is on your team! Confirm the attack, or consider changing the selected target and/or selected card.";
			GameController.instance.confirmButton.gameObject.SetActive(true);

			if (champion.attackingCard != null) break;
			GameController.instance.playerActionTooltip.text = (champion.team != this.team) ? "Choose another card to represent your attack, or change selected target." : "This champion is on your team! Consider changing the selected target, or continue by selecting a card to represent your attack.";
			GameController.instance.confirmButton.gameObject.SetActive(false);
		}
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			abilityPanel.OpenPanel();
		}
	}
	private IEnumerator ShakeImage(float duration, float magnitude)
	{
		Vector3 originalPos = transform.localPosition;

		for (float t = 0; t < 1; t += Time.deltaTime / duration)
		{
			float x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
			float y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
			Vector3 shake = new Vector3(x, y, originalPos.z);
			transform.localPosition = shake;
			yield return null;
		}
		transform.localPosition = originalPos;
	}
}
