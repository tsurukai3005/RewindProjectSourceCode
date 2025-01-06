using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ObjectsSpawner : MonoBehaviour
{
    [Header("Spawnable Objects")]
    [SerializeField] private List<GameObject> objectPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.0f;

    [Header("Force Settings")]
    [SerializeField] private List<float> spawnForceList; // 力のリスト

    [Header("Angle Settings")]
    [SerializeField] private List<float> spawnAngleList; // 射出角度のリスト

    [Header("Angular Velocity Settings")]
    [SerializeField] private List<float> spawnAngleVelocityList; // 角速度のリスト

    [Header("Spawner Parts")]
    [SerializeField] private GameObject spawnerBodyPrefab;
    [SerializeField] private GameObject spawnerBasePrefab;
    float parentZRotation = 180f;

    private GameObject spawnerBodyInstance;
    private GameObject spawnerBaseInstance;

    [SerializeField] public float spawnTimer = 0f;
    List<GameObject> spawnedObjects;
    TimeManager timeManager;

    // 力の可視化用
    private Vector3 lastForceVector = Vector3.zero; // 直近の力ベクトル
    private GameObject lastSpawnedObject; // 直近のオブジェクト

    void Start()
    {
        spawnedObjects = new List<GameObject>();
        timeManager = GetComponent<TimeManager>();
        if (transform.parent != null)
        {
            parentZRotation = transform.parent.eulerAngles.z;
        }

        SpawnSpawnerParts();
    }

    private void FixedUpdate()
    {
        SpawnerUpdate();
    }

    private void SpawnSpawnerParts()
    {
        if (spawnerBodyPrefab != null && spawnerBasePrefab != null)
        {
            float angleInRadians = parentZRotation * Mathf.Deg2Rad;
            
            // スポナーの位置と回転にBodyを生成
            spawnerBodyInstance = Instantiate(
                spawnerBodyPrefab,
                transform.position + new Vector3(
                    0.5f * Mathf.Sin(angleInRadians),
                    -0.5f * Mathf.Cos(angleInRadians),
                    0
                ),
                Quaternion.Euler(0, 0, parentZRotation),
                transform
            );
            spawnerBodyInstance.name = "SpawnerBody";

            // スポナーの位置と回転にBaseを生成
            spawnerBaseInstance = Instantiate(
                spawnerBasePrefab,
                transform.position,
                Quaternion.Euler(0, 0, parentZRotation),
                transform
            );
            spawnerBaseInstance.name = "SpawnerBase";
        }
    }

    void SpawnerUpdate()
    {
        int spawnedQuantity = spawnedObjects.Count;

        if (timeManager.isRunning)
        {
            spawnTimer -= Time.fixedDeltaTime;
            if (spawnTimer <= 0)
            {
                SpawnObject();
                spawnTimer = spawnInterval;
            }
        }
        else if (timeManager.isRewinding)
        {
            spawnTimer += Time.fixedDeltaTime;
            if (spawnTimer >= spawnInterval)
            {
                if ((spawnedQuantity > 0)) {
                    Destroy(spawnedObjects[spawnedQuantity - 1]);
                    spawnedObjects.RemoveAt(spawnedQuantity - 1);
                }
                spawnTimer = 0f;
            }
        }
    }

    // オブジェクトを生成 -> リストの最後尾に加える
    private void SpawnObject()
    {
        GameObject selectedPrefab = ChooseRandomPrefab();
        if (selectedPrefab == null) return;

        GameObject spawnedObject = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

        TimeAdjustObjectAndSpawner(spawnedObject);
        moveSpawnedObject(spawnedObject);

        spawnedObjects.Add(spawnedObject);
        lastSpawnedObject = spawnedObject;
    }

    private void TimeAdjustObjectAndSpawner(GameObject spawnedObject)
    {
        TimeManager spawnedObjectTimeManager = spawnedObject.GetComponent<TimeManager>();
        if (spawnedObjectTimeManager != null)
        {
            spawnedObjectTimeManager.elapsedTime = timeManager.elapsedTime;
        }
    }

    // 生成したオブジェクトに力を加えて射出する
    private void moveSpawnedObject(GameObject spawnedObject)
    {
        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("生成されたオブジェクトに Rigidbody2D がアタッチされていません: " + spawnedObject.name);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;

        // リストからランダムに値を選択
        float randomForce = ChooseRandomForce();
        float randomAngle = ChooseRandomAngle();
        float randomAngularVelocity = ChooseRandomAngularVelocity();

        // 力の方向を計算
        Vector2 forceDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;

        // オブジェクトに力と角速度を加える
        rb.AddForce(forceDirection * randomForce, ForceMode2D.Impulse);
        rb.angularVelocity = randomAngularVelocity;

        lastForceVector = forceDirection * randomForce;

        // Body の角度を調整
        if (spawnerBodyInstance != null)
        {
            spawnerBodyInstance.transform.rotation = Quaternion.Euler(0, 0, randomAngle + 90f);
        }
    }

    // ランダムなオブジェクトを選択
    private GameObject ChooseRandomPrefab()
    {
        if (objectPrefabs.Count == 0) return null;
        return objectPrefabs[Random.Range(0, objectPrefabs.Count)];
    }

    // ランダムな力を選択
    private float ChooseRandomForce()
    {
        if (spawnForceList.Count == 0) return 0f;
        return spawnForceList[Random.Range(0, spawnForceList.Count)];
    }

    // ランダムな射出角度を選択
    private float ChooseRandomAngle()
    {
        if (spawnAngleList.Count == 0) return 0f;
        return spawnAngleList[Random.Range(0, spawnAngleList.Count)];
    }

    // ランダムな角速度を選択
    private float ChooseRandomAngularVelocity()
    {
        if (spawnAngleVelocityList.Count == 0) return 0f;
        return spawnAngleVelocityList[Random.Range(0, spawnAngleVelocityList.Count)];
    }
}
