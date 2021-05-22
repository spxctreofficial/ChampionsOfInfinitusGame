using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPanel : MonoBehaviour {
	[HideInInspector]
	public ChampionController owner;
	public GameObject panel;

	private void Awake() {
		gameObject.transform.localPosition = new Vector2(3000, 0);
	}

	public void Setup(ChampionController champion) {
		owner = champion;
		champion.abilityPanel = this;

		foreach (var ability in owner.abilities) {
			var abilityController = Instantiate(GameController.instance.abilityTemplate, new Vector2(0, 0), Quaternion.identity).GetComponent<AbilityController>();
			abilityController.transform.SetParent(panel.transform, false);
			abilityController.Setup(champion, ability);
		}
	}

	public void OpenPanel() {
		gameObject.transform.localPosition = new Vector3(0, 0, 0);
		gameObject.transform.localScale = new Vector3(0, 0, 0);
		LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutQuad);
	}
	public void ClosePanel() {
		LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => MoveOutOfScreen());
	}

	private void MoveOutOfScreen() {
		gameObject.transform.localPosition = new Vector2(3000, 0);
	}
}
