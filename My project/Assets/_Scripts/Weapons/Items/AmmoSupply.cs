using UnityEngine;

public class AmmoSupply : MonoBehaviour, IPlayerInteract
{
    public WeaponsAmmoData weaponData;
    public WeaponType weaponType;

    public int minAmmoAmount = 25;
    public int maxAmmoAmount = 60;
    [HideInInspector] public int randomAmount;

    public AudioList collectItem;
    public AudioList denySFX;

    public void Interact()
    {
        WeaponSwapper swapper = FindFirstObjectByType<WeaponSwapper>();
        if (swapper == null) return;

        // 1. Get all weapons currently on the player
        WeaponBase[] allWeapons = swapper.GetComponentsInChildren<WeaponBase>(true);
        WeaponBase targetWeapon = null;

        // 2. Find the weapon that matches this box's type
        foreach (WeaponBase weapon in allWeapons)
        {
            if (weapon.currentWeaponType == this.weaponType)
            {
                targetWeapon = weapon;
                break;
            }
        }

        // 3. Safety checks
        if (targetWeapon == null)
        {
            Debug.LogWarning($"Player isn't carrying a {weaponType}!");
            return;
        }

        // Use the weapon's own data for the max limit
        int maxAllowed = targetWeapon.ammoData.totalAmountOfCarryAmmo;
        if (targetWeapon.totalAmountOfCarryAmmo >= maxAllowed)
        {
            AudioManager.Instance.PlaySounds(denySFX, transform.position);
            return;
        }

        // 4. Calculate and add ammo
        randomAmount = Random.Range(minAmmoAmount, maxAmmoAmount + 1);
        int amountToAdd = Mathf.Min(randomAmount, maxAllowed - targetWeapon.totalAmountOfCarryAmmo);

        if (amountToAdd > 0)
        {
            targetWeapon.GainingAmmunition(amountToAdd);
            targetWeapon.UpdateAmmoUI();
            AudioManager.Instance.PlaySounds(collectItem, transform.position);
            Destroy(gameObject);
        }
    }
}
