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

        if (cardOfPlayer && gameHandler.phase == GamePhase.PLAYERACTIONPHASE)
        {
            if (gameHandler.player.isAttacking)
            {
                GameObject opponentCard = gameHandler.OpponentArea.transform.GetChild(0).gameObject;

                Debug.Log("Player is attacking the opponent with a card with a value of " + this.card.cardValue);
                gameHandler.opponent.isAttacked = true;
                card.transform.SetParent(PlayArea.transform, false);

                yield return new WaitForSeconds(Random.Range(0.2f, 3f));

                opponentCard.transform.SetParent(PlayArea.transform, false);

                if (this.card.cardValue > opponentCard.GetComponent<Card>().cardValue)
                {
                    gameHandler.opponent.currentHP -= gameHandler.player.attackDamage;
                }
                else if (this.card.cardValue < opponentCard.GetComponent<Card>().cardValue)
                {
                    gameHandler.player.currentHP -= gameHandler.opponent.attackDamage;
                }
                Debug.Log("Player: " + gameHandler.player.currentHP);
                Debug.Log("Opponent: " + gameHandler.opponent.currentHP);

                gameHandler.player.isAttacking = false;
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
                        gameHandler.player.spadesBeforeExhaustion -= 1;
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
                        gameHandler.player.heartsBeforeExhaustion -= 1;
                        card.transform.SetParent(PlayArea.transform, false);
                        break;
                    case CardType.CLUB:
                        Debug.Log("Player is attempting to trade a CLUB.");
                        gameHandler.DealCardsPlayer(1);
                        card.transform.SetParent(PlayArea.transform, false);
                        break;
                    case CardType.DIAMOND:
                        Debug.Log("Player is attempting to use a DIAMOND.");
                        switch (this.card.cardValue)
                        {
                            case 1:
                                gameHandler.DealCards(2);
                                card.transform.SetParent(PlayArea.transform, false);
                                break;
                            case 3:
                                gameHandler.DealCards(4);
                                card.transform.SetParent(PlayArea.transform, false);
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
        else
        {
            Debug.Log("You cannot do that!");
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
