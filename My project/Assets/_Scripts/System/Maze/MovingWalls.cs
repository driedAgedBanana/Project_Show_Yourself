using UnityEngine;

public class MovingWalls : MonoBehaviour
{
    [SerializeField] private Vector3 _moveDirection = Vector3.right;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _speed = 2f;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        float move = Mathf.PingPong(Time.time * _speed, _distance);
        transform.position = _startPosition + (_moveDirection.normalized * move);
    }
}
