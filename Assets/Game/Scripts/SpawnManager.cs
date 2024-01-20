using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [HideInInspector] public Transform targetSpawn;

    public static SpawnManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var spawnPoint in spawnPoints)
            spawnPoint.gameObject.SetActive(false);
    }

    public Transform GetPlayerTransform()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
