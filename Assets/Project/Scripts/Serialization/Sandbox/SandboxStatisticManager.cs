using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxStatisticManager : StatisticManager
{
	public static new SandboxStatisticManager instance;

	protected override void Awake()
	{
		base.Awake();

		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
