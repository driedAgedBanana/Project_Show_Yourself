using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
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

    void SpawnObjects()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, randomPoint.position, randomPoint.rotation);
        _activeEnemies.Add(newEnemy);
    }
}
