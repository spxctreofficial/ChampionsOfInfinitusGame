using System.Collections;
using TMPro;
using UnityEngine;

public class DiscardManager : MonoBehaviour {
    public static DiscardManager instance;
    public ChampionController discarder;

    private void Awake() {
        if (instance is null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public static DiscardManager Create(ChampionController discarder) {
        DiscardManager discardManager = new GameObject("DiscardManager").AddComponent<DiscardManager>();
        discardManager.discarder = discarder;
        return discardManager;
    }

    public IEnumerator Initialize() {
        if (discarder is null) {
            Debug.LogError("DiscardManager can't operate without a valid discarder!");
            yield break;
        }

        switch (discarder.isPlayer) {
            case true:
                GameManager.instance.playerActionTooltip.text = discarder.discardAmount != 0 ? "Please discard " + discarder.discardAmount + "." : GameManager.instance.playerActionTooltip.text;

                yield return new WaitUntil(() => discarder.discardAmount == 0);
                break;
            case false:
                yield return StartCoroutine(discarder.hand.Discard(discarder.hand.GetDiscardArray(discarder.discardAmount)));
                discarder.discardAmount = 0;
                break;
        }
        yield break;
    }

    public IEnumerator PlayerDiscardOperator(Card card) {
        if (discarder is null) {
            Debug.LogError("DiscardManager can't operate without a valid discarder!");
            yield break;
        }
        if (!discarder.isPlayer) {
            Debug.Log("DiscardManager.PlayerDiscardOperator() can't operate without a valid discarder!");
            yield break;
        }
        if (discarder.discardAmount <= 0) {
            Debug.Log("DiscardManager.PlayerDiscardOperator() can't operate without a valid discardAmount!");
            yield break;
        }

        discarder.discardAmount--;
        card.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
        card.discardFeed.text = "DISCARDED";

        if (discarder.discardAmount != 0) {
            GameManager.instance.playerActionTooltip.text = "Please discard " + discarder.discardAmount + ".";
        }
        else {
            GameManager.instance.playerActionTooltip.text = string.Empty;
        }
        yield return StartCoroutine(discarder.hand.Discard(card));
    }

    public void Remove() {
        instance = null;
        Destroy(gameObject);
    }
}
