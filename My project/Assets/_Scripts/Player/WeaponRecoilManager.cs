using UnityEngine;

public class WeaponRecoilManager : MonoBehaviour
{
    public float _recoilX = -2f;
    public float _recoilY = 1f;
    public float resetSpeed = 5f;

    private Vector3 _targetRecoil;
    private Vector3 _currentRecoil;

    private void FixedUpdate()
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
}
