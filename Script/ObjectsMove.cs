using System.Runtime.CompilerServices;
using UnityEngine;

public class ObjectsMove : MonoBehaviour
{
    [SerializeField, Header("��]�^��")]
    private bool _isRotating = false;
    [SerializeField]
    private float _angularVelocity = 45f;
    [SerializeField]
    private Vector3 _rotationCenterOffset = Vector3.zero;
    [SerializeField, Header("�����^��")]
    private bool _isReciprocating = false;
    [SerializeField]
    private float _reciprocateSpeed = 3f;
    [SerializeField]
    private float _reciprocateDistance = 3f;
    [SerializeField]
    private float _reciprocateAngle = 0f;

    [Header("�O�ՁE��]���̕`��")]
    [SerializeField] private LineRenderer _pathLine;
    [SerializeField] private GameObject _centerMarker;
    [SerializeField] private GameObject _startMarker;
    [SerializeField] private GameObject _endMarker;

    private Rigidbody2D _rigidbody2D;
    private TimeManager _timeManager;
    private bool _isMovingForward = true; // ��������

    private LineRenderer _pathLineRenderer;
    private GameObject _rotationCenterMarker;
    private GameObject _reciprocateStartMarker;
    private GameObject _reciprocateEndMarker;

    private Vector3 _centerPosition;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _timeManager = GetComponent<TimeManager>();

        // �����ʒu���L�^
        _startPosition = transform.position;

        //InitializeVisuals();
        //UpdateVisuals();
    }

    private void Update()
    {
        //UpdateVisuals();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ���Ԃ̒�~�E�t�Đ����͉����E��]�������s��Ȃ�
        if (_timeManager.isPaused || _timeManager.isRewinding) return;

        if (_isRotating)
        {
            _centerPosition = RotateObject();
        }

        if (_isReciprocating)
        {
            _endPosition = ReciprocateObject();
        }
    }

    public Vector3 RotateObject()
    {
        // ��]�̒��S�_�����[���h���W�n�Ōv�Z
        Vector3 worldRotationCenter = transform.position + transform.TransformDirection(_rotationCenterOffset);

        // ��]�p�x���v�Z (Time.fixedDeltaTime���g���Ĉ�葬�x���ێ�)
        float angle = _angularVelocity * Time.fixedDeltaTime;

        // ���݂̃I�u�W�F�N�g�̈ʒu����]���S����ɉ�]
        Vector3 direction = transform.position - worldRotationCenter; // ���S�_����̕����x�N�g��
        Quaternion rotation = Quaternion.Euler(0, 0, angle); // Z����]
        Vector3 rotatedDirection = rotation * direction; // �����x�N�g������]

        // �V�����ʒu���v�Z
        Vector3 newPosition = worldRotationCenter + rotatedDirection;

        // Rigidbody2D���g���ĐV�����ʒu��K�p
        _rigidbody2D.MovePosition(newPosition);

        // �I�u�W�F�N�g���̂���]������
        _rigidbody2D.MoveRotation(_rigidbody2D.rotation + angle);

        return worldRotationCenter;
    }


    public Vector3 ReciprocateObject()
    {
        // �p�x�����W�A���ɕϊ����ĕ����x�N�g�����v�Z
        float angleRad = _reciprocateAngle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0).normalized;

        // �w�肵�������܂Ői�ނƐi�s������؂�Ԃ�
        float currentDistance = Vector3.Distance(transform.position, _startPosition);
        if (currentDistance >= _reciprocateDistance)
        {
            _isMovingForward = !_isMovingForward;
        }

        // �I�u�W�F�N�g�ɑ��x��^����
        // �ʒu�ɂ�鐧��͑��̃I�u�W�F�N�g�Ƃ̐ڐG�ŕs����ɂȂ�
        Vector3 _velocity = direction * _reciprocateSpeed * (_isMovingForward ? 1 : -1);
        _rigidbody2D.linearVelocity = _velocity;

        return transform.position;
    }


    /// <summary>
    /// �ȉ��̓I�u�W�F�N�g�̋O�ՁE��]�̒��S�_��`�悷��R�[�h
    /// </summary>
    private void InitializeVisuals()
    {
        if (_pathLineRenderer == null)
        {
            _pathLineRenderer = CreateLine(Color.green);
        }
        else 
        {
            _pathLineRenderer = Instantiate(_pathLine);
        }

        if (_rotationCenterMarker == null)
        {
            _rotationCenterMarker = CreateMarker("RotationCenterMarker", Color.cyan);
        }
        else 
        {
            _rotationCenterMarker = Instantiate( _centerMarker);
        }

        if (_reciprocateStartMarker == null)
        {
            _reciprocateStartMarker = CreateMarker("ReciprocateStartMarker", Color.red);
        }
        else
        {
            _reciprocateStartMarker = Instantiate( _startMarker);
        }

        if (_reciprocateEndMarker == null)
        {
            _reciprocateEndMarker = CreateMarker("ReciprocateEndMarker", Color.yellow);
        }
        else
        {
            _reciprocateEndMarker = Instantiate(_endMarker);
        }
    }

    private LineRenderer CreateLine(Color color)
    {
        GameObject lineObj = new GameObject("PathLineRenderer");
        _pathLineRenderer = lineObj.AddComponent<LineRenderer>();
        _pathLineRenderer.positionCount = 2;
        _pathLineRenderer.startWidth = 0.05f;
        _pathLineRenderer.endWidth = 0.05f;
        _pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _pathLineRenderer.startColor = Color.green;
        _pathLineRenderer.endColor = Color.green;

        return _pathLineRenderer;
    }

    private GameObject CreateMarker(string name, Color color)
    {
        GameObject markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        markerObj.name = name;
        markerObj.transform.localScale = Vector3.one * 0.2f;

        // �}�e���A����ݒ肵�ĐF���w��
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        mat.renderQueue = 3100;
        markerObj.GetComponent<Renderer>().material = mat;

        return markerObj;
    }

    private void UpdateVisuals()
    {
        // �����^���̋O�Ղ�\��
        if (_isReciprocating && (_pathLineRenderer != null))
        {
            _pathLineRenderer.enabled = true;
            _reciprocateEndMarker.SetActive(true);
            _reciprocateStartMarker.SetActive(true);

            _pathLineRenderer.SetPosition(0, _startPosition);
            _pathLineRenderer.SetPosition(1, transform.position);

            if (_reciprocateStartMarker != null)
            {
                _reciprocateStartMarker.transform.position = _startPosition;
            }

            if (_reciprocateEndMarker != null)
            {
                _reciprocateEndMarker.transform.position = transform.position;
            }
        }

        // ��]���S��\��
        if (_isRotating && (_rotationCenterMarker != null))
        {
            _rotationCenterMarker.SetActive(true);

            _rotationCenterMarker.transform.position = _centerPosition;
        }

        if (!_isReciprocating)
        {
            _pathLineRenderer.enabled = false;
            _reciprocateEndMarker.SetActive(false);
            _reciprocateStartMarker.SetActive(false);
        }

        if (!_isRotating)
        {
            _rotationCenterMarker.SetActive(false);
        }

    }
}
