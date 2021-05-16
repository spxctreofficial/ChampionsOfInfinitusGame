using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MainMenuController : MonoBehaviour
{
	public static MainMenuController instance;

	public Canvas overlayCanvas;
	public GameObject mainPanel;
	public Image logo;
	
	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
		}
	}
	private void Start()
	{
		mainPanel.AddComponent<CanvasGroup>();
	}

	public void Focus()
	{
		mainPanel.GetComponent<AudioLowPassFilter>().enabled = false;
		StartCoroutine(ShakeImage(logo.transform, 0.35f, 15f));
		AudioController.instance.Play("GlassBreak");
		Destroy(overlayCanvas.gameObject);
		LeanTween.scale(logo.gameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.1f).setEaseInOutQuad().setOnComplete(() => {
			LeanTween.scale(logo.gameObject, new Vector3(1f, 1f, 1f), 1.5f).setEaseInOutQuad();
		});

	}
	public void LoadSandbox()
	{
		LeanTween.alphaCanvas(mainPanel.GetComponent<CanvasGroup>(), 0f, 1f).setOnComplete(() => {
			SceneManager.LoadScene("Sandbox");
		});
	}
	public void QuitGame()
	{
		Application.Quit();
	}
	
	private IEnumerator ShakeImage(Transform transform, float duration, float magnitude)
	{
		var originalPos = transform.localPosition;

		for (float t = 0; t < 1; t += Time.deltaTime / duration)
		{
			var x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
			var y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
			var shake = new Vector3(x, y, originalPos.z);
			transform.localPosition = shake;
			yield return null;
		}
		transform.localPosition = originalPos;
	}
}
