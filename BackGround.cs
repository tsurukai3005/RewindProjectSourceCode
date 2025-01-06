using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround : MonoBehaviour
{
    [SerializeField, Header("�������ʁi���E�j"), Range(0, 1)]
    private float _parallaxEffectX;

    [SerializeField, Header("�������ʁi�㉺�j"), Range(0, 1)]
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

    // FixedUpdate�͎w�莞�Ԃ��ƂɎ��s�����
    // ���t���[���w�i�𓮂����ƁA�K�N�K�N���������ɂȂ�
    private void FixedUpdate()
    {
        _Parallax();
    }

    private void _Parallax()
    {
        // �J�����̈ړ��ʂɎ������ʂ�K�p����
        float tempX = _camera.transform.position.x * (1 - _parallaxEffectX);
        float distanceX = _camera.transform.position.x * _parallaxEffectX;

        float tempY = _camera.transform.position.y * (1 - _parallaxEffectY);
        float distanceY = _camera.transform.position.y * _parallaxEffectY;

        // �w�i�̍��W���X�V
        transform.position = new Vector3(
            _startBackGroundPosX + distanceX,
            _startBackGroundPosY + distanceY,
            transform.position.z
        );

        // �J�����Ƃ̋������摜�̉��������ꂽ��A�ʒu���J�����̍��W�Ɉړ�������iX�����j
        if (tempX > _startBackGroundPosX + _lengthToCameraX)
        {
            _startBackGroundPosX += _lengthToCameraX;
        }
        else if (tempX < _startBackGroundPosX - _lengthToCameraX)
        {
            _startBackGroundPosX -= _lengthToCameraX;
        }

        // �J�����Ƃ̋������摜�̏c�������ꂽ��A�ʒu���J�����̍��W�Ɉړ�������iY�����j
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

