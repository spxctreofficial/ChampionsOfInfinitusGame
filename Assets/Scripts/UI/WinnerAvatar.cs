using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WinnerAvatar : MonoBehaviour {
	private void Awake() {
		AudioController.instance.Play("GameEnd");
		LeanTween.delayedCall(1f, () => {
			LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.25f).setEaseInOutQuad().setOnComplete(() => {
				StartCoroutine(ShakeImage(0.25f, 10f));
				LeanTween.scale(gameObject, new Vector3(0.75f, 0.75f, 0.75f), 2.75f).setEaseInOutQuad();
			});
			LeanTween.rotate(gameObject, new Vector3(0, 0, 0), 0.25f).setEaseInOutQuad();
		});
	}

	private IEnumerator ShakeImage(float duration, float magnitude) {
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
