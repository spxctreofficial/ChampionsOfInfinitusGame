using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMechanics : MonoBehaviour
{
    public CardIndex cardIndex;

    public void clickSpade()
    {
        Debug.Log("Spade selected.");
    }
    public void clickSmallHeart()
    {
        Debug.Log("Small heart selected.");
    }
    public void clickMediumHeart()
    {
        Debug.Log("Medium heart selected.");
    }
    public void clickLargeHeart()
    {
        Debug.Log("Large heart selected.");
    }
    public void clickAceOfHeart()
    {
        Debug.Log("Ace of Hearts selected.");
    }
    public void clickClub()
    {
        Debug.Log("Club selected.");
    }
    public void clickDiamond()
    {
        Debug.Log("Diamond selected.");
    }

    //Deprecated
    public void clickCard1()
    {
        Debug.Log("hi card 1");
    }
    public void clickCard2()
    {
        Debug.Log("hi card 2");
    }
}
