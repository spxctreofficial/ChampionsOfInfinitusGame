using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticController : MonoBehaviour
{
	public static StatisticController instance;
	public List<MatchStatistic> matchStatistics = new List<MatchStatistic>();

	public GameController.Gamemodes gamemode;
	public bool winState;
	public MatchStatistic playerChampionStatistic;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	public void StartTrackingStatistics()
	{
		foreach (ChampionController champion in GameController.instance.champions)
		{
			matchStatistics.Add(new MatchStatistic(champion.champion));
		}

		gamemode = GameController.instance.gamemodes;
		playerChampionStatistic = matchStatistics[0];
	}
	public void TrackRemainingStatistics(ChampionController champion)
	{
		var index = GameController.instance.champions.IndexOf(champion);
		var matchStatistic = matchStatistics[index];

		matchStatistic.remainingHP = champion.currentHP;

		if (champion == GameController.instance.champions[0]) winState = true;
	}

	[System.Obsolete("GetChampionStatistics() has been deprecated." +
	                 " For more convenience and cleaner code, use method GetMatchStatistics() of the ChampionController class instead.")]
	public MatchStatistic GetChampionStatistics(ChampionController champion)
	{
		var index = GameController.instance.champions.IndexOf(champion);
		return matchStatistics[index];
	}
}
