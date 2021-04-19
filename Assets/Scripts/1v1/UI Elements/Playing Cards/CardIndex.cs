using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardIndex : MonoBehaviour
{
    public List<GameObject> playingCards = new List<GameObject>();

    // SPADES
    public GameObject AceOfSpades;
    public GameObject KingOfSpades;
    public GameObject QueenOfSpades;
    public GameObject JackOfSpades;
    public GameObject TenOfSpades;
    public GameObject NineOfSpades;
    public GameObject EightOfSpades;
    public GameObject SevenOfSpades;
    public GameObject SixOfSpades;
    public GameObject FiveOfSpades;
    public GameObject FourOfSpades;
    public GameObject ThreeOfSpades;
    public GameObject TwoOfSpades;

    // HEARTS
    public GameObject AceOfHearts;
    public GameObject KingOfHearts;
    public GameObject QueenOfHearts;
    public GameObject JackOfHearts;
    public GameObject TenOfHearts;
    public GameObject NineOfHearts;
    public GameObject EightOfHearts;
    public GameObject SevenOfHearts;
    public GameObject SixOfHearts;
    public GameObject FiveOfHearts;
    public GameObject FourOfHearts;
    public GameObject ThreeOfHearts;
    public GameObject TwoOfHearts;

    // CLUBS
    public GameObject AceOfClubs;
    public GameObject KingOfClubs;
    public GameObject QueenOfClubs;
    public GameObject JackOfClubs;
    public GameObject TenOfClubs;
    public GameObject NineOfClubs;
    public GameObject EightOfClubs;
    public GameObject SevenOfClubs;
    public GameObject SixOfClubs;
    public GameObject FiveOfClubs;
    public GameObject FourOfClubs;
    public GameObject ThreeOfClubs;
    public GameObject TwoOfClubs;

    // DIAMONDS
    public GameObject AceOfDiamonds;
    public GameObject KingOfDiamonds;
    public GameObject QueenOfDiamonds;
    public GameObject JackOfDiamonds;
    public GameObject TenOfDiamonds;
    public GameObject NineOfDiamonds;
    public GameObject EightOfDiamonds;
    public GameObject SevenOfDiamonds;
    public GameObject SixOfDiamonds;
    public GameObject FiveOfDiamonds;
    public GameObject FourOfDiamonds;
    public GameObject ThreeOfDiamonds;
    public GameObject TwoOfDiamonds;

    public void PopulatePlayingCardsList()
    {
        // Spades
        playingCards.Add(AceOfSpades);
        playingCards.Add(KingOfSpades);
        playingCards.Add(QueenOfSpades);
        playingCards.Add(JackOfSpades);
        playingCards.Add(TenOfSpades);
        playingCards.Add(NineOfSpades);
        playingCards.Add(EightOfSpades);
        playingCards.Add(SevenOfSpades);
        playingCards.Add(SixOfSpades);
        playingCards.Add(FiveOfSpades);
        playingCards.Add(FourOfSpades);
        playingCards.Add(ThreeOfSpades);
        playingCards.Add(TwoOfSpades);

        // Hearts
        playingCards.Add(AceOfHearts);
        playingCards.Add(KingOfHearts);
        playingCards.Add(QueenOfHearts);
        playingCards.Add(JackOfHearts);
        playingCards.Add(TenOfHearts);
        playingCards.Add(NineOfHearts);
        playingCards.Add(EightOfHearts);
        playingCards.Add(SevenOfHearts);
        playingCards.Add(SixOfHearts);
        playingCards.Add(FiveOfHearts);
        playingCards.Add(FourOfHearts);
        playingCards.Add(ThreeOfHearts);
        playingCards.Add(TwoOfHearts);

        // Clubs
        playingCards.Add(AceOfClubs);
        playingCards.Add(KingOfClubs);
        playingCards.Add(QueenOfClubs);
        playingCards.Add(JackOfClubs);
        playingCards.Add(TenOfClubs);
        playingCards.Add(NineOfClubs);
        playingCards.Add(EightOfClubs);
        playingCards.Add(SevenOfClubs);
        playingCards.Add(SixOfClubs);
        playingCards.Add(FiveOfClubs);
        playingCards.Add(FourOfClubs);
        playingCards.Add(ThreeOfClubs);
        playingCards.Add(TwoOfClubs);

        // Diamonds
        playingCards.Add(AceOfDiamonds);
        playingCards.Add(KingOfDiamonds);
        playingCards.Add(QueenOfDiamonds);
        playingCards.Add(JackOfDiamonds);
        playingCards.Add(TenOfDiamonds);
        playingCards.Add(NineOfDiamonds);
        playingCards.Add(EightOfDiamonds);
        playingCards.Add(SevenOfDiamonds);
        playingCards.Add(SixOfDiamonds);
        playingCards.Add(FiveOfDiamonds);
        playingCards.Add(FourOfDiamonds);
        playingCards.Add(ThreeOfDiamonds);
        playingCards.Add(TwoOfDiamonds);
    }
}
