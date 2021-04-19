using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicHandler : MonoBehaviour
{
    #region Variables
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    public GameObject PlayerArea;
    public GameObject OpponentArea;
    public GameObject PlayArea;
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
            gameObject.transform.SetParent(PlayerArea.transform, false);
        }
    }

    #region Card Functions
    public IEnumerator CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == PlayerArea;

        if (cardOfPlayer)
        {
            switch (gameHandler.phase)
            {
                case GamePhase.PLAYERACTIONPHASE:
                    if (gameHandler.player.isAttacking)
                    {
                        int value = -1;
                        int siblingIndex = 0;
                        for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
                        {
                            if (value < gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                            {
                                value = gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                                siblingIndex = gameHandler.OpponentArea.transform.GetChild(x).GetSiblingIndex();
                            }
                        }
                        GameObject opponentCard = gameHandler.opponent.cards == 0 ? Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity) : gameHandler.OpponentArea.transform.GetChild(siblingIndex).gameObject;

                        Debug.Log("Player is attacking the opponent with a card with a value of " + this.card.cardValue);
                        gameHandler.opponent.isAttacked = true;
                        card.transform.SetParent(PlayArea.transform, false);
                        this.card.ToggleCardVisibility();

                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                        opponentCard.transform.SetParent(PlayArea.transform, false);
                        this.card.ToggleCardVisibility();

                        if (this.card.cardValue > opponentCard.GetComponent<Card>().cardValue)
                        {
                            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType);
                        }
                        else if (this.card.cardValue < opponentCard.GetComponent<Card>().cardValue)
                        {
                            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType);
                        }
                        Debug.Log("Player: " + gameHandler.player.currentHP);
                        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);

                        gameHandler.player.isAttacking = false;
                        gameHandler.opponent.isAttacked = false;
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
                                card.transform.SetParent(PlayArea.transform, false);
                                break;
                            case CardType.HEART:
                                if (gameHandler.player.heartsBeforeExhaustion <= 0)
                                {
                                    Debug.Log("Player is exhausted! Cannot play more hearts.");
                                    break;
                                }
                                else if (gameHandler.player.currentHP >= gameHandler.player.maxHP)
                                {
                                    Debug.Log("Player health is full!");
                                    break;
                                }
                                Debug.Log("Player is attempting to heal.");
                                if (this.card.cardValue <= 6)
                                {
                                    gameHandler.player.Heal(5);
                                    gameHandler.player.heartsBeforeExhaustion--;
                                }
                                if (this.card.cardValue >= 7 && this.card.cardValue <= 9 && gameHandler.player.heartsBeforeExhaustion >= 2)
                                {
                                    gameHandler.player.Heal(10);
                                    gameHandler.player.heartsBeforeExhaustion -= 2;
                                }
                                if (this.card.cardValue >= 10 && this.card.cardValue <= 13)
                                {
                                    gameHandler.player.Heal(20);
                                    gameHandler.player.heartsBeforeExhaustion -= 3;
                                }
                                if (this.card.cardValue == 14)
                                {
                                    gameHandler.player.Heal(40);
                                    gameHandler.player.heartsBeforeExhaustion -= 3;
                                }
                                card.transform.SetParent(PlayArea.transform, false);
                                break;
                            case CardType.CLUB:
                                Debug.Log("Player is attempting to trade a CLUB.");
                                gameHandler.DealCardsPlayer(1);
                                card.transform.SetParent(PlayArea.transform, false);
                                break;
                            case CardType.DIAMOND:
                                if (this.card.cardValue < 5 && this.card.cardValue > 8)
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
                                        card.transform.SetParent(PlayArea.transform, false);
                                        gameHandler.player.diamondsBeforeExhaustion--;
                                        break;
                                    case 2:
                                        if (gameHandler.opponent.cards == 0)
                                        {
                                            card.transform.SetParent(PlayArea.transform, false);
                                            gameHandler.player.diamondsBeforeExhaustion--;
                                            break;
                                        }

                                        gameHandler.opponent.discardAmount = 1;
                                        card.transform.SetParent(PlayArea.transform, false);

                                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                        for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                                        {
                                            int value = 999;
                                            int siblingIndex = 0;
                                            for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
                                            {
                                                if (value > gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                                                {
                                                    value = gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                                                    siblingIndex = gameHandler.OpponentArea.transform.GetChild(x).GetSiblingIndex();
                                                }
                                            }
                                            GameObject opponentCard = gameHandler.OpponentArea.transform.GetChild(siblingIndex).gameObject;
                                            opponentCard.transform.SetParent(PlayArea.transform, false);
                                        }
                                        gameHandler.opponent.discardAmount = 0;
                                        gameHandler.player.diamondsBeforeExhaustion--;
                                        break;
                                    case 3:
                                        gameHandler.DealCards(4);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        gameHandler.player.diamondsBeforeExhaustion--;
                                        break;
                                    case 4:
                                        float chance = gameHandler.opponent.currentHP >= 75 ? 0.75f : 0.5f;
                                        if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 20 || gameHandler.opponent.cards == 0)
                                        {
                                            card.transform.SetParent(PlayArea.transform, false);

                                            yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                            gameHandler.opponent.Damage(20, DamageType.Unblockable);
                                            gameHandler.player.diamondsBeforeExhaustion--;
                                            break;
                                        }

                                        gameHandler.opponent.discardAmount = 2;
                                        card.transform.SetParent(PlayArea.transform, false);

                                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                        for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                                        {
                                            int value = 999;
                                            int siblingIndex = 0;
                                            for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
                                            {
                                                if (value > gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                                                {
                                                    value = gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                                                    siblingIndex = gameHandler.OpponentArea.transform.GetChild(x).GetSiblingIndex();
                                                }
                                            }
                                            GameObject opponentCard = gameHandler.OpponentArea.transform.GetChild(siblingIndex).gameObject;
                                            opponentCard.transform.SetParent(PlayArea.transform, false);
                                        }
                                        gameHandler.opponent.discardAmount = 0;
                                        gameHandler.player.diamondsBeforeExhaustion--;
                                        break;
                                    case 5:
                                        gameHandler.DealCardsPlayer(1);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 6:
                                        gameHandler.DealCardsPlayer(1);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 7:
                                        gameHandler.DealCardsPlayer(1);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 8:
                                        gameHandler.DealCardsPlayer(1);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 9:
                                        gameHandler.player.Heal(10);
                                        gameHandler.opponent.Heal(10);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 10:
                                        gameHandler.player.Heal(20);
                                        gameHandler.opponent.Heal(20);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 11:
                                        gameHandler.opponent.Damage(20, DamageType.Fire);
                                        card.transform.SetParent(PlayArea.transform, false);
                                        break;
                                    case 12:
                                        chance = gameHandler.opponent.currentHP >= 75 ? 0.45f : 0.20f;
                                        if (Random.Range(0f, 1f) > chance && gameHandler.opponent.currentHP > 40 || gameHandler.opponent.cards == 0)
                                        {
                                            card.transform.SetParent(PlayArea.transform, false);

                                            yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                            gameHandler.opponent.Damage(40, DamageType.Fire);
                                            gameHandler.player.diamondsBeforeExhaustion--;
                                            break;
                                        }

                                        gameHandler.opponent.discardAmount = 4;
                                        card.transform.SetParent(PlayArea.transform, false);

                                        yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                                        for (int i = 0; i < gameHandler.opponent.discardAmount; i++)
                                        {
                                            int value = 666;
                                            int siblingIndex = 0;
                                            for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
                                            {
                                                if (value > gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                                                {
                                                    value = gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                                                    siblingIndex = gameHandler.OpponentArea.transform.GetChild(x).GetSiblingIndex();
                                                }
                                            }
                                            GameObject opponentCard = gameHandler.OpponentArea.transform.GetChild(siblingIndex).gameObject;
                                            opponentCard.transform.SetParent(PlayArea.transform, false);
                                        }
                                        gameHandler.opponent.discardAmount = 0;
                                        gameHandler.player.diamondsBeforeExhaustion--;
                                        break;
                                }
                                break;
                        }
                    }
                    break;
                case GamePhase.PLAYERENDPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
                        card.transform.SetParent(PlayArea.transform, false);
                        gameHandler.player.discardAmount--;
                        gameHandler.PlayerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                    }
                    break;
                case GamePhase.OPPONENTACTIONPHASE:
                    if (gameHandler.player.discardAmount > 0)
                    {
                        card.transform.SetParent(PlayArea.transform, false);
                        gameHandler.player.discardAmount--;
                        if (gameHandler.player.discardAmount == 0)
                        {
                            gameHandler.PlayerActionTooltip.text = "It is the opponent's Action Phase.";
                            StartCoroutine(OpponentCardLogic());
                        }
                        else
                        {
                            gameHandler.PlayerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                        }
                        break;
                    }

                    if (gameHandler.opponent.isAttacking || gameHandler.player.isAttacked)
                    {
                        card.transform.SetParent(PlayArea.transform, false);
                        GameObject attackingCard = gameHandler.PlayArea.transform.GetChild(this.card.transform.GetSiblingIndex() - 1).gameObject;
                        attackingCard.GetComponent<Card>().ToggleCardVisibility();

                        if (this.card.cardValue > attackingCard.GetComponent<Card>().cardValue)
                        {
                            gameHandler.opponent.Damage(gameHandler.player.attackDamage, gameHandler.player.damageType);
                        }
                        else if (this.card.cardValue < attackingCard.GetComponent<Card>().cardValue)
                        {
                            gameHandler.player.Damage(gameHandler.opponent.attackDamage, gameHandler.opponent.damageType);
                        }
                        Debug.Log("Player: " + gameHandler.player.currentHP);
                        Debug.Log("Opponent: " + gameHandler.opponent.currentHP);

                        gameHandler.opponent.isAttacking = false;
                        gameHandler.player.isAttacked = false;

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
    }
    public IEnumerator OpponentCardLogic()
    {
        for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 1f));

            if (gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.CLUB)
            {
                GameObject selectedCard = gameHandler.OpponentArea.transform.GetChild(x).gameObject;
                selectedCard.transform.SetParent(gameHandler.PlayArea.transform, false);
                gameHandler.DealCardsOpponent(1);
            }
        }

        yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));

        for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
        {
            if (gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.DIAMOND && gameHandler.opponent.diamondsBeforeExhaustion != 0)
            {
                GameObject selectedCard = gameHandler.OpponentArea.transform.GetChild(x).gameObject;
                switch (selectedCard.GetComponent<Card>().cardValue)
                {
                    case 1:
                        gameHandler.DealCards(2);
                        selectedCard.transform.SetParent(gameHandler.PlayArea.transform, false);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
                        break;
                    case 2:
                        if (gameHandler.player.cards != 0)
                        {
                            gameHandler.player.discardAmount = 1;
                            selectedCard.transform.SetParent(PlayArea.transform, false);
                            gameHandler.PlayerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                            gameHandler.opponent.diamondsBeforeExhaustion--;
                            yield break;
                        }
                        break;
                    case 3:
                        gameHandler.DealCards(4);
                        selectedCard.transform.SetParent(gameHandler.PlayArea.transform, false);
                        gameHandler.opponent.diamondsBeforeExhaustion--;
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

        for (int x = 0; x < gameHandler.OpponentArea.transform.childCount; x++)
        {
            if (gameHandler.OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardType == CardType.SPADE && gameHandler.opponent.spadesBeforeExhaustion != 0)
            {
                GameObject selectedCard = gameHandler.OpponentArea.transform.GetChild(x).gameObject;
                GameObject attackingCard = null;
                Card selectedCardComponent = selectedCard.GetComponent<Card>();

                float f = gameHandler.player.currentHP <= gameHandler.player.maxHP * 0.25f ? 0.35f : 0.10f;
                if (selectedCardComponent.cardValue >= 11 && Random.Range(0f, 1f) <= f)
                {
                    continue;
                }

                selectedCard.transform.SetParent(gameHandler.PlayArea.transform, false);
                gameHandler.opponent.isAttacking = true;
                gameHandler.opponent.spadesBeforeExhaustion--;

                int value = -1;
                if (gameHandler.opponent.cards == 0)
                {
                    attackingCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                }
                else
                {
                    for (int y = 0; y < gameHandler.OpponentArea.transform.childCount; y++)
                    {
                        if (value < gameHandler.OpponentArea.transform.GetChild(y).gameObject.GetComponent<Card>().cardValue)
                        {
                            attackingCard = gameHandler.OpponentArea.transform.GetChild(y).gameObject;
                            value = attackingCard.GetComponent<Card>().cardValue;
                        }
                    }
                }

                attackingCard.transform.SetParent(gameHandler.PlayArea.transform, false);
                attackingCard.GetComponent<Card>().ToggleCardVisibility();
                gameHandler.player.isAttacked = true;

                gameHandler.PlayerActionTooltip.text = "The opponent is attacking. Defend with a card.";
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
    private void PurgePlayArea()
    {
        if (PlayArea.transform.childCount > 7)
        {
            Transform transform = PlayArea.transform.GetChild(0);
            GameObject gameObject = transform.gameObject;
            Object.Destroy(gameObject);
        }
    }
    #endregion
}
