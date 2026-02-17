using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponAmmoData")]
public class WeaponsAmmoData : ScriptableObject
{
    public WeaponType weaponType;
    public int maxAmmo;
    public int totalAmountOfCarryAmmo;
}
