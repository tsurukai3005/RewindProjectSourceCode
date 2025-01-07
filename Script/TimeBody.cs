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
    private float currentAngularVelocity;
    private bool hasAppliedInitialForce = false; // 速度が適用済みか

    // オブジェクトがプレイヤーに与える力
    private Vector2 forceOnPlayer = Vector2.zero;
    private float maxForceOnPlayer = 50f;
    private Rigidbody2D playerRb = null;
    private bool isPlayerContacting = false;

    [Header("UI表示用")]
    public Text pointsInTimeText;

    void Start()
    {
        pointsInTime = new List<PointInTime>();
        afterImages = new List<GameObject>();
        rb = GetComponent<Rigidbody2D>();
        timeManager = GetComponent<TimeManager>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CreateAfterImages();
    }

    void Update()
    {
        DrawAfterImages();
    }

    void FixedUpdate()
    {
        SwitchTimeState();
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

        // x, y の位置と z 軸の回転を固定
        rb.constraints = RigidbodyConstraints2D.FreezePositionX |
                         RigidbodyConstraints2D.FreezePositionY |
                         RigidbodyConstraints2D.FreezeRotation;

        HitPlayer();
        //VisualizeForceOnPlayer();
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
                if (playerCollider == null) return;

                playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb == null) return;

                Vector2 relativeVelocity = -pointsInTime[0].velocity - playerRb.linearVelocity;
                forceOnPlayer = relativeVelocity * rb.mass / Time.fixedDeltaTime;
                forceOnPlayer = Vector2.ClampMagnitude(forceOnPlayer, maxForceOnPlayer);
                isPlayerContacting = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                isPlayerContacting = true;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isPlayerContacting && playerRb != null && pointsInTime.Count > 0)
        {
            Vector2 relativeVelocity = -pointsInTime[0].velocity - playerRb.linearVelocity;
            forceOnPlayer = relativeVelocity * playerRb.mass / Time.fixedDeltaTime;
            forceOnPlayer = Vector2.ClampMagnitude(forceOnPlayer, maxForceOnPlayer);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerContacting = false;
            playerRb = null;
            forceOnPlayer = Vector2.zero;
        }
    }

    // 時間進行
    void Running()
    {
        // x, y, z の固定を解除
        rb.constraints = RigidbodyConstraints2D.None;

        if (boundMaterial != null)
        {
            // 時間進行中はオブジェクトの弾性力を有効にする
            rb.sharedMaterial = boundMaterial;
        }

        // Runnningに移行した瞬間のみ速度を適用する
        if (!hasAppliedInitialForce && pointsInTime.Count > 0)
        {
            PointInTime latestPoint = pointsInTime[0];
            rb.linearVelocity = latestPoint.velocity; // 最新のフレームの速度を適用
            rb.angularVelocity = latestPoint.angularVelocity;

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

            // 位置を用いて逆再生すると、軌道上のオブジェクトをすり抜ける
            // 速度・角速度のみによる逆再生だと安定しない
            transform.position = pointInTime.position;
            transform.rotation = pointInTime.rotation;
            rb.linearVelocity = -pointInTime.velocity;
            rb.angularVelocity = -pointInTime.angularVelocity;

            // プレイヤーに力を加える
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
        currentAngularVelocity = rb.angularVelocity;

        pointsInTime.Insert(0, new PointInTime(
            transform.position, 
            transform.rotation,
            currentVelocity,
            currentAngularVelocity
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

    /// forceOnPlayer の力の大きさと方向を可視化する
    private void VisualizeForceOnPlayer()
    {
        if (isPlayerContacting && playerRb != null)
        {
            // 力の始点は現在のオブジェクトの位置
            Vector2 startPoint = transform.position;

            // 力の終点は始点 + forceOnPlayer
            Vector2 endPoint = startPoint + forceOnPlayer * 0.005f;

            // 矢印を描画 (シーンビューのみ)
            Debug.DrawLine(startPoint, endPoint, Color.red, 0.1f, false);

            // 矢印の先端（三角部分）
            Vector2 direction = (endPoint - startPoint).normalized;
            Vector2 right = new Vector2(-direction.y, direction.x) * 0.05f;
            Vector2 left = new Vector2(direction.y, -direction.x) * 0.05f;

            Debug.DrawLine(endPoint, endPoint - direction * 0.1f + right, Color.red, 0.1f, false);
            Debug.DrawLine(endPoint, endPoint - direction * 0.1f + left, Color.red, 0.1f, false);

            // 力の情報をログに出力
            Debug.Log($"ForceOnPlayer: Magnitude = {forceOnPlayer.magnitude:F2}, Direction = {forceOnPlayer.normalized}");
        }
    }


}
