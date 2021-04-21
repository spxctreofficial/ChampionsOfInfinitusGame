using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLogicHandler : MonoBehaviour
{
    #region Variables
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    public GameObject playerArea;
    public GameObject opponentArea;
    public GameObject playArea;
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
            gameObject.transform.SetParent(opponentArea.transform, false);
        }
    }

    #region Card Functions
    public IEnumerator CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == playerArea;

        if (cardOfPlayer)
        {
            switch (gameHandler.phase)
            {
                case GamePhase.PLAYERACTIONPHASE:
                    if (gameHandler.player.isAttacking)
                    {
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

        for (int x = 0; x < gameHandler.opponent.cards; x++)
        {
            if (gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.CLUB)
            {
                GameObject selectedCard = gameHandler.opponentArea.transform.GetChild(x).gameObject;

                if (selectedCard.GetComponent<Card>().cardValue >= 10)
                {
                    Debug.Log("The opponent refuses to trade in a card worth: " + selectedCard.GetComponent<Card>().cardValue);
                    continue;
                }

                selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                selectedCard.GetComponent<Card>().ToggleCardVisibility();
                gameHandler.DealCardsOpponent(1);
                StartCoroutine(OpponentCardLogic());
                yield break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        for (int x = 0; x < gameHandler.opponent.cards; x++)
        {
            if (gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.DIAMOND && gameHandler.opponent.diamondsBeforeExhaustion != 0)
            {
                GameObject selectedCard = gameHandler.opponentArea.transform.GetChild(x).gameObject;
                switch (selectedCard.GetComponent<Card>().cardValue)
                {
                    case 1:
                        gameHandler.DealCards(2);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 2:
                        if (gameHandler.player.cards != 0)
                        {
                            gameHandler.player.discardAmount = 1;
                            selectedCard.transform.SetParent(playArea.transform, false);
                            selectedCard.GetComponent<Card>().ToggleCardVisibility();
                            gameHandler.playerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                            gameHandler.opponent.diamondsBeforeExhaustion--;
                            yield break;
                        }
                        break;
                    case 3:
                        gameHandler.DealCards(4);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 5:
                        gameHandler.DealCardsOpponent(1);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 6:
                        gameHandler.DealCardsOpponent(1);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 7:
                        gameHandler.DealCardsOpponent(1);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 8:
                        gameHandler.DealCardsOpponent(1);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 9:
                        if (gameHandler.opponent.currentHP > 0.80f * gameHandler.opponent.maxHP || gameHandler.player.currentHP < 0.25f * gameHandler.player.currentHP)
                        {
                            continue;
                        }
                        gameHandler.player.Heal(10);
                        gameHandler.opponent.Heal(10);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 10:
                        if (gameHandler.opponent.currentHP > 0.5f * gameHandler.opponent.maxHP || gameHandler.player.currentHP < 0.4f * gameHandler.player.currentHP)
                        {
                            continue;
                        }
                        gameHandler.player.Heal(20);
                        gameHandler.opponent.Heal(20);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    case 11:
                        gameHandler.player.Damage(20, DamageType.Fire);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    default:
                        Debug.Log("Not implemented yet. Skipping...");
                        break;
                }
            }
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        for (int x = 0; x < gameHandler.opponent.cards; x++)
        {
            if (gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.SPADE && gameHandler.opponent.spadesBeforeExhaustion != 0)
            {
                GameObject selectedCard = gameHandler.opponentArea.transform.GetChild(x).gameObject;
                GameObject attackingCard = null;
                Card selectedCardComponent = selectedCard.GetComponent<Card>();

                int value = -1;
                if (gameHandler.opponent.cards == 0)
                {
                    attackingCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                }
                else
                {
                    for (int y = 0; y < gameHandler.opponent.cards; y++)
                    {
                        if (gameHandler.opponentArea.transform.GetChild(y).gameObject.GetComponent<Card>().cardType == CardType.HEART && gameHandler.opponent.currentHP < 0.75f * gameHandler.opponent.maxHP)
						{
                            Debug.Log("The opponent refuses to use a HEART to represent an attack!");
                            continue;
						}
                        if (value < gameHandler.opponentArea.transform.GetChild(y).gameObject.GetComponent<Card>().cardValue)
                        {
                            attackingCard = gameHandler.opponentArea.transform.GetChild(y).gameObject;
                            value = attackingCard.GetComponent<Card>().cardValue;
                        }
                    }
                }

                float f = gameHandler.player.currentHP <= gameHandler.player.maxHP * 0.25f ? 0.35f : 0.10f;
                if (selectedCardComponent.cardValue >= 8 && Random.Range(0f, 1f) <= f
                    || selectedCardComponent.cardValue >= attackingCard.GetComponent<Card>().cardValue
                    || attackingCard.GetComponent<Card>().cardValue <= 9 && Random.Range(0f, 1f) <= f)
                {
                    continue;
                }

                selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                selectedCard.GetComponent<Card>().ToggleCardVisibility();
                attackingCard.transform.SetParent(gameHandler.playArea.transform, false);
                gameHandler.opponent.spadesBeforeExhaustion--;
                gameHandler.opponent.isAttacking = true;
                gameHandler.player.isAttacked = true;
                gameHandler.gambleButton.SetActive(true);

                gameHandler.playerActionTooltip.text = "The opponent is attacking. Defend with a card.";
                Debug.Log("Opponent is attacking the player with a card with a value of " + attackingCard.GetComponent<Card>().cardValue);

                yield break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        if (gameHandler.opponent.currentHP < gameHandler.opponent.maxHP)
        {
            for (int x = 0; x < gameHandler.opponent.cards; x++)
            {
                if (gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.HEART && gameHandler.opponent.heartsBeforeExhaustion != 0)
                {
                    GameObject selectedCard = gameHandler.opponentArea.transform.GetChild(x).gameObject;

                    if (selectedCard.GetComponent<Card>().cardValue <= 6)
                    {
                        gameHandler.opponent.Heal(5);
                        selectedCard.transform.SetParent(playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        gameHandler.opponent.heartsBeforeExhaustion--;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCard.GetComponent<Card>().cardValue >= 7 && selectedCard.GetComponent<Card>().cardValue <= 9 && gameHandler.opponent.heartsBeforeExhaustion >= 2)
                    {

                        gameHandler.opponent.Heal(10);
                        selectedCard.transform.SetParent(playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        gameHandler.opponent.heartsBeforeExhaustion -= 2;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCard.GetComponent<Card>().cardValue >= 10 && selectedCard.GetComponent<Card>().cardValue <= 13 && gameHandler.opponent.heartsBeforeExhaustion >= 3)
                    {
                        gameHandler.opponent.Heal(20);
                        selectedCard.transform.SetParent(playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        gameHandler.opponent.heartsBeforeExhaustion -= 3;
                        StartCoroutine(OpponentCardLogic());
                        yield break;
                    }
                    if (selectedCard.GetComponent<Card>().cardValue == 14 && gameHandler.opponent.heartsBeforeExhaustion == 3)
                    {
                        gameHandler.opponent.Heal(40);
                        card.transform.SetParent(playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
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

        int value = -1;
        int siblingIndex = 0;
        for (int x = 0; x < gameHandler.opponent.cards; x++)
        {
            if (value < gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
            {
                value = gameHandler.opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                siblingIndex = gameHandler.opponentArea.transform.GetChild(x).GetSiblingIndex();
            }
        }
        GameObject opponentCard = gameHandler.opponent.cards == 0 ? Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity) : gameHandler.opponentArea.transform.GetChild(siblingIndex).gameObject;

        Debug.Log("Player is attacking the opponent with a card with a value of " + attackingCard.GetComponent<Card>().cardValue);
        gameHandler.opponent.isAttacked = true;
        attackingCard.transform.SetParent(playArea.transform, false);
        attackingCard.GetComponent<Card>().ToggleCardVisibility();

        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

        opponentCard.transform.SetParent(playArea.transform, false);
        attackingCard.GetComponent<Card>().ToggleCardVisibility();

        if (attackingCard.GetComponent<Card>().cardValue > opponentCard.GetComponent<Card>().cardValue)
        {
            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType);
        }
        else if (attackingCard.GetComponent<Card>().cardValue < opponentCard.GetComponent<Card>().cardValue)
        {
            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType);
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

        defendingCard.transform.SetParent(playArea.transform, false);
        GameObject attackingCard = gameHandler.playArea.transform.GetChild(defendingCard.transform.GetSiblingIndex() - 1).gameObject;
        attackingCard.GetComponent<Card>().ToggleCardVisibility();

        if (defendingCard.GetComponent<Card>().cardValue > attackingCard.GetComponent<Card>().cardValue)
        {
            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType);
        }
        else if (defendingCard.GetComponent<Card>().cardValue < attackingCard.GetComponent<Card>().cardValue)
        {
            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType);
        }
        if (gameHandler.player.currentHP == 0 || gameHandler.opponent.currentHP == 0) return;

        Debug.Log("Player: " + gameHandler.player.currentHP);
        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);
        gameHandler.playerActionTooltip.text = "It is the opponent's Action Phase.";

        gameHandler.opponent.isAttacking = false;
        gameHandler.player.isAttacked = false;

        StartCoroutine(OpponentCardLogic());
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
        card.transform.SetParent(playArea.transform, false);

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
            card.transform.SetParent(playArea.transform, false);
        }
        if (cardComponent.cardValue >= 7 && cardComponent.cardValue <= 9 && gameHandler.player.heartsBeforeExhaustion >= 2)
        {

            gameHandler.player.Heal(10);
            gameHandler.player.heartsBeforeExhaustion -= 2;
            card.transform.SetParent(playArea.transform, false);
        }
        if (cardComponent.cardValue >= 10 && cardComponent.cardValue <= 13 && gameHandler.player.heartsBeforeExhaustion >= 3)
        {
            gameHandler.player.Heal(20);
            gameHandler.player.heartsBeforeExhaustion -= 3;
            card.transform.SetParent(playArea.transform, false);
        }
        if (cardComponent.cardValue == 14 && gameHandler.player.heartsBeforeExhaustion == 3)
        {
            gameHandler.player.Heal(40);
            gameHandler.player.heartsBeforeExhaustion -= 3;
            card.transform.SetParent(playArea.transform, false);
        }
    }
    void PlayerClub(GameObject card)
	{
        Card cardComponent = card.GetComponent<Card>();
        Debug.Log("Player is attempting to trade a CLUB.");
        gameHandler.DealCardsPlayer(1);
        card.transform.SetParent(playArea.transform, false);
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
                card.transform.SetParent(playArea.transform, false);
                gameHandler.player.diamondsBeforeExhaustion--;
                break;
            case 2:
                if (gameHandler.opponent.cards == 0)
                {
                    card.transform.SetParent(playArea.transform, false);
                    gameHandler.player.diamondsBeforeExhaustion--;
                    break;
                }

                gameHandler.opponent.discardAmount = 1;
                card.transform.SetParent(playArea.transform, false);
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
                    opponentCard.transform.SetParent(playArea.transform, false);
                }
                gameHandler.opponent.discardAmount = 0;
                gameHandler.player.diamondsBeforeExhaustion--;
                gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
                break;
            case 3:
                gameHandler.DealCards(4);
                card.transform.SetParent(playArea.transform, false);
                gameHandler.player.diamondsBeforeExhaustion--;
                break;
            case 4:
                float chance = gameHandler.opponent.currentHP >= 75 ? 0.75f : 0.5f;
                if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 20 || gameHandler.opponent.cards == 0)
                {
                    card.transform.SetParent(playArea.transform, false);

                    yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                    gameHandler.opponent.Damage(20, DamageType.Unblockable);
                    gameHandler.player.diamondsBeforeExhaustion--;
                    break;
                }

                gameHandler.opponent.discardAmount = gameHandler.opponent.cards < 2 ? gameHandler.opponent.cards : 2;
                card.transform.SetParent(playArea.transform, false);
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
                    opponentCard.transform.SetParent(playArea.transform, false);
                }
                gameHandler.opponent.discardAmount = 0;
                gameHandler.player.diamondsBeforeExhaustion--;
                gameHandler.endTurnButton.GetComponent<Button>().interactable = true;
                break;
            case 5:
                gameHandler.DealCardsPlayer(1);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 6:
                gameHandler.DealCardsPlayer(1);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 7:
                gameHandler.DealCardsPlayer(1);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 8:
                gameHandler.DealCardsPlayer(1);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 9:
                gameHandler.player.Heal(10);
                gameHandler.opponent.Heal(10);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 10:
                gameHandler.player.Heal(20);
                gameHandler.opponent.Heal(20);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 11:
                gameHandler.opponent.Damage(20, DamageType.Fire);
                card.transform.SetParent(playArea.transform, false);
                break;
            case 12:
                chance = gameHandler.opponent.currentHP >= 75 ? 0.45f : 0.20f;
                if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 40 || gameHandler.opponent.cards == 0)
                {
                    card.transform.SetParent(playArea.transform, false);

                    yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                    gameHandler.opponent.Damage(40, DamageType.Fire);
                    gameHandler.player.diamondsBeforeExhaustion--;
                    break;
                }

                gameHandler.opponent.discardAmount = gameHandler.opponent.cards < 4 ? gameHandler.opponent.cards : 4;
                card.transform.SetParent(playArea.transform, false);
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
                    opponentCard.transform.SetParent(playArea.transform, false);
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
                card.transform.SetParent(playArea.transform, false);
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
                card.transform.SetParent(playArea.transform, false);
                gameHandler.player.discardAmount--;
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
        if (playArea.transform.childCount > 7)
        {
            Transform transform = playArea.transform.GetChild(0);
            GameObject gameObject = transform.gameObject;
            Destroy(gameObject);
        }
    }
    #endregion
}
