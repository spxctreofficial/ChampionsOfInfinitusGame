using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionHandler : MonoBehaviour
{

	#region Variables

	#region Default
	public ChampionDeprecated champion;

    public string championName;
    public Sprite championImage;
    public int currentHP, maxHP, attackDamage;
    public string attackName;
    public DamageTypeDeprecated damageType;

    [HideInInspector]
    public bool isAttackUnblockable;
    [HideInInspector]
    public int cards, discardAmount;
    [HideInInspector]
    public int spadesBeforeExhaustion, heartsBeforeExhaustion, diamondsBeforeExhaustion;
    [HideInInspector]
    public bool isDead, isAttacking, isAttacked;
    [HideInInspector]
    public bool isUltReady;
    #endregion

    #region The Wraith King
    [HideInInspector]
    public int clubs, undeadTurningMultiplier;
    [HideInInspector]
    public bool isDeathCrownReady, isDeathMistReady, isUndeadTurningReady;
	#endregion

	#endregion

	public void ChampionSetup()
    {
		#region Default
		championName = champion.championName;
        championImage = champion.championImage;
        currentHP = champion.currentHP;
        maxHP = champion.maxHP;
        attackDamage = champion.attackDamage;
        attackName = champion.attackName;
        damageType = champion.damageType;
        isAttackUnblockable = false;
        cards = 0;
        discardAmount = 0;
        spadesBeforeExhaustion = 1;
        heartsBeforeExhaustion = 3;
        diamondsBeforeExhaustion = 1;
        isDead = false;
        isAttacking = false;
        isAttacked = false;
        isUltReady = false;

        var thisImage = GetComponent<Image>();
        thisImage.sprite = this.championImage;
        #endregion

        #region The Wraith King
        clubs = 0;
        undeadTurningMultiplier = 0;
        isDeathCrownReady = false;
        isDeathMistReady = false;
        isUndeadTurningReady = false;

		#endregion
	}

	#region Default Champion Functions
	[HideInInspector]
    public void Damage(int amount, DamageTypeDeprecated damageType, ChampionHandler source, float shakeMagnitude = 5f)
    {
        var gameHandler = FindObjectOfType<GameHandler>();
        var audioManager = FindObjectOfType<AudioManager>();

        switch (source.championName)
		{
            case "The Wraith King":
                if (damageType == DamageTypeDeprecated.Melee && isDeathCrownReady)
                {
                    amount += source.DeathCrown();
                }
                break;
		}

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        switch (damageType)
		{
            case DamageTypeDeprecated.Melee:
                shakeMagnitude = 20f;
                audioManager.Play("Sword" + Random.Range(1, 2));
                break;
            case DamageTypeDeprecated.Ranged:
                shakeMagnitude = 10f;
                break;
            case DamageTypeDeprecated.Fire:
                shakeMagnitude = 8f;
                audioManager.Play("FireDamage1");
                break;
            case DamageTypeDeprecated.Lightning:
                shakeMagnitude = 15f;
                break;
            default:
                audioManager.Play("Unblockable1");
                break;

		}
        StartCoroutine(ShakeImageViolent(0.2f, shakeMagnitude));

        if (gameHandler.player.currentHP == 0)
		{
            gameHandler.phase = GamePhaseDeprecated.GAMELOSE;
            StartCoroutine(gameHandler.GameEnd());
		}
        else if (gameHandler.opponent.currentHP == 0)
		{
            gameHandler.phase = GamePhaseDeprecated.GAMEWIN;
            StartCoroutine(gameHandler.GameEnd());
		}
    }
    [HideInInspector]
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        FindObjectOfType<AudioManager>().Play("Heal");
    }
    public void EnlargeChampionDashboard()
    {
        FindObjectOfType<GameHandler>().EnlargeChampionDashboard(championImage);
    }
    private IEnumerator ShakeImage(float duration, float shakeRange)
	{
        var elapsed = 0.0f;
        var originalRotation = transform.rotation;

        while (elapsed < duration)
        {

            elapsed += Time.deltaTime;
            var z = Random.value * shakeRange - (shakeRange / 2);
            transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y, originalRotation.z + z);
            yield return null;
        }

        transform.rotation = originalRotation;
    }
    private IEnumerator ShakeImageViolent(float duration, float magnitude)
    {
        var originalPos = transform.localPosition;

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            var x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
            var y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
            var ohShit = new Vector3(x, y, originalPos.z);
            transform.localPosition = ohShit;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
    #endregion

    #region The Wraith King Functions
    [HideInInspector]
    public int DeathCrown()
    {
        if (isDeathCrownReady && attackName == "Wraith's Edge")
        {
            return 10;
        }
        else
        {
            return 0;
        }
    }
    [HideInInspector]
    public void DeathMist(GameHandler gameHandler, Card card)
	{
        if (isDeathMistReady)
		{
            var newCard = Instantiate(gameHandler.cardIndex.ClubToken, new Vector2(0, 0), Quaternion.identity);
            var siblingIndex = card.transform.GetSiblingIndex();

            gameHandler.cardLogicHandler.Discard(card.gameObject);
            newCard.transform.SetParent(gameHandler.playerArea.transform, false);
            newCard.transform.SetSiblingIndex(siblingIndex);
            gameHandler.skipButton.SetActive(false);
            gameHandler.playerAbilityStatus2.text = "";
            isDeathMistReady = false;
        }
        else
		{
            Debug.LogWarning("how the fuck were yuou able to access this method you bitch how the fuck fwhafefaewfawef");
		}
	}
    [HideInInspector]
    public IEnumerator UndeadTurning(GameHandler gameHandler, ChampionHandler target)
	{
        isUndeadTurningReady = false;
        for (var x = 0; x < undeadTurningMultiplier; x++)
		{
            target.Damage(5, DamageTypeDeprecated.Shadow, this);
            yield return new WaitForSeconds(0.25f);
        }

        gameHandler.player.isAttacking = false;
        gameHandler.player.isAttacked = false;

        gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
    }
    #endregion
}
