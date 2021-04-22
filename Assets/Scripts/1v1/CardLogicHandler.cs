using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLogicHandler : MonoBehaviour
{
    #region Variables
    public GameHandler gameHandler;
    public GameObject summonObject;

    Card card;
    bool cardOfPlayer;
    #endregion

    void Update()
    {
        PurgePlayArea();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            card.ToggleCardVisibility();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            GameObject gameObject = Instantiate(summonObject, new Vector2(0, 0), Quaternion.identity);
            gameObject.transform.SetParent(gameHandler.playerArea.transform, false);
        }
    }

    #region Card Functions
    public IEnumerator CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == gameHandler.playerArea;

        if (cardOfPlayer)
        {
            switch (gameHandler.phase)
            {
                case GamePhase.PLAYERACTIONPHASE:
                    if (gameHandler.player.isAttacking)
                    {
                        switch (gameHandler.player.championName)
						{
                            case "The Wraith King":
                                if (gameHandler.player.isUndeadTurningReady)
								{
                                    Discard(card);
                                    gameHandler.player.undeadTurningMultiplier++;
                                    yield break;
								}
                                break;
						}
                        StartCoroutine(AttackCalc(card));
                    }
                    else
                    {
                        switch (this.card.cardType)
                        {
                            case CardType.SPADE:
                                PlayerSpade(card);
                                break;
                            case CardType.HEART:
                                PlayerHeart(card);
                                break;
                            case CardType.CLUB:
                                PlayerClub(card);
                                break;
                            case CardType.DIAMOND:
                                StartCoroutine(PlayerDiamond(card));
                                break;
                        }
                    }
                    break;
                case GamePhase.PLAYERENDPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
                        PlayerDiscard("Normal");
                    }
                    break;
                case GamePhase.OPPONENTACTIONPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
                        PlayerDiscard("Forced");
                        break;
                    }

                    if (gameHandler.opponent.isAttacking || gameHandler.player.isAttacked)
                    {
                        DefenseCalc(card);
                        break;
                    }

					switch (gameHandler.player.championName)
					{
                        case "The Wraith King":
                            gameHandler.player.DeathMist(gameHandler, this.card);
                            gameHandler.playerActionTooltip.text = "It is the opponent's Action Phase.";
                            StartCoroutine(OpponentCardLogic());
                            break;
					}
                    break;
                default:
                    Debug.Log("It's not the player's turn!");
                    break;
            }
        }
        else
        {
            Debug.Log("This is not the player's card!");
        }
        yield break;
    }
    public IEnumerator OpponentCardLogic()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 1f));

        foreach (Transform child in gameHandler.opponentArea.transform)
        {
            GameObject selectedCard = child.gameObject;
            Card selectedCardComponent = selectedCard.GetComponent<Card>();
            if (selectedCardComponent.cardType == CardType.CLUB)
            {
                if (selectedCardComponent.cardValue >= 10)
                {
                    Debug.Log("The opponent refuses to trade in a card worth: " + selectedCardComponent.cardValue);
                    continue;
                }

                Discard(selectedCard, true);
                gameHandler.DealCardsOpponent(1);

                switch (gameHandler.player.championName)
				{
                    case "The Wraith King":
                        gameHandler.player.isDeathMistReady = true;
                        gameHandler.playerActionTooltip.text = "Death Mist is ready. Choose a card.";
                        gameHandler.playerAbilityStatus2.text = "Death Mist - UP";
                        gameHandler.skipButton.SetActive(true);
                        yield break;
				}

                StartCoroutine(OpponentCardLogic());
                yield break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        foreach (Transform child in gameHandler.opponentArea.transform)
        {
            GameObject selectedCard = child.gameObject;
            Card selectedCardComponent = selectedCard.GetComponent<Card>();
            if (selectedCardComponent.cardType == CardType.DIAMOND && gameHandler.opponent.diamondsBeforeExhaustion != 0)
            {
                switch (selectedCardComponent.cardValue)
                {
                    case 1:
                        gameHandler.DealCards(2);
                        Discard(selectedCard, true);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 2:
                        if (gameHandler.player.cards != 0)
                        {
                            gameHandler.player.discardAmount = 1;
                            Discard(selectedCard, true);
                            gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                            gameHandler.opponent.diamondsBeforeExhaustion--;
                            yield break;
                        }
                        Debug.Log("Player has no cards! Bot cannot use this card.");
                        break;
                    case 3:
                        gameHandler.DealCards(4);
                        Discard(selectedCard, true);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 4:
                        if (gameHandler.player.cards == 0)
						{
                            gameHandler.player.Damage(20, DamageType.Unblockable, gameHandler.player);
                            Debug.Log("Player has no cards! Dealing damage automatically.");
                            StartCoroutine(OpponentCardLogic());
                            yield break;
						}
                        gameHandler.player.discardAmount = 2;
                        Discard(selectedCard, true);
                        gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                        gameHandler.skipButton.SetActive(true);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        yield break;
                    case 5:
                        gameHandler.DealCardsOpponent(1);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 6:
                        gameHandler.DealCardsOpponent(1);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 7:
                        gameHandler.DealCardsOpponent(1);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 8:
                        gameHandler.DealCardsOpponent(1);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 9:
                        if (gameHandler.opponent.currentHP > 0.80f * gameHandler.opponent.maxHP || gameHandler.player.currentHP < 0.25f * gameHandler.player.currentHP)
                        {
                            continue;
                        }
                        gameHandler.player.Heal(10);
                        gameHandler.opponent.Heal(10);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 10:
                        if (gameHandler.opponent.currentHP > 0.5f * gameHandler.opponent.maxHP || gameHandler.player.currentHP < 0.4f * gameHandler.player.currentHP)
                        {
                            continue;
                        }
                        gameHandler.player.Heal(20);
                        gameHandler.opponent.Heal(20);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 11:
                        gameHandler.player.Damage(20, DamageType.Fire, gameHandler.opponent);
                        Discard(selectedCard, true);
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 12:
                        if (gameHandler.player.cards == 0)
                        {
                            gameHandler.player.Damage(40, DamageType.Fire, gameHandler.opponent);
                            Debug.Log("Player has no cards! Dealing damage automatically.");
                            StartCoroutine(OpponentCardLogic());
                            yield break;
                        }
                        gameHandler.player.discardAmount = 4;
                        Discard(selectedCard, true);
                        gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                        gameHandler.skipButton.SetActive(true);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        yield break;
                    default:
                        Debug.Log("Not implemented yet. Skipping...");
                        break;
                }
            }
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        foreach (Transform child in gameHandler.opponentArea.transform)
        {
            GameObject selectedCard = child.gameObject;
            Card selectedCardComponent = selectedCard.GetComponent<Card>();
            GameObject attackingCard = null;
            Card attackingCardComponent = null;
            if (selectedCardComponent.cardType == CardType.SPADE && gameHandler.opponent.spadesBeforeExhaustion != 0)
            {
                int value = -1;
                if (gameHandler.opponent.cards == 0 || gameHandler.player.cards == 0 && Random.Range(0f, 1f) <= 0.75f)
                {
                    attackingCard = Instantiate(gameHandler.cardIndex.playingCards[Random.Range(0, gameHandler.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                    attackingCardComponent = attackingCard.GetComponent<Card>();
                    attackingCardComponent.ToggleCardVisibility();
                }
                else
                {
                    foreach (Transform kid in gameHandler.opponentArea.transform)
                    {
                        if (kid.gameObject.GetComponent<Card>().cardType == CardType.HEART && gameHandler.opponent.currentHP < 0.75f * gameHandler.opponent.maxHP)
						{
                            Debug.Log("The opponent refuses to use a HEART to represent an attack!");
                            continue;
						}
                        if (value < kid.gameObject.GetComponent<Card>().cardValue)
                        {
                            if (gameHandler.opponent.currentHP >= 0.5f * gameHandler.opponent.maxHP
                                && kid.GetComponent<Card>().cardValue >= 12
                                && Random.Range(0f, 1f) <= 0.75f)
							{
                                Debug.Log("The opponent is confident! They refuse to use a value of " + kid.gameObject.GetComponent<Card>().cardValue + " to attack!");
                                continue;
							}
                            attackingCard = kid.gameObject;
                            attackingCardComponent = attackingCard.GetComponent<Card>();
                            value = attackingCardComponent.cardValue;
                        }
                    }
                }

                float f = gameHandler.player.currentHP <= gameHandler.player.maxHP * 0.25f ? 0.35f : 0.10f;
                if (selectedCardComponent.cardValue >= 8 && Random.Range(0f, 1f) <= f
                    || selectedCardComponent.cardValue >= attackingCardComponent.cardValue
                    || attackingCardComponent.cardValue <= 9 && Random.Range(0f, 1f) <= f)
                {
                    if (gameHandler.opponent.cards <= 3)
					{
                        if (gameHandler.opponent.cards != 1 && Random.Range(0f, 1f) >= f)
						{
                            Debug.Log("The opponent does not want to attack! Reason: Less than three cards.");
						}
					}
                    else
					{
                        Debug.Log("The opponent does not want to attack!");
                        continue;
                    }
                }

                Discard(selectedCard, true);
                Discard(attackingCard);
                gameHandler.opponent.spadesBeforeExhaustion--;
                gameHandler.opponent.isAttacking = true;
                gameHandler.player.isAttacked = true;
                gameHandler.gambleButton.SetActive(true);

                gameHandler.playerActionTooltip.text = "The opponent is attacking. Defend with a card.";
                Debug.Log("Opponent is attacking the player with a card with a value of " + attackingCardComponent.cardValue);

                yield break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        if (gameHandler.opponent.currentHP < gameHandler.opponent.maxHP)
        {
            foreach (Transform child in gameHandler.opponentArea.transform)
            {
                GameObject selectedCard = child.gameObject;
                Card selectedCardComponent = selectedCard.GetComponent<Card>();
                if (selectedCardComponent.cardType == CardType.HEART && gameHandler.opponent.heartsBeforeExhaustion != 0)
                {
                    if (selectedCardComponent.cardValue <= 6)
                    {
                        gameHandler.opponent.Heal(5);
                        Discard(selectedCard, true);
                        gameHandler.opponent.heartsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCardComponent.cardValue >= 7 && selectedCard.GetComponent<Card>().cardValue <= 9 && gameHandler.opponent.heartsBeforeExhaustion >= 2)
                    {

                        gameHandler.opponent.Heal(10);
                        Discard(selectedCard, true);
                        gameHandler.opponent.heartsBeforeExhaustion -= 2;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCardComponent.cardValue >= 10 && selectedCard.GetComponent<Card>().cardValue <= 13 && gameHandler.opponent.heartsBeforeExhaustion >= 3)
                    {
                        gameHandler.opponent.Heal(20);
                        Discard(selectedCard, true);
                        gameHandler.opponent.heartsBeforeExhaustion -= 3;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCardComponent.cardValue == 14 && gameHandler.opponent.heartsBeforeExhaustion == 3)
                    {
                        gameHandler.opponent.Heal(40);
                        Discard(selectedCard, true);
                        gameHandler.opponent.heartsBeforeExhaustion -= 3;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 1f));

        gameHandler.EndOpponentTurn();
    }
    #endregion

    #region Card Logic Interpreters
    public IEnumerator AttackCalc(GameObject attackingCard)
	{
        gameHandler.gambleButton.SetActive(false);
        GameObject opponentCard = null;
        Card opponentCardComponent = null;
        Card attackingCardComponent = attackingCard.GetComponent<Card>();

        int value = -1;
        foreach (Transform child in gameHandler.opponentArea.transform)
        {
            GameObject selectedCard = child.gameObject;
            Card selectedCardComponent = selectedCard.GetComponent<Card>();
            if (value < selectedCardComponent.cardValue)
            {
                if (gameHandler.opponent.currentHP >= 0.5f * gameHandler.opponent.maxHP
                                && selectedCardComponent.cardValue >= 12
                                && Random.Range(0f, 1f) <= 0.75f)
                {
                    Debug.Log("The opponent is confident! They refuse to use a value of " + selectedCardComponent.cardValue + " to defend!");
                    continue;
                }
                value = selectedCardComponent.cardValue;
                opponentCard = selectedCard;
                opponentCardComponent = selectedCardComponent;
            }
        }
        opponentCard = opponentCard == null ? Instantiate(gameHandler.cardIndex.playingCards[Random.Range(0, gameHandler.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity) : opponentCard;

        Debug.Log("Player is attacking the opponent with a card with a value of " + attackingCard.GetComponent<Card>().cardValue);
        gameHandler.opponent.isAttacked = true;
        Discard(attackingCard, true);

        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

        attackingCard.GetComponent<Card>().ToggleCardVisibility(true);
        Discard(opponentCard, true);

        if (attackingCardComponent.cardValue > opponentCardComponent.cardValue)
        {
            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType, gameHandler.player);

            switch (gameHandler.player.championName)
            {
                case "The Wraith King":
                    if (attackingCardComponent.cardType == CardType.CLUB)
                    {
                        gameHandler.player.isUndeadTurningReady = true;
                        gameHandler.playerActionTooltip.text = "Undead Turning is ready. Select CLUBS to amplify attack.";
                        gameHandler.skipButton.SetActive(true);
                        gameHandler.skipButton.transform.GetChild(0).GetComponent<Text>().text = "Confirm";
                        yield break;
                    }
                    break;
            }
        }
        else if (attackingCardComponent.cardValue < opponentCardComponent.cardValue)
        {
            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType, gameHandler.opponent);
        }

        Debug.Log("Player: " + gameHandler.player.currentHP);
        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);

        gameHandler.player.isAttacking = false;
        gameHandler.opponent.isAttacked = false;

        gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
    }
    public void DefenseCalc(GameObject defendingCard)
	{
        gameHandler.gambleButton.SetActive(false);

        Discard(defendingCard, false);
        GameObject attackingCard = gameHandler.playArea.transform.GetChild(defendingCard.transform.GetSiblingIndex() - 1).gameObject;
        Card attackingCardComponent = attackingCard.GetComponent<Card>();
        Card defendingCardComponent = defendingCard.GetComponent<Card>();

        attackingCardComponent.ToggleCardVisibility(true);

        if (defendingCardComponent.cardValue > attackingCardComponent.cardValue)
        {
            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType, gameHandler.player);
        }
        else if (defendingCardComponent.cardValue < attackingCardComponent.cardValue)
        {
            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType, gameHandler.opponent);
        }
        if (gameHandler.player.currentHP == 0 || gameHandler.opponent.currentHP == 0) return;

        Debug.Log("Player: " + gameHandler.player.currentHP);
        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);
        gameHandler.playerActionTooltip.text = "It is the opponent's Action Phase.";

        gameHandler.opponent.isAttacking = false;
        gameHandler.player.isAttacked = false;

        StartCoroutine(OpponentCardLogic());
    }
    public void Discard(GameObject card, bool flip = false, bool animate = true)
	{
        card.transform.SetParent(gameHandler.playArea.transform, false);
        if (flip) card.GetComponent<Card>().ToggleCardVisibility();
        if (animate)
        {
            card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            StartCoroutine(card.GetComponent<HoverScale>().ScaleDown(new Vector3(1f, 1f, 1f)));
        }
    }
    void PlayerSpade(GameObject card)
	{
        if (gameHandler.player.spadesBeforeExhaustion <= 0)
        {
            Debug.Log("Player is exhausted! Cannot play more spades.");
            return;
        }

        Debug.Log("Player is now attacking the opponent.");
        gameHandler.player.isAttacking = true;
        gameHandler.player.spadesBeforeExhaustion--;
        Discard(card);

        gameHandler.endTurnButton.GetComponent<Button>().interactable = false;
        gameHandler.gambleButton.SetActive(true);
    }
    void PlayerHeart(GameObject card)
	{
        Card cardComponent = card.GetComponent<Card>();

        if (gameHandler.player.heartsBeforeExhaustion <= 0)
        {
            Debug.Log("Player is exhausted! Cannot play more hearts.");
            return;
        }
        if (gameHandler.player.currentHP >= gameHandler.player.maxHP)
        {
            Debug.Log("Player health is full!");
            return;
        }
        Debug.Log("Player is attempting to heal.");
        if (cardComponent.cardValue <= 6)
        {
            gameHandler.player.Heal(5);
            gameHandler.player.heartsBeforeExhaustion--;
            Discard(card);
        }
        if (cardComponent.cardValue >= 7 && cardComponent.cardValue <= 9 && gameHandler.player.heartsBeforeExhaustion >= 2)
        {

            gameHandler.player.Heal(10);
            gameHandler.player.heartsBeforeExhaustion -= 2;
            Discard(card);
        }
        if (cardComponent.cardValue >= 10 && cardComponent.cardValue <= 13 && gameHandler.player.heartsBeforeExhaustion >= 3)
        {
            gameHandler.player.Heal(20);
            gameHandler.player.heartsBeforeExhaustion -= 3;
            Discard(card);
        }
        if (cardComponent.cardValue == 14 && gameHandler.player.heartsBeforeExhaustion == 3)
        {
            gameHandler.player.Heal(40);
            gameHandler.player.heartsBeforeExhaustion -= 3;
            Discard(card);
        }
    }
    void PlayerClub(GameObject card)
	{
        Debug.Log("Player is attempting to trade a CLUB.");
        gameHandler.DealCardsPlayer(1);
        Discard(card);
    }
    IEnumerator PlayerDiamond(GameObject card)
	{
        Card cardComponent = card.GetComponent<Card>();

        if (cardComponent.cardValue < 5 || cardComponent.cardValue > 8)
        {
            if (gameHandler.player.diamondsBeforeExhaustion <= 0)
            {
                Debug.Log("Player is exhausted! Cannot play more diamonds.");
                yield break;
            }
        }
        Debug.Log("Player is attempting to use a DIAMOND.");
        switch (cardComponent.cardValue)
        {
            case 1:
                gameHandler.DealCards(2);
                Discard(card);
                gameHandler.player.diamondsBeforeExhaustion--;
                break;
            case 2:
                if (gameHandler.opponent.cards == 0)
                {
                    Debug.Log("Opponent has no cards! Cannot use this card.");
                    break;
                }

                gameHandler.opponent.discardAmount = 1;
                Discard(card);
                gameHandler.endTurnButton.GetComponent<Button>().interactable = false;

                yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                {
                    gameHandler.opponent.cards = gameHandler.opponentArea.transform.childCount;
                    int value = 999;
                    int siblingIndex = 0;
                    for (int x = 0; x < gameHandler.opponent.cards; x++)
                    {
                        if (value > gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                        {
                            value = gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                            siblingIndex = gameHandler.opponentArea.transform.GetChild(x).GetSiblingIndex();
                        }
                    }
                    GameObject opponentCard = gameHandler.opponentArea.transform.GetChild(siblingIndex).gameObject;
                    Discard(opponentCard, true);
                }
                gameHandler.opponent.discardAmount = 0;
                gameHandler.player.diamondsBeforeExhaustion--;
                gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
                break;
            case 3:
                gameHandler.DealCards(4);
                Discard(card);
                gameHandler.player.diamondsBeforeExhaustion--;
                break;
            case 4:
                float chance = gameHandler.opponent.currentHP >= 75 ? 0.75f : 0.5f;
                if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 20 || gameHandler.opponent.cards == 0)
                {
                    Discard(card);

                    yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

                    gameHandler.opponent.Damage(20, DamageType.Unblockable, gameHandler.player);
                    gameHandler.player.diamondsBeforeExhaustion--;
                    break;
                }

                gameHandler.opponent.discardAmount = gameHandler.opponent.cards < 2 ? gameHandler.opponent.cards : 2;
                Discard(card);
                gameHandler.endTurnButton.GetComponent<Button>().interactable = false;

                yield return new WaitForSeconds(Random.Range(0.1f, 1.5f));

                for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                {
                    gameHandler.opponent.cards = gameHandler.opponentArea.transform.childCount;
                    int value = 999;
                    int siblingIndex = 0;
                    for (int x = 0; x < gameHandler.opponent.cards; x++)
                    {
                        if (value > gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                        {
                            value = gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                            siblingIndex = gameHandler.opponentArea.transform.GetChild(x).GetSiblingIndex();
                        }
                    }
                    GameObject opponentCard = gameHandler.opponentArea.transform.GetChild(siblingIndex).gameObject;
                    Discard(opponentCard, true);
                }
                gameHandler.opponent.discardAmount = 0;
                gameHandler.player.diamondsBeforeExhaustion--;
                gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
                break;
            case 5:
                gameHandler.DealCardsPlayer(1);
                Discard(card);
                break;
            case 6:
                gameHandler.DealCardsPlayer(1);
                Discard(card);
                break;
            case 7:
                gameHandler.DealCardsPlayer(1);
                Discard(card);
                break;
            case 8:
                gameHandler.DealCardsPlayer(1);
                Discard(card);
                break;
            case 9:
                gameHandler.player.Heal(10);
                gameHandler.opponent.Heal(10);
                Discard(card);
                break;
            case 10:
                gameHandler.player.Heal(20);
                gameHandler.opponent.Heal(20);
                Discard(card);
                break;
            case 11:
                gameHandler.opponent.Damage(20, DamageType.Fire, gameHandler.player);
                Discard(card);
                break;
            case 12:
                chance = gameHandler.opponent.currentHP >= 75 ? 0.45f : 0.20f;
                if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 40 || gameHandler.opponent.cards == 0)
                {
                    Discard(card);

                    yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                    gameHandler.opponent.Damage(40, DamageType.Fire, gameHandler.player);
                    gameHandler.player.diamondsBeforeExhaustion--;
                    break;
                }

                gameHandler.opponent.discardAmount = gameHandler.opponent.cards < 4 ? gameHandler.opponent.cards : 4;
                Discard(card);
                gameHandler.endTurnButton.GetComponent<Button>().interactable = false;

                yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

                for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                {
                    gameHandler.opponent.cards = gameHandler.opponentArea.transform.childCount;
                    int value = 999;
                    int siblingIndex = 0;
                    for (int x = 0; x < gameHandler.opponent.cards; x++)
                    {
                        if (value > gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                        {
                            value = gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                            siblingIndex = gameHandler.opponentArea.transform.GetChild(x).GetSiblingIndex();
                        }
                    }
                    GameObject opponentCard = gameHandler.opponentArea.transform.GetChild(siblingIndex).gameObject;
                    Discard(opponentCard, true);
                }
                gameHandler.opponent.discardAmount = 0;
                gameHandler.player.diamondsBeforeExhaustion--;
                gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
                break;
        }
    }
    void PlayerDiscard(string type)
	{
        switch (type)
		{
            case "Normal":
                Discard(card.gameObject);
                gameHandler.player.discardAmount--;
                if (gameHandler.player.discardAmount == 0)
                {
                    gameHandler.playerActionTooltip.text = "Waiting for the opponent...";
                }
                else
                {
                    gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                }
                break;
            case "Forced":
                Discard(card.gameObject);
                gameHandler.player.discardAmount--;
                gameHandler.player.cards = gameHandler.playerArea.transform.childCount;
                gameHandler.skipButton.SetActive(false);
                if (gameHandler.player.cards == 0)
				{
                    gameHandler.player.discardAmount = 0;
                }
                if (gameHandler.player.discardAmount == 0)
                {
                    gameHandler.playerActionTooltip.text = "It is the opponent's Action Phase.";
                    StartCoroutine(OpponentCardLogic());
                }
                else
                {
                    gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                }
                break;
            default:
                Debug.LogWarning("No discard type was defined!");
                break;
		}
	}

    #endregion


    #region Other Functions
    private void PurgePlayArea()
    {
        if (gameHandler.playArea.transform.childCount > 7) Destroy(gameHandler.playArea.transform.GetChild(0).gameObject);
    }
    #endregion
}
