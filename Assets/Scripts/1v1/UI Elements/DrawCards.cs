using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCards : MonoBehaviour
{
    public GameHandler gameHandler;
    public CardIndex cardIndex;
    public void onClick()
    {
        GameObject playerCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector3(0, 0, 0), Quaternion.identity);
        playerCard.transform.SetParent(gameHandler.playerArea.transform, false);
    }
}
