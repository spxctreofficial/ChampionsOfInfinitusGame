using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialGameController : GameController {
	public new static TutorialGameController instance;

	[HideInInspector]
	public int tutorialProgress;

	public List<DialogueSession> dialogueSessions = new List<DialogueSession>();
	[SerializeField]
	private DialogueSession tutorialWinDialogue, tutorialLoseDialogue;
	public Queue<DialogueSession> dialogueSessionsQueue = new Queue<DialogueSession>();

	public List<CardScriptableObject> playerStartingHand = new List<CardScriptableObject>();
	public List<CardScriptableObject> enemyStartingHand = new List<CardScriptableObject>();


	protected override void Awake() {
		base.Awake();
		
		gameArea.GetComponent<CanvasGroup>().alpha = 0f;

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
		
		yield return new WaitUntil(() => tutorialProgress == 8);

		yield return new WaitForSeconds(2f);

		NextTurnCalculator();
	}
	protected override IEnumerator BeginningPhase(ChampionController champion) {
		switch (tutorialProgress) {
			case 8:
				gamePhase = GamePhase.BeginningPhase;

				// Updates Text
				phaseIndicator.text = "Beginning Phase - " + champion.championName;
				champion.championParticleController.PlayEffect(champion.championParticleController.CyanGlow);
				yield return StartCoroutine(champion.hand.Deal(CardIndex.FindCardInfo(CardSuit.DIAMOND, 12)));
				yield return StartCoroutine(champion.hand.Deal(CardIndex.FindCardInfo(CardSuit.DIAMOND, 13)));

				// Beginning Phase Ability Check
				foreach (ChampionController selectedChampion in champions) {
					foreach (Ability ability in selectedChampion.abilities) {
						yield return StartCoroutine(ability.OnBeginningPhase());
					}
				}

				yield return new WaitForSeconds(2f);

				StartCoroutine(ActionPhase(champion));
				yield break;
		}

		StartCoroutine(base.BeginningPhase(champion));
	}
	// protected override IEnumerator ActionPhase(ChampionController champion) {
	// 	switch (tutorialProgress) {
	// 		case 8:
	// 			yield break;
	// 	}
	// 	
	// 	StartCoroutine(base.ActionPhase(champion));
	// }
	protected override IEnumerator GamePrep() {
		gameArea.SetActive(true);

		ChampionSlot.CreateDefaultSlots();
		yield break;
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
		AudioSource musicSource = AudioController.instance.GetAudioSource(gameArea.GetComponent<AudioSource>().clip);
		float cachedVolume = musicSource.volume;

		bool isDialogueDone = false;
		DialogueSystem.Create(StatisticManager.instance.winState ? tutorialWinDialogue : tutorialLoseDialogue, new Vector2(0, -270), () => isDialogueDone = true).transform.SetParent(gameArea.transform, false);

		yield return new WaitUntil(() => isDialogueDone);

		gameEndPanel.gameObject.SetActive(true);

		gameEndPanel.winText.text = victoriousChampion.championName + " wins!";

		foreach (ChampionController champion in champions) {
			if (!champion.teamMembers.Contains(victoriousChampion) && champion != victoriousChampion) continue;
					
			Image newWinnerAvatar = Instantiate(gameEndPanel.winnerAvatar, Vector2.zero, Quaternion.identity);
			newWinnerAvatar.gameObject.SetActive(true);
			newWinnerAvatar.sprite = champion.avatar;
			newWinnerAvatar.transform.SetParent(gameEndPanel.winnerAvatars.transform, false);
		}

		gameEndPanel.rewardPanel.GetComponent<RectTransform>().localPosition = new Vector2(-1920, 0);

		while (musicSource.volume > 0.5f * cachedVolume) {
			musicSource.volume -= 0.5f * cachedVolume / 180;
			yield return null;
		}
				
		yield return new WaitForSeconds(3f);

		LeanTween.move(gameEndPanel.winnerAvatars.GetComponent<RectTransform>(), new Vector2(1920, 0), 0.75f).setEaseInOutQuart();
		LeanTween.move(gameEndPanel.rewardPanel.GetComponent<RectTransform>(), Vector2.zero, 0.75f).setEaseInOutQuart().setOnComplete(() => {
			StartCoroutine(StatisticManager.instance.RewardCalculation(gameEndPanel.rewardText, gameEndPanel.collectButton.gameObject));
		});
	}
	public override IEnumerator GameEndCheck() {
		List<ChampionController> aliveChampions = new List<ChampionController>();
		foreach (ChampionController champion in champions) {
			if (champion.isDead || champion.currentOwner != null) continue;
			aliveChampions.Add(champion);
		}

		if (aliveChampions.Count == 1) {
			foreach (ChampionController champion in champions) {
				champion.championParticleController.OrangeGlow.Stop();
				champion.championParticleController.CyanGlow.Stop();
				champion.championParticleController.RedGlow.Stop();

				champion.championParticleController.OrangeGlow.Clear();
				champion.championParticleController.CyanGlow.Clear();
				champion.championParticleController.RedGlow.Clear();
			}
			yield return new WaitForSeconds(3f);
			
			GameEnd(aliveChampions[0]);
		}
	}
}
