using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CameraShakeManager : MonoBehaviour
{
    private Camera _camera;

    [Header("Recoil Settings")]
    public float _recoilX = -2f;
    public float _recoilY = 1f;
    public float resetSpeed = 5f;
    
    private Vector3 _targetRecoil;
    private Vector3 _currentRecoil;

    [Header("Damage Shake Settings")]
    public float _shakeX = -20f;
    public float _shakeY = 10f;
    public float recoverSpeed = 7f;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        CalculateWeaponRecoil();
    }

    public void CalculateWeaponRecoil()
    {
        // Apply the camera position to the new selected position during shooting
        _targetRecoil = Vector3.Lerp(_targetRecoil, Vector3.zero, resetSpeed * Time.deltaTime);
        _currentRecoil = Vector3.Lerp(_currentRecoil, _targetRecoil, Time.deltaTime * 10f);

        transform.localRotation = Quaternion.Euler(_currentRecoil);
    }

    public void ApplyingRecoil()
    {
        // Getting called each time weapon shot- pick a random position
        _targetRecoil += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), 0f);
    }

    public void ApplyingDamageShake()
    {
        _targetRecoil += new Vector3(_shakeX, Random.Range(-_shakeY, _shakeY), 0f);
    }
}
