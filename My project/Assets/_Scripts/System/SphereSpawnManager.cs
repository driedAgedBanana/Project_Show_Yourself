using System.Collections.Generic;
using UnityEngine;

public class SphereSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject spherePrefab;
    public float spawnInterval = 0.01f;
    public int spawnLimit;

    private float _timer;
    private float _spawnedCount;
    private BoxCollider _spawnZone;

    private List<GameObject> _activeSphere = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        _spawnZone = GetComponent<BoxCollider>();
        if (_spawnZone == null || !spherePrefab)
        {
            Debug.LogError("Missing box collider or sphere prefab!");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _activeSphere.RemoveAll(sphere => sphere == null);

        if (_activeSphere.Count >= spawnLimit) return;

        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            SpawnSphere();
            _timer = 0f;
        }
    }

    private void SpawnSphere()
    {
        Vector3 spawnPos = GetRandomPointInCollision(_spawnZone);
        GameObject newSphere = Instantiate(spherePrefab, spawnPos, Quaternion.identity);
        _activeSphere.Add(newSphere);
    }

    Vector3 GetRandomPointInCollision(BoxCollider box)
    {
        Vector3 center = box.center + transform.position;
        Vector3 size = box.size;

        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float y = Random.Range(-size.y / 2f, size.y / 2f);
        float z = Random.Range(-size.z / 2f, size.z / 2f);

        return center + new Vector3(x, y, z);
    }

    void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }
    }
}
