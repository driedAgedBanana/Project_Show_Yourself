using UnityEngine;

[System.Serializable]
public class EnemiesDeadLoot
{
    public GameObject itemPrefab;
    [Range(0, 100)] public float dropChance;
}
