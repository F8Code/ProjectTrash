using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public List<GameObject> trashPrefabs; // Σύρε εδώ τα 5 prefabs σου (με διαφορετικά χρώματα)
    public Vector3 spawnPosition = new Vector3(0, 1, 0); // Θέση spawn (μπορείς να την κάνεις random)
    public float spawnInterval = 5f; // Κάθε 5 sec

    void Start()
    {
        if (trashPrefabs == null || trashPrefabs.Count == 0)
        {
            Debug.LogError("TrashPrefabs list is empty! Βεβαιώσου ότι σύρεις τα prefabs στο Inspector.");
            return;
        }
        InvokeRepeating("SpawnTrash", 0f, spawnInterval);
    }

    private void SpawnTrash()
    {
        // Επίλεξε τυχαίο prefab από τη λίστα
        int randomIndex = Random.Range(0, trashPrefabs.Count);
        GameObject selectedPrefab = trashPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            GameObject newTrash = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Spawned new Trash (color variant " + randomIndex + ") at " + spawnPosition);
        }
        else
        {
            Debug.LogError("Selected prefab is null!");
        }
    }
}