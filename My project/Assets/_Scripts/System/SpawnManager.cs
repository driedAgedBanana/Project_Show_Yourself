using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public GameObject bedroomSpawnLocation;

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (playerPrefab != null && bedroomSpawnLocation != null)
        {
            Instantiate(playerPrefab, bedroomSpawnLocation.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Player prefab or spawn location is not assigned in the SpawnManager.");
        }
    }
}
