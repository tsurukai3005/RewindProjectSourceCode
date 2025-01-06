using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine.UI;

public class TimeBody : MonoBehaviour
{
    [SerializeField, Header("Physics Materials")]
    public PhysicsMaterial2D defaultMaterial;
    public PhysicsMaterial2D boundMaterial; // 跳ねる物理マテリアル

    [SerializeField, Header("デバック表示数")]
    public int debugVolume = 10;
    [SerializeField, Header("残像の数")]
    public int afterImageCounts = 5;
    [SerializeField, Header("残像用のオブジェクト")]
    public GameObject afterImageSource;

    List<PointInTime> pointsInTime; // 過去のオブジェクトの状態を記録するリスト
    List<GameObject> afterImages; // 残像を管理するリスト

    TimeManager timeManager; // 時間のカウント・状態の管理

    Rigidbody2D rb;
    private Vector2 currentVelocity;
    private Vector2 previousVelocity;
    private Vector2 MaxVelocity;
    private Vector2 acceleration;
    private bool hasAppliedInitialForce = false; // 速度と加速度が適用済みか
    private float MaxSpeedX = 15f;
    private float MaxSpeedY = 12f;

    // プレイヤーに与える力を記録
    private Vector2 forceOnPlayer = Vector2.zero;
    // プレイヤーが接触しているかを管理
    private Rigidbody2D playerRb = null;
    // プレイヤーとの接触状態
    private bool isPlayerContacting = false;

    [Header("UI表示用")]
    public Text pointsInTimeText;

    void Start()
    {
        pointsInTime = new List<PointInTime>();
        afterImages = new List<GameObject>();
        rb = GetComponent<Rigidbody2D>();
        timeManager = GetComponent<TimeManager>();

        CreateAfterImages();
    }

    void Update()
    {
        //InputManager();
        DrawAfterImages();

        //DebugDisplay();
    }

    private void DebugDisplay()
    {
        // デバッグ表示
        DisplayPointsInTime();
        UpdatePointsInTimeUI();
    }

    void FixedUpdate()
    {
        SwitchTimeState();

        //DisplayVelocityAndAcceleration();
    }

    private void SwitchTimeState()
    {
        if (timeManager.isRewinding)
        {
            Rewind();
        }
        else if (timeManager.isPaused)
        {
            Pause();
            hasAppliedInitialForce = false;
        }
        else if (timeManager.isRunning)
        {
            Running();
            Record();
        }
    }

