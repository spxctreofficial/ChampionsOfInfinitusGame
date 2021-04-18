using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicHandler : MonoBehaviour
{
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    public GameObject PlayerArea;
    public GameObject OpponentArea;
    public GameObject PlayArea;

    Card card;
    bool cardOfPlayer;

    void Update()
    {
        PurgePlayArea();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            card.ToggleCardVisibility();
        }
    }

    public IEnumerator CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == PlayerArea;

        if (cardOfPlayer)
        {
            if (gameHandler.phase == GamePhase.PLAYERACTIONPHASE)
            {
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
                    GameObject opponentCard = gameHandler.OpponentArea.transform.GetChild(siblingIndex).gameObject;

                    Debug.Log("Player is attacking the opponent with a card with a value of " + this.card.cardValue);
                    gameHandler.opponent.isAttacked = true;
                    card.transform.SetParent(PlayArea.transform, false);

                    yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                    opponentCard.transform.SetParent(PlayArea.transform, false);

                    if (this.card.cardValue > opponentCard.GetComponent<Card>().cardValue)
                    {
                        gameHandler.opponent.Damage(gameHandler.player.attackDamage);
                    }
                    else if (this.card.cardValue < opponentCard.GetComponent<Card>().cardValue)
                    {
                        gameHandler.player.Damage(gameHandler.opponent.attackDamage);
                    }
                    Debug.Log("Player: " + gameHandler.player.currentHP);
                    Debug.Log("Opponent: " + gameHandler.opponent.currentHP);

                    gameHandler.player.isAttacking = false;
                    gameHandler.player.isAttacked = false;
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
                            if (gameHandler.player.diamondsBeforeExhaustion <= 0)
                            {
                                Debug.Log("Player is exhausted! Cannot play more diamonds.");
                                break;
                            }
                            Debug.Log("Player is attempting to use a DIAMOND.");
                            switch (this.card.cardValue)
                            {
                                case 1:
                                    gameHandler.DealCards(2);
                                    card.transform.SetParent(PlayArea.transform, false);
                                    gameHandler.player.diamondsBeforeExhaustion--;
                                    break;
                                case 3:
                                    gameHandler.DealCards(4);
                                    card.transform.SetParent(PlayArea.transform, false);
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
                            }
                            break;
                    }
                }
            }
            else if (gameHandler.phase == GamePhase.PLAYERENDPHASE)
            {
                if (gameHandler.player.discardAmount > 0)
                {
                    card.transform.SetParent(PlayArea.transform, false);
                    gameHandler.player.discardAmount--;
                    gameHandler.PlayerActionTooltip.text = "Please discard " + gameHandler.player.discardAmount + ".";
                }
            }
            else
            {
                Debug.Log("It's not the player's turn!");
            }
        }
        else
        {
            Debug.Log("This is not the player's card!");
        }
    }

    private void PurgePlayArea()
    {
        if (PlayArea.transform.childCount > 7)
        {
            Transform transform = PlayArea.transform.GetChild(0);
            GameObject gameObject = transform.gameObject;
            Object.Destroy(gameObject);
        }
    }
}
