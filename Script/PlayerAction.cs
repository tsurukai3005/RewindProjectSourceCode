using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField, Header("移動速度")]
    private float _moveSpeed = 5f;
    [SerializeField, Header("空中移動速度")]
    private float _airMoveSpeed = 4f;
    //[SerializeField, Header("ジャンプ速度")]
    private float _jumpSpeed = 30f;
    [SerializeField, Header("ジャンプ可能回数")]
    private int _jumpCounts = 2;
    [SerializeField, Header("最大ジャンプ時間")]
    private float _jumpTime = 0.3f;
    //[SerializeField, Header("通常時の重力の値")]
    private float _defaultGravityScale = 2.0f;
    //[SerializeField, Header("下降時の重力の値")]
    private float _fallGravityScale = 7.0f; // 下降時の強い重力
    //[SerializeField, Header("下降時の速度限界値")]
    private float DownMaxSpeed = 18f;
    [SerializeField, Header("ジャンプSE")]
    private AudioSource _jumpSE;
    [SerializeField, Header("ダメージSE")]
    private GameObject _damageSE;

    private Vector2 _inputDirection;
    private Rigidbody2D _rigid;
    private int _remainingJumps;
    private SpriteRenderer _spriteRenderer;
    private Animator _animation;
    private bool _onFloor;
    private float _jumpTimeCounter;
    private bool _isJumping;
    private bool _jumpButtonHeld;

    void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _animation = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _remainingJumps = _jumpCounts;
        _jumpTimeCounter = _jumpTime;
    }

    private void FixedUpdate()
    {
        becameInvisible();
        _Move();
        _Jump();
        _LookMoveDirection();
        _HitFloor();
        _ApplyFallGravity();
    }

    private void _Move()
    {
        if (!_onFloor)
        {
            _rigid.linearVelocity = new Vector2(_inputDirection.x * _airMoveSpeed, _rigid.linearVelocity.y);
        }
        else
        {
            _rigid.linearVelocity = new Vector2(_inputDirection.x * _moveSpeed, _rigid.linearVelocity.y);
            _animation.SetBool("isWalking", _inputDirection.x != 0.0f);
        }
    }

    private void _LookMoveDirection()
    {
        if (_inputDirection.x > 0.0f)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (_inputDirection.x < 0.0f)
        {
            transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
    }

    private void _HitFloor()
    {
        int FloorlayerMask = LayerMask.GetMask("Floor"); // Floorレイヤーの番号を取得
        Vector3 bottomPos = transform.position - new Vector3(0.0f, transform.lossyScale.y / 2.0f); // プレイヤーの足元の座標
        Vector3 bottomSize = new Vector3(transform.lossyScale.x - 0.1f, 0.3f); // プレイヤーの足元判定のサイズ
        RaycastHit2D bottomHit = Physics2D.BoxCast(bottomPos, bottomSize, 0.0f, Vector2.zero, 0.1f, FloorlayerMask); // プレイヤーの足元に床があるかどうか

        if (bottomHit.transform == null)
        {
            _animation.SetBool("isJumping", true);
            _onFloor = false;
        }
        else
        {
            // 床に着地した瞬間のみ、ジャンプ回数をリセットする
            // これをしないと、ジャンプした直後の床判定でジャンプ回数が復活してしまう
            if (!_onFloor)
            {
                _remainingJumps = _jumpCounts;
                _jumpTimeCounter = _jumpTime;
                _isJumping = false;
            }

            _onFloor = true;
            _animation.SetBool("isJumping", false);
        }    
    }

    // 落下中に強い重力を与える
    private void _ApplyFallGravity()
    {
        if (!_onFloor && _rigid.linearVelocity.y < 0) // 空中かつ下降中
        {
            _rigid.gravityScale = _fallGravityScale;
            float CurrentSpeed = (float)Math.Sqrt(Math.Pow(_rigid.linearVelocity.y, 2));

            // 下降速度に制限を付ける
            if (CurrentSpeed > DownMaxSpeed)
            {
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, _rigid.linearVelocity.y / CurrentSpeed * DownMaxSpeed);
            }
        }
        else
        {
            _rigid.gravityScale = _defaultGravityScale;
        }
    }

    // プレイヤーの足元判定の表示（デバッグ用）
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 bottomPos = transform.position - new Vector3(0.0f, transform.lossyScale.y / 2.0f);
        Vector3 bottomSize = new Vector3(transform.lossyScale.x - 0.1f, 0.1f);

        Gizmos.DrawCube(bottomPos, bottomSize);
    }

    // プレイヤーがカメラ外に移動した場合、消す処理
    private void becameInvisible()
    {
        Camera cameraToUse = mainCamera != null ? mainCamera : Camera.main;

        if (cameraToUse == null)
        {
            Debug.LogError("カメラが見つかりません。InspectorまたはMain Cameraタグを確認してください。");
            return;
        }

        // プレイヤーのワールド座標をビューポート座標に変換
        Vector3 viewportPoint = cameraToUse.WorldToViewportPoint(transform.position);

        // 余裕を持たせる範囲 (-0.2 ～ 1.2)
        float margin = 0.2f;

        bool isOutOfViewport =
            viewportPoint.y < -margin || viewportPoint.y > 1 + margin ||
            viewportPoint.x < -margin || viewportPoint.x > 1 + margin;

        // ビューポート外に完全に出た場合に削除
        if (isOutOfViewport)
        {
            Debug.Log("プレイヤーがカメラの視界外に出たため、削除されました。");
            Destroy(gameObject);
        }
    }

    public void _OnMove(InputAction.CallbackContext context)
    {
        _inputDirection = context.ReadValue<Vector2>();
    }

    public void _Jump()
    {
        if (_jumpButtonHeld && _remainingJumps > 0 && _jumpTimeCounter > 0) // ジャンプ開始
        {
            if (!_isJumping) // ジャンプボタンを押した瞬間のみ発動
            {
                _isJumping = true;
                _remainingJumps--;
                _animation.SetBool("isJumping", false); // 二段ジャンプ時にアニメを最初から再生するための設定

                // 空中ジャンプ時にプレイヤーの速度をゼロにし、一定の高さでジャンプできるようにする
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);

                _rigid.AddForce(Vector2.up * _jumpSpeed, ForceMode2D.Impulse);
                _jumpSE.PlayOneShot(_jumpSE.clip);
            }
            else if (_isJumping) // ジャンプボタンを長押ししたときの処理
            {
                _rigid.AddForce(Vector2.up * _jumpSpeed, ForceMode2D.Force);
                _jumpTimeCounter -= Time.fixedDeltaTime;
            }
        }

        if (!_jumpButtonHeld) // ジャンプ中止
        {
            _isJumping = false;
            _jumpTimeCounter = _jumpTime;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _jumpButtonHeld = true;
                break;

            case InputActionPhase.Canceled:
                _jumpButtonHeld = false;
                break;
        }
    }

}
