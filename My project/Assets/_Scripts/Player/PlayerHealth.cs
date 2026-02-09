using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    [HideInInspector] public bool isAlive = true;

    [Header("Radiation Rads")]
    [Range(0f, 100f)] public float currentRads = 0f;
    [HideInInspector] public const float cleanRads = 0f;
    public float maxRads = 100f;
    public Image radiationBar;
    private float _lerpRadTimer;
    public float radChipSpeed = 1.2f;

    [Header("Radiation sickness tuning")]
    public float baseSicknessTickDamage = 2f;
    public float maxExtraDamageMultiplier = 4f;
    public float maxTickFrequency = 0.8f;
    public float minTickFrequency = 5.0f;

    private bool _hasRadiationSickness = false;
    private Coroutine _sicknessCoroutine;

    private float radImmunityTimer = 0f;
    private const float RadImmunityDuration = 0.1f;

    private void Start()
    {
        currentHealth = maxHealth;
        isAlive = true;
        currentRads = cleanRads;
    }

    private void Update()
    {
        if (radImmunityTimer > 0f)
        {
            radImmunityTimer -= Time.deltaTime;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    #region Health Management
    public void TakeDamage(float damageAmount)
    {
        if(!isAlive || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        PlayerController.Instance.cameraShakeManager.ApplyingDamageShake();
        print(currentHealth);

        if(currentHealth <= 0)
        {
            // Die();
            print("Player is dead!");
        }
    }

    public void Heal(float healAmount)
    {
        if(!isAlive || healAmount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        // Update UI
    }

    private void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        currentHealth = 0f;
    }
    #endregion

    #region Rads API
    public void TakeRadiation(float amount)
    {
        if (!isAlive || amount <= 0f) return;
        if (radImmunityTimer > 0f) return; // block radiation while immune

        currentRads = Mathf.Clamp(currentRads + amount, cleanRads, maxRads);
        // UpdateRadsUI();

        if (!_hasRadiationSickness && currentRads > 0f)
            StartRadiationSickness();
    }

    public void ReduceRadiation(float amount)
    {
        if (!isAlive) return;

        currentRads = Mathf.Clamp(currentRads - amount, cleanRads, maxRads);

        if (currentRads <= cleanRads && _hasRadiationSickness)
        {
            currentRads = cleanRads;
            radImmunityTimer = RadImmunityDuration;
            StopRadiationSickness();
        }
    }
    #endregion

    #region Radiation sickness
    private void StartRadiationSickness()
    {
        if (_hasRadiationSickness || !isAlive) return;
        _hasRadiationSickness = true;
        _sicknessCoroutine = StartCoroutine(RadiationSicknessRoutine());
    }

    private void StopRadiationSickness()
    {
        if (!_hasRadiationSickness) return;

        _hasRadiationSickness = false;

        if (_sicknessCoroutine != null)
        {
            StopCoroutine(_sicknessCoroutine);
            _sicknessCoroutine = null;
        }
    }

    private IEnumerator RadiationSicknessRoutine()
    {
        while (_hasRadiationSickness && isAlive)
        {
            float radsNormalized = Mathf.Clamp01(currentRads / maxRads);

            // Scaled damage and tick interval
            float extraDamage = baseSicknessTickDamage * maxExtraDamageMultiplier * radsNormalized;
            float tickDamage = baseSicknessTickDamage + extraDamage;
            float interval = Mathf.Lerp(minTickFrequency, maxTickFrequency, radsNormalized);

            TakeDamage(tickDamage);

            if (!isAlive)
            {
                StopRadiationSickness();
                yield break;
            }

            yield return new WaitForSeconds(interval);
        }
    }
    #endregion
}
