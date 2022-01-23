public class Weapon {
	public WeaponScriptableObject weaponScriptableObject;
	public int damageModifier, currentDurability;
	public ChampionController owner;

	public int EffectiveDamage => weaponScriptableObject.attackDamage + damageModifier;

	public Weapon(WeaponScriptableObject weaponScriptableObject, ChampionController owner) {
		this.weaponScriptableObject = weaponScriptableObject;
		this.owner = owner;
		currentDurability = weaponScriptableObject.maxDurability;
	}

	public void Damage(int amount) {
		currentDurability -= amount;
		if (currentDurability <= 0) {
			owner.equippedWeapon = null;
		}
	}
}