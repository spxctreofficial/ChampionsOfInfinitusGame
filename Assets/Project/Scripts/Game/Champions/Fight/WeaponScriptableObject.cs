using UnityEngine;

public enum DamageType { Melee, Ranged, Fire, Lightning, Shadow, Unblockable }

[CreateAssetMenu(fileName = "Ability", menuName = "ChampionsOfInfinitusGame/Champion/New Weapon")]
public class WeaponScriptableObject : ScriptableObject {
    public string weaponName;
    [TextArea(5, 20)] public string weaponDescription;
    public Sprite weaponSprite;
    public int attackDamage, maxDurability;
    public DamageType damageType;
}
