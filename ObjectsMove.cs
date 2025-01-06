using System.Runtime.CompilerServices;
using UnityEngine;

public class ObjectsMove : MonoBehaviour
{
    [SerializeField, Header("回転運動")]
    private bool _isRotating = false;
    [SerializeField]
    private float _angularVelocity = 45f;
    [SerializeField]
    private Vector3 _rotationCenterOffset = Vector3.zero;
    [SerializeField, Header("往復運動")]
    private bool _isReciprocating = false;
    [SerializeField]
    private float _reciprocateSpeed = 3f;
    [SerializeField]
    private float _reciprocateDistance = 3f;
    [SerializeField]
    private float _reciprocateAngle = 0f;

    [Header("軌跡・回転軸の描画")]
    [SerializeField] private LineRenderer _pathLine;
    [SerializeField] private GameObject _centerMarker;
    [SerializeField] private GameObject _startMarker;
    [SerializeField] private GameObject _endMarker;

    private Rigidbody2D _rigidbody2D;
    private TimeManager _timeManager;
    private bool _isMovingForward = true; // 往復方向

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

        // 初期位置を記録
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
        // 時間の停止・逆再生中は往復・回転処理を行わない
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
        // 回転の中心点をワールド座標系で計算
        Vector3 worldRotationCenter = transform.position + transform.TransformDirection(_rotationCenterOffset);

        // 回転角度を計算 (Time.fixedDeltaTimeを使って一定速度を維持)
        float angle = _angularVelocity * Time.fixedDeltaTime;

        // 現在のオブジェクトの位置を回転中心を基準に回転
        Vector3 direction = transform.position - worldRotationCenter; // 中心点からの方向ベクトル
        Quaternion rotation = Quaternion.Euler(0, 0, angle); // Z軸回転
        Vector3 rotatedDirection = rotation * direction; // 方向ベクトルを回転

        // 新しい位置を計算
        Vector3 newPosition = worldRotationCenter + rotatedDirection;

        // Rigidbody2Dを使って新しい位置を適用
        _rigidbody2D.MovePosition(newPosition);

        // オブジェクト自体も回転させる
        _rigidbody2D.MoveRotation(_rigidbody2D.rotation + angle);

        return worldRotationCenter;
    }


    public Vector3 ReciprocateObject()
    {
        // 角度をラジアンに変換して方向ベクトルを計算
        float angleRad = _reciprocateAngle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0).normalized;

        // 指定した距離まで進むと進行方向を切り返す
        float currentDistance = Vector3.Distance(transform.position, _startPosition);
        if (currentDistance >= _reciprocateDistance)
        {
            _isMovingForward = !_isMovingForward;
        }

        // オブジェクトに速度を与える
        // 位置による制御は他のオブジェクトとの接触で不安定になる
        Vector3 _velocity = direction * _reciprocateSpeed * (_isMovingForward ? 1 : -1);
        _rigidbody2D.linearVelocity = _velocity;

        return transform.position;
    }


    /// <summary>
    /// 以下はオブジェクトの軌跡・回転の中心点を描画するコード
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

        // マテリアルを設定して色を指定
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        mat.renderQueue = 3100;
        markerObj.GetComponent<Renderer>().material = mat;

        return markerObj;
    }

    private void UpdateVisuals()
    {
        // 往復運動の軌跡を表示
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

        // 回転中心を表示
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
