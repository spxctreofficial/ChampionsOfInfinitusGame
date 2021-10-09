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
	protected override IEnumerator BeginningPhase(ChampionController championController) {
		switch (tutorialProgress) {
			case 8:
				gamePhase = GamePhase.BeginningPhase;

				// Updates Text
				phaseIndicator.text = "Beginning Phase - " + championController.champion.championName;
				championController.championParticleController.PlayEffect(championController.championParticleController.CyanGlow);
				yield return StartCoroutine(championController.hand.Deal(CardIndex.FindCardInfo(CardSuit.DIAMOND, 12)));
				yield return StartCoroutine(championController.hand.Deal(CardIndex.FindCardInfo(CardSuit.DIAMOND, 13)));

				// Beginning Phase Ability Check
				foreach (ChampionController selectedChampion in champions) {
					foreach (Ability ability in selectedChampion.abilities) {
						yield return StartCoroutine(ability.OnBeginningPhase());
					}
				}

				yield return new WaitForSeconds(2f);

				StartCoroutine(ActionPhase(championController));
				yield break;
		}

		StartCoroutine(base.BeginningPhase(championController));
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
			championController.team = championController.champion.championID + players.IndexOf(champion);
		}

		yield break;
	}
	protected override IEnumerator GameEndAction(ChampionController victoriousChampionController) {
		AudioSource musicSource = AudioController.instance.GetAudioSource(gameArea.GetComponent<AudioSource>().clip);
		float cachedVolume = musicSource.volume;

		bool isDialogueDone = false;
		DialogueSystem.Create(StatisticManager.instance.winState ? tutorialWinDialogue : tutorialLoseDialogue, new Vector2(0, -270), () => isDialogueDone = true).transform.SetParent(gameArea.transform, false);

		yield return new WaitUntil(() => isDialogueDone);

		gameEndPanel.gameObject.SetActive(true);

		gameEndPanel.winText.text = victoriousChampionController.champion.championName + " wins!";

		foreach (ChampionController championController in champions) {
			if (!championController.teamMembers.Contains(victoriousChampionController) && championController != victoriousChampionController) continue;
					
			Image newWinnerAvatar = Instantiate(gameEndPanel.winnerAvatar, Vector2.zero, Quaternion.identity);
			newWinnerAvatar.gameObject.SetActive(true);
			newWinnerAvatar.sprite = championController.champion.avatar;
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
		foreach (ChampionController championController in champions) {
			if (championController.isDead || championController.currentOwner != null) continue;
			aliveChampions.Add(championController);
		}

		if (aliveChampions.Count == 1) {
			foreach (ChampionController championController in champions) {
				championController.championParticleController.OrangeGlow.Stop();
				championController.championParticleController.CyanGlow.Stop();
				championController.championParticleController.RedGlow.Stop();

				championController.championParticleController.OrangeGlow.Clear();
				championController.championParticleController.CyanGlow.Clear();
				championController.championParticleController.RedGlow.Clear();
			}
			yield return new WaitForSeconds(3f);
			
			GameEnd(aliveChampions[0]);
		}
	}
}
