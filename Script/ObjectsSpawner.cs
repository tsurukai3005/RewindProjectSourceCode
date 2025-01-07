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
    [SerializeField] private List<float> spawnForceList;

    [Header("Angle Settings")]
    [SerializeField] private List<float> spawnAngleList;

    [Header("Angular Velocity Settings")]
    [SerializeField] private List<float> spawnAngleVelocityList;

    [Header("Spawner Parts")]
    [SerializeField] private GameObject spawnerBodyPrefab;
    [SerializeField] private GameObject spawnerBasePrefab;
    [SerializeField] private float parentZRotation = 180f; // �X�|�i�[�ڒn�ʂ̊p�x�i180�x�ŉ������ɐڒn����j

    private GameObject spawnerBodyInstance;
    private GameObject spawnerBaseInstance;

    [SerializeField] public float spawnTimer = 0f;
    List<GameObject> spawnedObjects;
    TimeManager timeManager;

    // �͂̉����p
    private Vector3 lastForceVector = Vector3.zero; // ���߂̗̓x�N�g��
    private GameObject lastSpawnedObject; // ���߂̃I�u�W�F�N�g

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

    // �X�|�i�[�̈ʒu�ɑ�C�̌����ڂ𐶐�����
    private void SpawnSpawnerParts()
    {
        if (spawnerBodyPrefab != null && spawnerBasePrefab != null)
        {
            float angleInRadians = parentZRotation * Mathf.Deg2Rad;
            
            // �X�|�i�[�̈ʒu�Ɖ�]��Body�𐶐�
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

            // �X�|�i�[�̈ʒu�Ɖ�]��Base�𐶐�
            spawnerBaseInstance = Instantiate(
                spawnerBasePrefab,
                transform.position,
                Quaternion.Euler(0, 0, parentZRotation),
                transform
            );
            spawnerBaseInstance.name = "SpawnerBase";
        }
    }

    // 
    void SpawnerUpdate()
    {
        

        if (timeManager.isRunning)
        {
            SpawnerRun();
        }
        else if (timeManager.isRewinding)
        {
            SpawnerRewind();
        }
    }

    private void SpawnerRun()
    {
        spawnTimer -= Time.fixedDeltaTime;

        if (spawnTimer <= 0)
        {
            SpawnObject();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnerRewind()
    {
        int spawnedQuantity = spawnedObjects.Count;
        spawnTimer += Time.fixedDeltaTime;

        if (spawnTimer >= spawnInterval)
        {
            if ((spawnedQuantity > 0))
            {
                // �ˏo�����I�u�W�F�N�g���t�Đ��ŏ��ɏ����Ă���
                Destroy(spawnedObjects[spawnedQuantity - 1]);
                spawnedObjects.RemoveAt(spawnedQuantity - 1);
            }

            spawnTimer = 0f;
        }
    }

    // �I�u�W�F�N�g�𐶐� -> ���X�g�̍Ō���ɉ�����
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

    // �V���ɐ��������I�u�W�F�N�g�̎��Ԃ𑼂Ƒ�����
    private void TimeAdjustObjectAndSpawner(GameObject spawnedObject)
    {
        TimeManager spawnedObjectTimeManager = spawnedObject.GetComponent<TimeManager>();
        if (spawnedObjectTimeManager != null)
        {
            spawnedObjectTimeManager.elapsedTime = timeManager.elapsedTime;
        }
    }

    // ���������I�u�W�F�N�g�ɗ͂������Ďˏo����
    private void moveSpawnedObject(GameObject spawnedObject)
    {
        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("�������ꂽ�I�u�W�F�N�g�� Rigidbody2D ���A�^�b�`����Ă��܂���: " + spawnedObject.name);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;

        // ���X�g���烉���_���ɒl��I��
        float randomForce = ChooseRandomForce();
        float randomAngle = ChooseRandomAngle();
        float randomAngularVelocity = ChooseRandomAngularVelocity();

        // �͂̕������v�Z
        Vector2 forceDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;

        // �I�u�W�F�N�g�ɗ͂Ɗp���x��������
        rb.AddForce(forceDirection * randomForce, ForceMode2D.Impulse);
        rb.angularVelocity = randomAngularVelocity;

        lastForceVector = forceDirection * randomForce;

        // �ˏo�p��Body�����킹��
        if (spawnerBodyInstance != null)
        {
            spawnerBodyInstance.transform.rotation = Quaternion.Euler(0, 0, randomAngle + 90f);
        }
    }

    // �����_���ȃI�u�W�F�N�g��I��
    private GameObject ChooseRandomPrefab()
    {
        if (objectPrefabs.Count == 0) return null;
        return objectPrefabs[Random.Range(0, objectPrefabs.Count)];
    }

    // �����_���ȗ͂�I��
    private float ChooseRandomForce()
    {
        if (spawnForceList.Count == 0) return 0f;
        return spawnForceList[Random.Range(0, spawnForceList.Count)];
    }

    // �����_���Ȏˏo�p�x��I��
    private float ChooseRandomAngle()
    {
        if (spawnAngleList.Count == 0) return 0f;
        return spawnAngleList[Random.Range(0, spawnAngleList.Count)];
    }

    // �����_���Ȋp���x��I��
    private float ChooseRandomAngularVelocity()
    {
        if (spawnAngleVelocityList.Count == 0) return 0f;
        return spawnAngleVelocityList[Random.Range(0, spawnAngleVelocityList.Count)];
    }
}
