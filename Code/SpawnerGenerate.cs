using UnityEngine;
using System.Collections.Generic;

public class SpawnerGenerate : MonoBehaviour
{
    [Header("Floor Settings")]
    [SerializeField] private float YInterval = 25f; // Y軸方向の間隔
    [SerializeField] private float XInterval = 8f;
    [SerializeField] private float FloorAngle = 20f; // 床の傾き
    [SerializeField] private GameObject RandomFloor; // 生成する床オブジェクト

    [Header("Player Reference")]
    [SerializeField] private GameObject Player; // プレイヤーオブジェクト

    [Header("Spawner Settings")]
    [SerializeField] private List<GameObject> Spawners;
    private TimeManager spawnerTimeManager;
    private TimeManager timeManager;

    private float spawnThreshold; // 次に生成する閾値
    private float lastSpawnY = 0f; // 最後に生成した床のY座標
    private List<GameObject> spawnedFloors = new List<GameObject>(); // 生成済みの床オブジェクトリスト
    private string lastLane = ""; // 最後に使用したレーン

    private readonly string[] lanes = { "Left", "Center", "Right" }; // レーン名

    void Start()
    {
        timeManager = GetComponent<TimeManager>();

        if (Player == null || RandomFloor == null)
        {
            Debug.LogError("Player または RandomFloor がアサインされていません。");
            return;
        }

        FirstSpawn();
    }

    void Update()
    {
        if (Player != null)
        {
            HandleFloorSpawning();
            HandleFloorDeletion();
        }

    }

    /// プレイヤーの位置に基づき、床の生成を管理する
    private void HandleFloorSpawning()
    {
        if (Player.transform.position.y > spawnThreshold)
        {
            SpawnFloor();
            spawnThreshold += YInterval; // 次の生成ポイントを更新
        }
    }
    private void FirstSpawn()
    {
        // **初期生成：3個分のオブジェクトを配置**
        for (int i = 0; i < 3; i++)
        {
            SpawnFloor();
        }

        // 次の生成ポイントを設定
        spawnThreshold = YInterval;
    }

    /// <summary>
    /// 床をスポーンする
    /// </summary>
    private void SpawnFloor()
    {
        string nextLane = ChooseNextLane();
        GameObject selectedSpawner = ChooseRandomSpawner();

        float xPos = GetLanePosition(nextLane);
        float yPos = lastSpawnY + YInterval;
        float angle = GetLaneAngle(nextLane);

        GameObject parentFloor = Instantiate(RandomFloor, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, 0));
        spawnedFloors.Add(parentFloor);

        // スポナーを子として配置
        GameObject spawnerChild = Instantiate(
            selectedSpawner,
            parentFloor.transform.position + new Vector3(0, -2f, 0),
            Quaternion.identity,
            parentFloor.transform
        );

        parentFloor.transform.rotation = Quaternion.Euler(0, 0, angle);

        TimeAdjust(spawnerChild);

        parentFloor.name = $"Floor_{nextLane}_{yPos}";
        spawnerChild.name = $"Spawner_{nextLane}_{yPos}";

        lastSpawnY = yPos;
        lastLane = nextLane;
    }

    private void TimeAdjust(GameObject spawnerChild)
    {
        spawnerTimeManager = spawnerChild.GetComponent<TimeManager>();
        spawnerTimeManager.elapsedTime = timeManager.elapsedTime;
    }

    private GameObject ChooseRandomSpawner()
    {
        return Spawners[Random.Range(0, Spawners.Count)];
    }

    /// レーンをランダムに選択。ただし連続配置は不可。
    /// <returns>次に選択するレーン名</returns>
    private string ChooseNextLane()
    {
        List<string> availableLanes = new List<string>(lanes);
        availableLanes.Remove(lastLane); // 連続配置を避ける

        return availableLanes[Random.Range(0, availableLanes.Count)];
    }

    /// レーン名に基づきX座標を取得
    private float GetLanePosition(string lane)
    {
        switch (lane)
        {
            case "Left":
                return XInterval;
            case "Right":
                return -XInterval;
            case "Center":
            default:
                return 0f;
        }
    }

    /// レーン名に基づき角度を取得
    private float GetLaneAngle(string lane)
    {
        switch (lane)
        {
            case "Left":
                return FloorAngle; // 左に傾ける
            case "Right":
                return -FloorAngle; // 右に傾ける
            case "Center":
            default:
                return (Random.Range(0, 2) == 0) ? FloorAngle : -FloorAngle; // ランダムな2方向に傾ける
        }
    }

    /// <summary>
    /// プレイヤーから離れた床を削除する
    /// </summary>
    private void HandleFloorDeletion()
    {
        float deleteThreshold = Player.transform.position.y - YInterval * 2;

        for (int i = spawnedFloors.Count - 1; i >= 0; i--)
        {
            if (spawnedFloors[i].transform.position.y < deleteThreshold)
            {
                Destroy(spawnedFloors[i]);
                spawnedFloors.RemoveAt(i);
            }
        }
    }
}
