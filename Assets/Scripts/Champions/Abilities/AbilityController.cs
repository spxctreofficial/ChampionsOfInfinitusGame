using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityController : MonoBehaviour
{
    public ChampionController champion;
	public Ability ability;

	// Constructors
	public void Setup(ChampionController champion, Ability ability)
	{
		// Setting up variables
		this.champion = champion;
		this.ability = ability;

		// Setting up appearance
		gameObject.GetComponent<Image>().sprite = ability.sprite;
		switch (ability.abilityType)
		{
			case Ability.AbilityType.Passive:
			case Ability.AbilityType.AttackB:
			case Ability.AbilityType.DefenseB:
				gameObject.GetComponent<Button>().interactable = false;
				break;
			default:
				gameObject.AddComponent<SmartHover>();
				break;
		}
	}

	// Click Event
	public void OnClick()
	{

	}

	// Checks
	public bool CheckForAbility(string searchCriteria)
	{
		if (champion.abilities.Count == 0) return false;
		foreach (Ability ability in champion.abilities) if (searchCriteria == ability.abilityID) return true;
		return false;
	}

	// Triggers
	public IEnumerator OnBeginningPhase()
	{
		switch (ability.abilityID)
		{
			case "QuickAssist":
				StartCoroutine(QuickAssist());
				break;
		}
			yield break;
	}
	public IEnumerator OnActionPhase()
	{
		yield break;
	}
	public IEnumerator OnEndPhase()
	{
		yield break;
	}

	public IEnumerator OnDamage()
	{
		yield break;
	}
	public IEnumerator OnHeal()
	{
		yield break;
	}
	public IEnumerator OnDeath()
	{
		yield break;
	}

	// Ability Methods
	private IEnumerator QuickAssist()
	{
		if (!champion.isMyTurn) yield break;

		yield return new WaitForSeconds(1f);

		foreach (ChampionController selectedChampion in GameController.instance.champions)
		{
			if (selectedChampion == champion || selectedChampion.isDead || selectedChampion.faction != champion.faction) continue;

			champion.hand.Deal(1);
			Debug.Log("Quick assist was activated for " + champion.name + ". Dealing that champion a card!");
		}

		StartCoroutine(ShakeImage(0.2f, 10f));
	}





	// Image Shake Borrowed From ChampionController
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
