using UnityEngine;

public class PrefabManager : MonoBehaviour {
	public static PrefabManager instance;

	[Header("GameController")]
	public GameObject championTemplate;
	public GameObject abilityTemplate;
	public GameObject cardTemplate;
	public GameObject handPrefab;
	public GameObject championSlotPrefab;

	[Header("Versatile")]
	public GameObject dialogueSystemPrefab;
	public GameObject confirmDialogPrefab;
	public GameObject miniConfirmDialogPrefab;
	public GameObject notificationDialogPrefab;
	public GameObject championInfoPanelPrefab;

	private void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
        }
    }
}