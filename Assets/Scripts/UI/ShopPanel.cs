using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
	public void Show() {
		LeanTween.move(MainMenuController.instance.mainPanel.GetComponent<RectTransform>(), new Vector2(-1920, 0), 1f).setEaseInOutQuad();
		LeanTween.move(gameObject.GetComponent<RectTransform>(), new Vector2(0, 0), 1f).setEaseInOutQuad();
	}
}
