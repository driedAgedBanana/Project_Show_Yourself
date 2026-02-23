using UnityEngine;

public class RifleAmmo : MonoBehaviour, IPlayerInteract
{
    public WeaponsAmmoData weaponData;
    public WeaponType weaponType = WeaponType.Rifle;

    public int minAmmoAmount = 25;
    public int maxAmmoAmount = 60;
    [HideInInspector] public int randomAmount;

    public void Interact()
    {
        
    }
}
