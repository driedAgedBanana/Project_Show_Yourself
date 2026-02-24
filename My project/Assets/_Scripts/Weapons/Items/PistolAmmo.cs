using UnityEngine;

public class PistolAmmo : MonoBehaviour, IPlayerInteract
{
    public WeaponsAmmoData weaponData;
    public WeaponType weaponType;

    public int minAmmoAmount = 25;
    public int maxAmmoAmount = 60;
    [HideInInspector] public int randomAmount;

    public void Interact()
    {
        WeaponSwapper swapper = FindFirstObjectByType<WeaponSwapper>();

        WeaponBase targetWeapon = weaponType switch
        {
            WeaponType.Pistol => swapper.mainWeapon.GetComponent<WeaponBase>(),
            _ => null
        };

        if (targetWeapon == null)
        {
            Debug.LogWarning($"AmmoBox: No WeaponBase found for {weaponType}!");
            return;
        }

        if (targetWeapon.totalAmountOfCarryAmmo >= weaponData.totalAmountOfCarryAmmo)
        {
            Debug.Log("Ammo full, cannot pick up.");
            return;
        }

        randomAmount = Random.Range(minAmmoAmount, maxAmmoAmount + 1);
        int beforeAmmo = targetWeapon.totalAmountOfCarryAmmo;

        int amountThatCanBeAdded = Mathf.Clamp(targetWeapon.totalAmountOfCarryAmmo + randomAmount, 0, weaponData.totalAmountOfCarryAmmo);
        int actualAddedAmount = amountThatCanBeAdded - beforeAmmo;

        targetWeapon.GainingAmmunition(actualAddedAmount);

        targetWeapon.UpdateAmmoUI();

        Destroy(gameObject);
    }
}