    // 時間停止：オブジェクトの速度と回転速度をゼロに設定
    void Pause()
    {
        if (defaultMaterial != null)
        {
            rb.sharedMaterial = defaultMaterial;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 衝突検出モードをContinuousに設定
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // x, y の位置と z 軸の回転を固定
        rb.constraints = RigidbodyConstraints2D.FreezePositionX |
                         RigidbodyConstraints2D.FreezePositionY |
                         RigidbodyConstraints2D.FreezeRotation;

        HitPlayer();
    }

    private void HitPlayer()
    {
        // BoxCollider2D で接触判定
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f
        );

        isPlayerContacting = false; // 接触状態リセット

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                CapsuleCollider2D playerCollider = hit.GetComponent<CapsuleCollider2D>();
                if (playerCollider != null)
                {
                    playerRb = hit.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        // PointInTimeから速度を取得
                        Vector2 rewindVelocity = -pointsInTime[0].velocity;

                        // 相対速度を計算
                        Vector2 relativeVelocity = rewindVelocity - playerRb.linearVelocity;

                        forceOnPlayer = 0.005f * relativeVelocity * playerRb.mass / Time.fixedDeltaTime;
                        isPlayerContacting = true;
                    }
                }
            }
        }
    }

    // 時間進行
    void Running()
    {
        float CurrentSpeedX = (float)Math.Sqrt(Math.Pow(rb.linearVelocity.x, 2));
        float CurrentSpeedY = (float)Math.Sqrt(Math.Pow(rb.linearVelocity.y, 2));

        //if (CurrentSpeedX > MaxSpeedX)
        //{
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x / CurrentSpeedX * MaxSpeedX, rb.linearVelocity.y);
        //}
        //if (CurrentSpeedY > MaxSpeedY)
        //{
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / CurrentSpeedY * MaxSpeedY);
        //}

        // Rigidbody2DをDynamicに設定
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.angularDamping = 0.0f;

        // x, y, z の固定を解除
        rb.constraints = RigidbodyConstraints2D.None;

        if (boundMaterial != null)
        {
            // 時間進行中はオブジェクトの弾性力を有効にする
            rb.sharedMaterial = boundMaterial;
        }

        if (!hasAppliedInitialForce && pointsInTime.Count > 0)
        {
            PointInTime latestPoint = pointsInTime[0];
            rb.linearVelocity = latestPoint.velocity;
            //rb.AddForce(latestPoint.acceleration * rb.mass, ForceMode2D.Force);

            hasAppliedInitialForce = true;
        }
    }

    // 時間逆行：新しい記録から順に再生する
    void Rewind()
    {
        // x, y, z の固定を解除
        rb.constraints = RigidbodyConstraints2D.None;

        if (defaultMaterial != null)
        {
            rb.sharedMaterial = defaultMaterial;
        }

        if (pointsInTime.Count > 0)
        {
            PointInTime pointInTime = pointsInTime[0];
            transform.position = pointInTime.position;
            transform.rotation = pointInTime.rotation;
            rb.linearVelocity = -pointInTime.velocity;

            // プレイヤーに力を適用
            if (isPlayerContacting && playerRb != null)
            {
                playerRb.AddForce(forceOnPlayer, ForceMode2D.Impulse);
            }

            pointsInTime.RemoveAt(0);
        }
        else
        {
            timeManager.isRewinding = false;
            timeManager.isPaused = true;
            Pause();
        }

        // 力をリセット
        forceOnPlayer = Vector2.zero;
        playerRb = null;
        isPlayerContacting = false;
    }


    // 記録：recordTimeの時間分、オブジェクトの位置・角度・速度を記録する
    void Record()
    {
        currentVelocity = rb.linearVelocity;

        if (pointsInTime.Count > 0)
        {
            previousVelocity = pointsInTime[0].velocity;
            acceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
        }

        pointsInTime.Insert(0, new PointInTime(
            transform.position, 
            transform.rotation,
            currentVelocity,
            acceleration
        ));
    }

    // 残像の描写に必要な分だけオブジェクトを複製する
    void CreateAfterImages()
    {
        if (afterImageSource == null) return;

        for (int i = 0; i < afterImageCounts; i++)
        {
            GameObject afterImage = Instantiate(afterImageSource, transform.position, transform.rotation);
            afterImage.name = gameObject.name + $"_afterImage[{i}]";
            //RemoveUnnecessaryComponents(afterImage);
            afterImage.transform.SetParent(this.transform);
            afterImage.transform.localScale = Vector3.one;
            afterImages.Add(afterImage);

            SpriteRenderer spriteRenderer = afterImage.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "BackEffect";
                spriteRenderer.sortingOrder = -1; // 他のオブジェクトよりも後ろに描画
            }
        }
    }

    // 過去の位置・角度を元に残像を配置
    void DrawAfterImages()
    {
        if (afterImageSource == null) return;

        for (int i = 0; i < afterImageCounts; i++)
        {
            if (pointsInTime.Count > i + 1) // 安全にインデックスを確認
            {
                afterImages[i].transform.position = pointsInTime[i + 1].position;
                afterImages[i].transform.rotation = pointsInTime[i + 1].rotation;

                // 残像の透明度を下げる
                SpriteRenderer spriteRenderer = afterImages[i].GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    float transparency = 0.2f;
                    color.a = transparency;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    void DisplayVelocityAndAcceleration()
    {
        if (rb != null)
        {
            // 現在の速度を取得
            Vector2 currentVelocity = rb.linearVelocity;

            // 速度をデバッグログに表示
            Debug.Log($"速度: {currentVelocity.magnitude:F2} m/s, 方向: {currentVelocity.normalized}");
        }
    }

    void DisplayPointsInTime()
    {
        Debug.Log($"--- PointsInTime List for Object: {gameObject.name} (Count: {pointsInTime.Count}) ---");
        for (int i = 0; i < pointsInTime.Count; i++)
        {
            PointInTime point = pointsInTime[i];
            Debug.Log($"Point {i}: Position: {point.position}, Rotation: {point.rotation.eulerAngles}, Velocity: {point.velocity}");
        }
        Debug.Log($"--- End of PointsInTime List for Object: {gameObject.name} ---");
    }

    // ゲーム画面上に pointsInTime を表示
    void UpdatePointsInTimeUI()
    {
        if (pointsInTimeText != null)
        {
            string displayText = $"--- PointsInTime ({gameObject.name}) ---\n";
            for (int i = 0; i < Mathf.Min(pointsInTime.Count, debugVolume); i++) // 上位5つだけ表示
            {
                PointInTime point = pointsInTime[i];
                displayText += $"Point {i}: Pos({point.position.x:F2}, {point.position.y:F2}), Rot({point.rotation.eulerAngles.z:F2}), Vel({point.velocity.x:F2}, {point.velocity.y:F2})\n";
            }

            if (pointsInTime.Count > debugVolume)
                displayText += $"... and {pointsInTime.Count - debugVolume} more points\n";

            pointsInTimeText.text = displayText;
        }
    }

}
