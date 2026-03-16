using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    [HideInInspector] public bool isAlive = true;
    public GameObject weaponSlot;
    private Rigidbody _rb;
    [Space]
    public AudioList heartBeat;
    [Header("Low Health Audio")]
    public float heartbeatInterval = 1.0f; // Seconds between beats
    [HideInInspector] public float heartbeatTimer;

    [Header("Health UI")]
    public Slider healthSlider;
    public float lerpSpeed;
    [Space]
    public GameObject gameOverPanel;
    [SerializeField] private float _waitTime;
    private float _targetFillAmount;
    [Space]
    public TextMeshProUGUI healthText;


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
        isAlive = currentHealth >= 0;
        currentHealth = maxHealth;
        isAlive = true;
        currentRads = cleanRads;

        UpdateHealthBarUI();

        _rb = PlayerController.Instance.rb;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (radImmunityTimer > 0f)
        {
            radImmunityTimer -= Time.deltaTime;
        }

        UpdateHealthBarUI();
        UpdateHealthBarText();

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    #region Health UI

    public void UpdateHealthBarUI()
    {
        if (healthSlider == null) return;

        // Calculate the target (0.0 to 1.0)
        _targetFillAmount = (float)currentHealth / maxHealth;

        // Smoothly interpolate the current slider value toward the target
        healthSlider.value = Mathf.Lerp(healthSlider.value, _targetFillAmount, Time.deltaTime * lerpSpeed);
    }

    public void UpdateHealthBarText()
    {
        healthText.text = $"Current Health: {currentHealth:F0} / {maxHealth:F0}";
    }

    #endregion

    #region Health Management
    public void TakeDamage(float damageAmount)
    {
        if (!isAlive || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        PlayerController.Instance.cameraShakeManager.ApplyingDamageShake();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (!isAlive || healAmount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    private void Die()
    {
        isAlive = false;
        currentHealth = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        _rb.constraints = RigidbodyConstraints.None;
        weaponSlot.SetActive(false);
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(_waitTime);
        gameOverPanel.SetActive(true);
        GameManager.Instance.ShowMouse();
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
