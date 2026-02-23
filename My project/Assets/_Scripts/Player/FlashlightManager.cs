using UnityEngine;
using System.Collections;

public class FlashlightManager : MonoBehaviour
{
    public Light weaponFlashLight;

    public float standardIntensity = 25f;
    public float closeRangeIntensity = 10f;

    public float switchDuration = 0.2f;
    public float rayCheckRange;
    public LayerMask obstacleLayer;

    private Coroutine currentRoutine;
    private float currentTarget;

    void Start()
    {
        weaponFlashLight = GetComponent<Light>();
        weaponFlashLight.intensity = standardIntensity;
        currentTarget = standardIntensity;
    }

    void Update()
    {
        RaycastChecker();
    }

    public void RaycastChecker()
    {
        RaycastHit hit;
        float newTarget = standardIntensity;

        if (Physics.Raycast(transform.position, transform.forward, out hit, rayCheckRange, obstacleLayer))
        {
            newTarget = closeRangeIntensity;
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green);
        }

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(SwitchBetweenLightIntensity(currentTarget, switchDuration));
        }
    }

    private IEnumerator SwitchBetweenLightIntensity(float targetIntensity, float duration)
    {
        float startIntensity = weaponFlashLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            weaponFlashLight.intensity = Mathf.SmoothStep(
                startIntensity,
                targetIntensity,
                elapsedTime / duration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        weaponFlashLight.intensity = targetIntensity;
    }
}