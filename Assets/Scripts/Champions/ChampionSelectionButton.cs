using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionSelectionButton : MonoBehaviour
{
	[HideInInspector]
	public Champion championComponent;

	private void Start()
	{
		StartCoroutine(Setup());
	}

	public void OnClick()
	{
		GameController.instance.playerChampion = championComponent;
	}

	private IEnumerator Setup()
	{
		yield return null;
		if (championComponent != null) gameObject.GetComponent<Image>().sprite = championComponent.avatar;
	}
}