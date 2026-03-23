using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DistanceCheck : MonoBehaviour
{
    public TextMeshProUGUI distanceAmountText;
    public Door endDoor;
    public GameObject destinationPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endDoor = FindFirstObjectByType<Door>();
        destinationPrefab = endDoor.door;
    }

    // Update is called once per frame
    void Update()
    {
        if (destinationPrefab == null) return;
        CalculateDistance();
    }

    public void CalculateDistance()
    {
        float distance = Vector3.Distance(transform.position, destinationPrefab.transform.position);

        distanceAmountText.text = $"Distance to end point: {distance:F2} metres";
    }
}
