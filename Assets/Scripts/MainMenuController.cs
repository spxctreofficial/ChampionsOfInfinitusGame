using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MainMenuController : MonoBehaviour {
	public static MainMenuController instance;

	public Canvas overlayCanvas;
	public GameObject mainPanel;
	public ShopPanel shopPanel;
	public Image logo;

	public GameObject confirmDialogPrefab;
	public GameObject miniConfirmDialogPrefab;
	
	private void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	private void Start() {
		overlayCanvas.gameObject.SetActive(true);
		mainPanel.AddComponent<CanvasGroup>();
		
		shopPanel.Setup();
	}

	/// <summary>
	/// PRESS TO START equivalent.
	/// </summary>
	public void Focus() {
		mainPanel.GetComponent<AudioLowPassFilter>().enabled = false;
		StartCoroutine(ShakeImage(logo.transform, 0.35f, 15f));
		AudioController.instance.Play("GlassBreak");
		Destroy(overlayCanvas.gameObject);
		LeanTween.scale(logo.GetComponent<RectTransform>(), new Vector3(1.2f, 1.2f, 1.2f), 0.1f).setEaseInOutQuad().setOnComplete(() => {
			LeanTween.scale(logo.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 1.5f).setEaseInOutQuad();
		});

	}
	/// <summary>
	/// Loads the Sandbox scene.
	/// </summary>
	public void LoadSandbox() {
		LeanTween.alphaCanvas(mainPanel.GetComponent<CanvasGroup>(), 0f, 1f).setOnComplete(() => {
			SceneManager.LoadScene("Sandbox");
		});
	}
	/// <summary>
	/// Quits the game.
	/// </summary>
	public void QuitGame() {
		var confirmDialog = ConfirmDialog.CreateNew_Mini("QUIT", "Are you sure you want to quit the game?", () => {
			ConfirmDialog.instance.Hide();
		}, () => {
			DataManager.instance.Save();
			Application.Quit();
		});
		confirmDialog.transform.SetParent(mainPanel.transform, false);
		confirmDialog.GetComponent<RectTransform>().localPosition = new Vector2(0, -270);
	}

	private IEnumerator ShakeImage(Transform transform, float duration, float magnitude) {
		var originalPos = transform.localPosition;

		for (float t = 0; t < 1; t += Time.deltaTime / duration) {
			var x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
			var y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
			var shake = new Vector3(x, y, originalPos.z);
			transform.localPosition = shake;
			yield return null;
		}
		transform.localPosition = originalPos;
	}
}
