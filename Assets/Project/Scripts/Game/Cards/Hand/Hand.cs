using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour {
    public HandViewport handViewport;

    public ChampionController owner;

    public Deck deck;

    public List<Card> cards = new List<Card>();
    public Queue<Card> queued = new Queue<Card>();

    private void Start() {
        deck = new Deck(PrefabManager.instance.defaultDecks.defaultDeck);
    }

    /// <summary>
    /// Sets `championController` as the owner of this hand.
    /// </summary>
    /// <param name="championController"></param>
    public void SetOwner(ChampionController championController) {
        owner = championController;
        championController.hand = this;

        name = owner.champion.championName + "'s Hand";

        switch (owner.champion.championClass) {
            case Champion.Class.Warrior:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.warriorDeck);
                break;
            case Champion.Class.Berserker:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.berserkerDeck);
                break;
            case Champion.Class.Mage:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.mageDeck);
                break;
            case Champion.Class.Warlock:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.warlockDeck);
                break;
            case Champion.Class.Priest:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.priestDeck);
                break;
            case Champion.Class.Rogue:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.rogueDeck);
                break;
            case Champion.Class.Ronin:
                deck = deck.Indoctrinate(PrefabManager.instance.defaultDecks.roninDeck);
                break;
            default:
                break;
        }

        deck = deck.Indoctrinate(owner.champion.extendedDeck);
    }

    #region Get Cards Functions

    /// <summary>
    /// Returns the amount of cards that this hand owns.
    /// </summary>
    /// <returns></returns>
    public int GetCardCount() {
        int cardCount = 0;

        foreach (Card card in cards) {
            if (card.owner is null) continue;
            cardCount++;
        }

        return cardCount;
    }

    public Card GetDiscard() {
        Card card = null;
        foreach (Card selectedCard in cards) {
            if (card is null) {
                card = selectedCard;
                continue;
            }

            if (selectedCard.cardData.cardImportanceFactor > card.cardData.cardImportanceFactor ||
                selectedCard.cardData.cardImportanceFactor == card.cardData.cardImportanceFactor && Random.Range(0f, 1f) < 0.5f ||
                GetDefenseCards().Contains(selectedCard) && GetDefenseCards().Length > 2) {
                card = selectedCard;
            }
        }

        return card;
    }
    public Card[] GetDiscardArray(int discardAmount) {
        List<Card> discardList = new List<Card>();
        for (int discarded = 0; discarded < discardAmount; discarded++) {
            Card discard = GetDiscard();
            cards.Remove(discard);
            discard.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
            discard.discardFeed.text = "DISCARDED";
            discardList.Add(discard);
        }

        return discardList.ToArray();
    }
    public Card[] GetDefenseCards() {
        List<Card> cards = new List<Card>();

        foreach (Card card in this.cards) {
            switch (card.cardData.cardFunctions.primaryFunction) {
                case "block":
                case "parry":
                    cards.Add(card);
                    break;
            }
        }

        return cards.ToArray();
    }
    public Card[] GetWeaponCards() {
        List<Card> cards = new List<Card>();

        foreach (Card card in this.cards) {
            if (card.cardData is WeaponCardData) {
                cards.Add(card);
            }
        }

        return cards.ToArray();
    }
    public Card GetPlayableWeaponCard() {
        Card selectedCard = null;

        foreach (Card card in this.cards) {
            if (card.cardData is not WeaponCardData) continue;
            if (card.cardData.staminaRequirement > owner.currentStamina) continue;
            if (card is null) {
                selectedCard = card;
                continue;
            }

            if (((WeaponCardData)card.cardData).weaponScriptableObject.attackDamage > ((WeaponCardData)card.cardData).weaponScriptableObject.attackDamage) {
                selectedCard = card;
                continue;
            }
        }

        if (selectedCard is null) Debug.Log("No playable weapon was found! Returning a null.");
        return selectedCard;
    }

    #endregion

    #region Draw Functions

    /// <summary>
    /// Deals an amount of randomly generated cards to this hand, with additional parameters for animation and fine control.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="cardColor"></param>
    /// <param name="excludeDraw"></param>
    /// <param name="flip"></param>
    /// <param name="animate"></param>
    /// <param name="abilityCheck"></param>
    /// <returns></returns>
    public IEnumerator Deal(int amount = 4, Deck deck = null, bool excludeDraw = false, bool flip = false, bool animate = true, bool abilityCheck = true) {
        if (deck is null) deck = this.deck;

        for (int i = 0; i < amount; i++) {
            yield return StartCoroutine(Deal(deck.Retrieve(owner), flip, animate, abilityCheck));

            yield return new WaitForSeconds(0.25f);
        }
    }
    public IEnumerator Deal(CardData cardData, bool flip = false, bool animate = true, bool abilityCheck = true) {
        AudioManager.instance.Play("flip");

        Card card = Instantiate(PrefabManager.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
        card.cardData = cardData;

        handViewport.Add(card);

        card.owner = owner;
        card.owner.matchStatistic.totalCardsDealt++;
        cards.Add(card);

        if (flip) card.Flip();
        if (animate) {
            card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            card.GetComponent<SmartHover>().ScaleDown();
        }
        if (abilityCheck) {
            foreach (ChampionController selectedChampion in GameManager.instance.champions) {
                foreach (Ability ability in selectedChampion.abilities) {
                    yield return StartCoroutine(ability.OnDeal(card, owner));
                }
            }
        }
    }

    #endregion

    #region Discard / Use Card Functions

    /// <summary>
    /// Discards a specified card from this hand.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="flip"></param>
    /// <param name="animate"></param>
    /// <param name="abilityCheck"></param>
    /// <returns></returns>
    public IEnumerator Discard(Card card, bool flip = false, bool animate = true, bool abilityCheck = true) {
        AudioManager.instance.Play("flip");

        card.transform.SetParent(GameManager.instance.discardArea.transform, false);
        handViewport.Remove(card);
        GameManager.instance.UpdateDiscardArea();

        if (card.owner is { }) {
            card.owner.matchStatistic.totalCardsDiscarded++;
            card.caption.text = "Played by " + card.owner.champion.championName;
        }
        card.owner = null;
        cards.Remove(card);

        // Extra parameters.
        if (flip) card.Flip();
        if (animate) {
            card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            card.GetComponent<SmartHover>().ScaleDown();
        }
        // if (abilityCheck) {
        //     foreach (ChampionController selectedChampion in GameManager.instance.champions) {
        //         foreach (Ability ability in selectedChampion.abilities) {
        //             // ABILITY CHECK HERE
        //         }
        //     }
        // }
        yield break;
    }
    public IEnumerator Discard(Card[] cards, bool flip = false, bool animate = true, bool abilityCheck = false) {
        if (cards.Length == 0) yield break;

        foreach (Card card in cards) {
            yield return StartCoroutine(Discard(card, flip, animate, abilityCheck));
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator UseCard(Card card, bool flip = false, bool animate = true, bool abilityCheck = false) {
        owner.currentStamina = Mathf.Max(owner.currentStamina - card.EffectiveStaminaRequirement, 0);
        yield return StartCoroutine(Discard(card, flip, animate, abilityCheck));
    }

    #endregion
}
