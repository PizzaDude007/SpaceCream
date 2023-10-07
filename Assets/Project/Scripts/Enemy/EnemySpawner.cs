using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;
using static PlasticPipe.Server.MonitorStats;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyTypes;
    //public Vector2 spawnAreaRange;
    public float minSpawnRange = 30f;
    public float maxSpawnRange = 100f;

    public int maxEnemies;
    [SerializeField]
    public static int currentEnemies;
    public float spawnRate;

    public GameObject blackhole_FX;

    //private GameObject player;

    private void Awake()
    {
        currentEnemies = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        blackhole_FX = (GameObject)Resources.Load("FX/FX_Blackhole", typeof(GameObject));
        StartCoroutine("SpawnEnemy");
        StartCoroutine("UpdateEnemyCount");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnEnemy()
    {
        while (true)
        {
            if (currentEnemies < maxEnemies)
            {
                //Spawn enemies in a circle around the player
                float randomDistance = Random.Range(minSpawnRange, maxSpawnRange);
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);

                //Spawn enemy at random position around player
                Vector3 pos = transform.position + new Vector3(Mathf.Cos(randomAngle) * randomDistance, 0f, Mathf.Sin(randomAngle) * randomDistance);
                GameObject enemyPrefab = SelectRandomEnemyType();
                Instantiate(enemyPrefab, pos, Quaternion.identity);

                //Spawn blackhole FX
                GameObject spawn_FX = Instantiate(blackhole_FX, pos, Quaternion.identity);
                spawn_FX.GetComponent<ParticleSystem>().Play();
                Destroy(spawn_FX, 5f);

                currentEnemies++;
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

    IEnumerable UpdateEnemyCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        }
    }

    GameObject SelectRandomEnemyType()
    {
        // Calculate the total inverse weight based on the attack damages of enemy types.
        float totalInverseWeight = 0f;
        foreach (var enemyType in enemyTypes)
        {
            totalInverseWeight += 1.0f / enemyType.GetComponent<AssignEnemyType>().AttackDamage;
        }

        // Generate a random value within the total inverse weight.
        float randomValue = Random.Range(0f, totalInverseWeight);

        // Select an enemy type based on the random value and inverse weights.
        foreach (var enemyType in enemyTypes)
        {
            float inverseWeight = 1.0f / enemyType.GetComponent<AssignEnemyType>().AttackDamage;
            if (randomValue <= inverseWeight)
            {
                return enemyType;
            }
            randomValue -= inverseWeight;
        }

        // Return the last enemy type if none were selected (shouldn't happen).
        return enemyTypes[enemyTypes.Length - 1];
    }

    //GameObject SelectRandomEnemyType()
    //{
    //    // Calculate the total weight based on attack damages of enemy types.
    //    float totalWeight = 0f;
    //    foreach (var enemyType in enemyTypes)
    //    {
    //        totalWeight += enemyType.GetComponent<AssignEnemyType>().AttackDamage;
    //    }

    //    // Generate a random value within the total weight.
    //    float randomValue = Random.Range(0f, totalWeight);

    //    // Select an enemy type based on the random value and weights.
    //    foreach (var enemyType in enemyTypes)
    //    {
    //        if (randomValue <= enemyType.GetComponent<AssignEnemyType>().AttackDamage)
    //        {
    //            return enemyType;
    //        }
    //        randomValue -= enemyType.GetComponent<AssignEnemyType>().AttackDamage;
    //    }

    //    // Return the last enemy type if none were selected (shouldn't happen).
    //    return enemyTypes[enemyTypes.Length - 1];
    //}
}
