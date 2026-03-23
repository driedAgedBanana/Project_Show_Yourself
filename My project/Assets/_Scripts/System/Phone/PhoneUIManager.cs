using System.Collections.Generic;
using UnityEngine;

public enum PhoneApp
{
    Map,
    Health,
    WeaponInformations,
    Mission,
    Notes,
    Settings
}
public class PhoneUIManager : MonoBehaviour
{
    public static PhoneUIManager Instance;

    [Header("Phone UI")]
    public GameObject map;
    public GameObject health;
    public GameObject mission;
    public GameObject weaponInformation;
    public GameObject settings;

    private Dictionary<PhoneApp, GameObject> _phoneApps = new Dictionary<PhoneApp, GameObject>();
    [HideInInspector] public PhoneApp currentPhoneApp = PhoneApp.Map;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _phoneApps[PhoneApp.Map] = map;
        _phoneApps[PhoneApp.Health] = health;
        _phoneApps[PhoneApp.Mission] = mission;
        _phoneApps[PhoneApp.WeaponInformations] = weaponInformation;
        _phoneApps[PhoneApp.Settings] = settings;

        foreach (GameObject menu in _phoneApps.Values)
        {
            menu.SetActive(false);
        }

        SetAppState(PhoneApp.Map);
    }

    public void SetAppState(PhoneApp newPhoneApp)
    {
        // Changes the current menu state and activates/deactivates menus accordingly.
        // Disable the menu's that is not chosen
        foreach (GameObject menu in _phoneApps.Values)
        {
            menu.SetActive(false);
        }

        // Enable the menu that is chosen
        if (_phoneApps.ContainsKey(newPhoneApp))
        {
            _phoneApps[newPhoneApp].SetActive(true);
        }
        currentPhoneApp = newPhoneApp;
    }

    public void ToMap()
    {
        SetAppState(PhoneApp.Map);
    }

    public void ToHealth()
    {
        SetAppState(PhoneApp.Health);
    }

    public void ToMission()
    {
        SetAppState(PhoneApp.Mission);
    }

    public void ToWeaponInformation()
    {
        SetAppState(PhoneApp.WeaponInformations);
    }

    public void ToSettings()
    {
        SetAppState(PhoneApp.Settings);
    }
}
