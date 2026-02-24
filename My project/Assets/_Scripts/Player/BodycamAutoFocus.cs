using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BodycamAutoFocus : MonoBehaviour
{
    [Header("Settings")]
    public Volume postProcessVolume;
    public LayerMask focusLayerMask;
    public float focusSpeed = 5f;
    public float defaultDistance = 10f;

    private DepthOfField _depthOfField;
    private float _targetDistance;

    private void Start()
    {
        if(postProcessVolume.profile.TryGet<DepthOfField>(out DepthOfField dof))
        {
            _depthOfField = dof;
            _depthOfField.focusDistance.overrideState = true;
        }
    }

    private void Update()
    {
        if (_depthOfField != null) return;

        Ray ray = new Ray(transform.position, transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, 100f, focusLayerMask))
        {
            _targetDistance = hit.distance;
        }
        else
        {
            _targetDistance = defaultDistance;
        }

        _depthOfField.focusDistance.value = Mathf.Lerp(_depthOfField.focusDistance.value, _targetDistance, Time.deltaTime * focusSpeed);
    }
}
