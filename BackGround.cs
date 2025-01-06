using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround : MonoBehaviour
{
    [SerializeField, Header("視差効果（左右）"), Range(0, 1)]
    private float _parallaxEffectX;

    [SerializeField, Header("視差効果（上下）"), Range(0, 1)]
    private float _parallaxEffectY;

    private GameObject _camera;
    private float _lengthToCameraX;
    private float _lengthToCameraY;
    private float _startBackGroundPosX;
    private float _startBackGroundPosY;

    // Start is called before the first frame update
    void Start()
    {
        _startBackGroundPosX = transform.position.x;
        _startBackGroundPosY = transform.position.y;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        _lengthToCameraX = spriteRenderer.bounds.size.x;
        _lengthToCameraY = spriteRenderer.bounds.size.y;

        _camera = Camera.main.gameObject;
    }

    // FixedUpdateは指定時間ごとに実行される
    // 毎フレーム背景を動かすと、ガクガクした処理になる
    private void FixedUpdate()
    {
        _Parallax();
    }

    private void _Parallax()
    {
        // カメラの移動量に視差効果を適用する
        float tempX = _camera.transform.position.x * (1 - _parallaxEffectX);
        float distanceX = _camera.transform.position.x * _parallaxEffectX;

        float tempY = _camera.transform.position.y * (1 - _parallaxEffectY);
        float distanceY = _camera.transform.position.y * _parallaxEffectY;

        // 背景の座標を更新
        transform.position = new Vector3(
            _startBackGroundPosX + distanceX,
            _startBackGroundPosY + distanceY,
            transform.position.z
        );

        // カメラとの距離が画像の横幅分離れたら、位置をカメラの座標に移動させる（X方向）
        if (tempX > _startBackGroundPosX + _lengthToCameraX)
        {
            _startBackGroundPosX += _lengthToCameraX;
        }
        else if (tempX < _startBackGroundPosX - _lengthToCameraX)
        {
            _startBackGroundPosX -= _lengthToCameraX;
        }

        // カメラとの距離が画像の縦幅分離れたら、位置をカメラの座標に移動させる（Y方向）
        if (tempY > _startBackGroundPosY + _lengthToCameraY)
        {
            _startBackGroundPosY += _lengthToCameraY;
        }
        else if (tempY < _startBackGroundPosY - _lengthToCameraY)
        {
            _startBackGroundPosY -= _lengthToCameraY;
        }
    }
}

