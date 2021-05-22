using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
	public void Show() {
		LeanTween.move(MainMenuController.instance.mainPanel, new Vector2(-1920, 0), 1f);
		LeanTween.move(gameObject, new Vector2(0, 0), 1f);
	}
}
