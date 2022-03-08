using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PauseMenu : MonoBehaviour
{

	[SerializeField] private CanvasGroup pauseMenuPanel;
	[SerializeField] private CanvasGroup generalPanel, settingsPanel;
	public bool isPaused;

	private int fadeID, scaleID;

	private void Start()
	{
		pauseMenuPanel.GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch (isPaused)
			{
				case true:
					ResumeGame();
					break;
				case false:
					PauseGame();
					break;
			}
		}
	}

	public void ResumeGame()
	{
		isPaused = false;
		pauseMenuPanel.interactable = false;
		pauseMenuPanel.blocksRaycasts = false;

		LeanTween.cancel(fadeID);
		LeanTween.cancel(scaleID);
		fadeID = LeanTween.alphaCanvas(pauseMenuPanel, 0f, 0.3f).setEaseInOutQuart().uniqueId;
		scaleID = LeanTween.scale(pauseMenuPanel.GetComponent<RectTransform>(), new Vector2(0.8f, 0.8f), 0.3f).setEaseInOutQuart().uniqueId;
	}

	public void PauseGame()
	{
		if (SandboxGameManager.instance is {} &&
		    (SandboxGameManager.instance.mapSelectionConfig.activeInHierarchy
		     || SandboxGameManager.instance.championSelectionConfig.activeInHierarchy
		     || SandboxGameManager.instance.gamemodeSelectionConfig.activeInHierarchy
		     || SandboxGameManager.instance.difficultySelectionConfig.activeInHierarchy)) return;

		isPaused = true;
		pauseMenuPanel.interactable = true;
		pauseMenuPanel.blocksRaycasts = true;

		LeanTween.cancel(fadeID);
		LeanTween.cancel(scaleID);
		fadeID = LeanTween.alphaCanvas(pauseMenuPanel, 1f, 0.3f).setEaseInOutQuart().uniqueId;
		scaleID = LeanTween.scale(pauseMenuPanel.GetComponent<RectTransform>(), Vector3.one, 0.3f).setEaseInOutQuart().uniqueId;
	}

	public void OpenSettings()
	{
		LeanTween.alphaCanvas(generalPanel, 0f, 0.3f).setEaseInOutQuart();
		LeanTween.alphaCanvas(settingsPanel, 1f, 0.3f).setEaseInOutQuart().setOnComplete(() =>
		{
			generalPanel.blocksRaycasts = false;
			settingsPanel.blocksRaycasts = true;
		});
	}

	public void CloseSettings()
	{
		LeanTween.alphaCanvas(settingsPanel, 0f, 0.3f).setEaseInOutQuart();
		LeanTween.alphaCanvas(generalPanel, 1f, 0.3f).setEaseInOutQuart().setOnComplete(() =>
		{
			generalPanel.blocksRaycasts = true;
			settingsPanel.blocksRaycasts = false;
		});
	}

	public void SetFullScreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}

	public void QuitGame()
	{
		ConfirmDialog confirmDialog = ConfirmDialog.CreateNew("QUIT", "\n\nAre you sure you want to quit the game?\n\n\n", () =>
		{
			ConfirmDialog.instance.Hide();
		}, () =>
		{
			Destroy(StatisticManager.instance);
			AudioManager.instance.Stop(GameManager.instance.gameArea.GetComponent<AudioSource>().clip);
			DataManager.instance.Save();
			SceneManager.LoadScene("MainMenu");
		});
		confirmDialog.transform.SetParent(transform, false);
		confirmDialog.GetComponent<RectTransform>().localPosition = new Vector2(0, -270);
	}
}
