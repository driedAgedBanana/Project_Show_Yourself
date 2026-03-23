using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 2f;
    public int spawnLimit = 10;

    private float _timer = 0f;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    void Update()
    {
        // Clean up any null (dead) enemies in the list
        _activeEnemies.RemoveAll(enemy => enemy == null);

        if (_activeEnemies.Count >= spawnLimit) return;

        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            SpawnObjects();
            _timer = 0f;
        }
    }

    private void SpawnObjects()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnLimit; i++)
        {
            // 1. Pick a random PREFAB from your array
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // 2. Pick a random SPAWN POINT
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 3. Instantiate and store the reference
            GameObject newEnemy = Instantiate(randomPrefab, randomPoint.position, randomPoint.rotation);
            _activeEnemies.Add(newEnemy);
        }
    }
}
