using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class TutorialGameController : GameController {
	public static new TutorialGameController instance;

	[HideInInspector]
	public int tutorialProgress;

	public List<DialogueSession> dialogueSessions = new List<DialogueSession>();
	public Queue<DialogueSession> dialogueSessionsQueue = new Queue<DialogueSession>();

	public List<CardScriptableObject> playerStartingHand = new List<CardScriptableObject>();
	public List<CardScriptableObject> enemyStartingHand = new List<CardScriptableObject>();


	protected override void Awake() {
		base.Awake();

		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	protected override void Start() {
		base.Start();
		
		foreach (DialogueSession dialogueSession in dialogueSessions) {
			dialogueSessionsQueue.Enqueue(dialogueSession);
		}
	}

	protected override IEnumerator GameStart() {
		phaseIndicator.text = "Start of Game";
		Debug.Log("run");

		gameArea.GetComponent<CanvasGroup>().alpha = 0f;
		// Game Preparation
		yield return StartCoroutine(GameSetup());

		foreach (ChampionController champion in champions) {
			foreach (ChampionController selectedChampion in champions) {
				if (selectedChampion == champion || selectedChampion.team != champion.team) continue;
				champion.teamMembers.Add(selectedChampion);
			}

			if (champion.isPlayer) {
				foreach (CardScriptableObject cardScriptableObject in playerStartingHand) {
					StartCoroutine(champion.hand.Deal(cardScriptableObject));
				}
			}
			else {
				foreach (CardScriptableObject cardScriptableObject in enemyStartingHand) {
					StartCoroutine(champion.hand.Deal(cardScriptableObject));
				}
			}
		}
		
		confirmButton.transform.SetAsLastSibling();
		gambleButton.transform.SetAsLastSibling();
		endTurnButton.transform.SetAsLastSibling();
		attackCancelButton.transform.SetAsLastSibling();

		LeanTween.alphaCanvas(gameArea.GetComponent<CanvasGroup>(), 1f, 1f).setOnComplete(() => {
			DialogueSystem.Create(dialogueSessionsQueue.Dequeue(), new Vector2(0, -270), () => {
				DialogueSystem.Create(dialogueSessionsQueue.Dequeue(), new Vector2(0, -270), null, false).transform.SetParent(gameArea.transform, false);
				tutorialProgress = 1;
			}, true, false).transform.SetParent(gameArea.transform, false);
		});
		
		yield return new WaitUntil(() => tutorialProgress == 25);

		yield return new WaitForSeconds(2f);

		// NextTurnCalculator();
	}
	protected override IEnumerator GamePrep() {
		gameArea.SetActive(true);

		ChampionSlot.CreateDefaultSlots();
		yield break;
	}

	public override void StartEndPhase() {
		switch (tutorialProgress) {
			case 6:
				
				return;
		}
		
		base.Awake();
	}
	protected override IEnumerator GameSetup() {
		gameArea.GetComponent<Image>().sprite = currentMap.mapBackground;
		gameArea.GetComponent<AudioSource>().clip = currentMap.themeSong;
		
		AudioController.instance.Play(gameArea.GetComponent<AudioSource>());

		foreach (Champion champion in players)
		{
			ChampionSlot slot = players.IndexOf(champion) == 0 ? slots[players.IndexOf(champion)] : slots[2];

			ChampionController championController = Spawn(champion, slot, players.IndexOf(champion) == 0, false);
			championController.team = championController.championID + players.IndexOf(champion);
		}

		yield break;
	}
	protected override IEnumerator GameEndAction(ChampionController victoriousChampion) {
		TMP_Text gameEndText = gameEndArea.transform.GetChild(0).GetComponent<TMP_Text>();
		TMP_Text gameEndTextTeam = gameEndAreaTeam.transform.GetChild(0).GetComponent<TMP_Text>();
		Image winnerAvatar = gameEndArea.transform.GetChild(1).GetComponent<Image>();
		GameObject winnerAvatars = gameEndAreaTeam.transform.GetChild(1).gameObject;
		GameObject rewardPanel = gameEndArea.transform.GetChild(2).gameObject;
		GameObject rewardPanelTeam = gameEndAreaTeam.transform.GetChild(2).gameObject;
		AudioSource musicSource = AudioController.instance.GetAudioSource(gameArea.GetComponent<AudioSource>().clip);
		float cachedVolume = musicSource.volume;

		yield break;
	}
	public override IEnumerator GameEndCheck() {
		List<ChampionController> aliveChampions = new List<ChampionController>();
		yield break;
	}
}
