using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] private GameObject _topWall;
    [SerializeField] private GameObject _bottomWall;
    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _rightWall;

    public void Initialize(bool top, bool bottom, bool right, bool left)
    {
        _topWall.SetActive(top);
        _bottomWall.SetActive(bottom);
        _rightWall.SetActive(right);
        _leftWall.SetActive(left);
    } 
}
