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
                                if (gameHandler.player.spadesBeforeExhaustion <= 0)
                                {
                                    Debug.Log("Player is exhausted! Cannot play more spades.");
                                    break;
                                }

                                Debug.Log("Player is now attacking the opponent.");
                                gameHandler.player.isAttacking = true;
                                gameHandler.player.spadesBeforeExhaustion--;
                                card.transform.SetParent(playArea.transform, false);

                                gameHandler.endTurnButton.GetComponent<Button>().interactable = false;
                                gameHandler.gambleButton.SetActive(true);
                                break;
                            case CardType.HEART:
                                if (gameHandler.player.heartsBeforeExhaustion <= 0)
                                {
                                    Debug.Log("Player is exhausted! Cannot play more hearts.");
                                    break;
                                }
                                if (gameHandler.player.currentHP >= gameHandler.player.maxHP)
                                {
                                    Debug.Log("Player health is full!");
                                    break;
                                }
                                Debug.Log("Player is attempting to heal.");
                                if (this.card.cardValue <= 6)
                                {
                                    gameHandler.player.Heal(5);
                                    gameHandler.player.heartsBeforeExhaustion--;
                                    card.transform.SetParent(playArea.transform, false);
                                }
                                if (this.card.cardValue >= 7 && this.card.cardValue <= 9 && gameHandler.player.heartsBeforeExhaustion >= 2)
                                {

                                    gameHandler.player.Heal(10);
                                    gameHandler.player.heartsBeforeExhaustion -= 2;
                                    card.transform.SetParent(playArea.transform, false);
                                }
                                if (this.card.cardValue >= 10 && this.card.cardValue <= 13 && gameHandler.player.heartsBeforeExhaustion == 3)
                                {
                                    gameHandler.player.Heal(20);
                                    gameHandler.player.heartsBeforeExhaustion -= 3;
                                    card.transform.SetParent(playArea.transform, false);
                                }
                                if (this.card.cardValue == 14 && gameHandler.player.heartsBeforeExhaustion == 3)
                                {
                                    gameHandler.player.Heal(40);
                                    gameHandler.player.heartsBeforeExhaustion -= 3;
                                    card.transform.SetParent(playArea.transform, false);
                                }
                                break;
                            case CardType.CLUB:
                                Debug.Log("Player is attempting to trade a CLUB.");
                                gameHandler.DealCardsPlayer(1);
                                card.transform.SetParent(playArea.transform, false);
                                break;
                            case CardType.DIAMOND:
                                if (this.card.cardValue < 5 || this.card.cardValue > 8)
                                {
                                    if (gameHandler.player.diamondsBeforeExhaustion <= 0)
                                    {
                                        Debug.Log("Player is exhausted! Cannot play more diamonds.");
                                        break;
                                    } 
                                }
                                Debug.Log("Player is attempting to use a DIAMOND.");
                                switch (this.card.cardValue)
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

                                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                        for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                                        {
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

                                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                        for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                                        {
                                            int value = 666;
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
                                break;
                        }
                    }
                    break;
                case GamePhase.PLAYERENDPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
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
                    }
                    break;
                case GamePhase.OPPONENTACTIONPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
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
    }
    public IEnumerator OpponentCardLogic()
    {
        for (int x = 0; x < gameHandler.opponent.cards; x++)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 1f));

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

        yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

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
                        break;
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
                        break;
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
                        break;
                    case 10:
                        if (gameHandler.opponent.currentHP > 0.5f * gameHandler.opponent.maxHP || gameHandler.player.currentHP < 0.4f * gameHandler.player.currentHP)
                        {
                            continue;
                        }
                        gameHandler.player.Heal(10);
                        gameHandler.opponent.Heal(10);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        break;
                    case 11:
                        gameHandler.player.Damage(20, DamageType.Fire);
                        selectedCard.transform.SetParent(gameHandler.playArea.transform, false);
                        selectedCard.GetComponent<Card>().ToggleCardVisibility();
                        break;
                    default:
                        Debug.Log("Not implemented yet. Skipping...");
                        break;
                }
            }
            if (gameHandler.opponent.diamondsBeforeExhaustion == 0)
            {
                break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

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
            if (gameHandler.opponent.spadesBeforeExhaustion == 0)
            {
                break;
            }
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 3f));
        
        if (gameHandler.opponent.currentHP != gameHandler.opponent.maxHP)
        {
            Debug.Log("Not implemented yet. Skipping...");
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

        gameHandler.EndOpponentTurn();
    }
    #endregion

    #region Other Functions
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
        Debug.Log("Player: " + gameHandler.player.currentHP);
        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);
        gameHandler.playerActionTooltip.text = "It is the opponent's Action Phase.";

        gameHandler.opponent.isAttacking = false;
        gameHandler.player.isAttacked = false;

        StartCoroutine(OpponentCardLogic());
    }
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
