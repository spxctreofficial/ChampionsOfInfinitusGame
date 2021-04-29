using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
	public ChampionController owner;

	public void SetOwner(ChampionController championController)
	{
		this.owner = championController;
		championController.hand = this;
	}
}
