[System.Serializable]
public class Weapon {
	public WeaponScriptableObject weaponScriptableObject;
	public int damageModifier, currentDurability;
	private ChampionController owner;

	public ChampionController Owner {
		get {
			return owner;
        }
		set {
			foreach (ChampionController championController in GameManager.instance.champions) {
				if (championController.equippedWeapon == this) {
					championController.equippedWeapon = null;
                }
			}

			owner = value;
			if (owner is { }) owner.equippedWeapon = this;
		}
    }
	public int EffectiveDamage => weaponScriptableObject.attackDamage + damageModifier;

	public Weapon(WeaponScriptableObject weaponScriptableObject, ChampionController owner = null) {
		this.weaponScriptableObject = weaponScriptableObject;
		Owner = owner;
		currentDurability = weaponScriptableObject.maxDurability;
	}

	public void Damage(int amount) {
		currentDurability -= amount;
		if (currentDurability <= 0) {
			owner.equippedWeapon = null;
		}
	}
}