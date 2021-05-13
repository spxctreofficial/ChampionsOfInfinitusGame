using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPanel : MonoBehaviour
{
	public ChampionController owner;

	private void Start()
	{
		gameObject.transform.localScale = new Vector3(0, 0, 0);
		LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutQuad);
	}

	public void Setup(ChampionController champion)
	{
		owner = champion;
	}

	public void ClosePanel()
	{
		LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.5f).setEase(LeanTweenType.easeInOutQuad).setDestroyOnComplete(true);
	}
}
