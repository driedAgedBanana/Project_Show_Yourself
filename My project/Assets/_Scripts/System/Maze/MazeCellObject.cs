using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] private GameObject _topWall;
    [SerializeField] private GameObject _bottomWall;
    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _rightWall;

    [Header("Exit Visuals")]
    [SerializeField] private GameObject _exitVisual; // Drag your Door/Portal prefab here
    public bool isExit;

    public void Initialize(bool top, bool bottom, bool right, bool left, bool exitState)
    {
        _topWall.SetActive(top);
        _bottomWall.SetActive(bottom);
        _rightWall.SetActive(right);
        _leftWall.SetActive(left);

        isExit = exitState;
        if (_exitVisual != null) _exitVisual.SetActive(isExit);
    }
}
