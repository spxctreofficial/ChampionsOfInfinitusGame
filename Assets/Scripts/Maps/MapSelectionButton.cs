using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionButton : MonoBehaviour
{
	[HideInInspector]
	public Map mapComponent;

	private void Start()
	{
		StartCoroutine(Setup());
	}

	public void OnClick()
	{
		GameController.instance.currentMap = mapComponent;
	}

	private IEnumerator Setup()
	{
		yield return null;
		if (mapComponent != null) gameObject.GetComponent<Image>().sprite = mapComponent.mapBackground;
	}
}