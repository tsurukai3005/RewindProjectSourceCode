using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraManagerEndlessStage : MonoBehaviour
{
    [SerializeField] private PlayerAction _player;
    [SerializeField] float upfollowSpeed = 200.0f; // 追従速度を調整
    [SerializeField] float downfollowSpeed = 20.0f; // 追従速度を調整
    [SerializeField] float DistancePlayerToCamera = 3f;
    [SerializeField] float CameraXOffset = -2.7f;
    private Vector3 _cameraPos;
    private Vector3 _targetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_player == null)
        {
            Debug.LogError("PlayerActionがアサインされていません。");
        }

        _cameraPos = transform.position;
    }

    private void FixedUpdate()
    {
        _cameraPos = transform.position;
        _FollowPlayer();
    }

    private void _FollowPlayer()
    {
        if (_player == null) return;

        // ターゲット位置の設定（z軸はカメラの位置を維持）
        _targetPos.x = CameraXOffset;
        _targetPos.y = _player.transform.position.y + DistancePlayerToCamera;
        _targetPos.z = transform.position.z;

        if (transform.position.y < _targetPos.y)
        {
            // MoveTowardsで一定速度で追従
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPos,
                upfollowSpeed * Time.deltaTime
            );
        }
        else
        {
            // MoveTowardsで一定速度で追従
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPos,
                downfollowSpeed * Time.deltaTime
            );
        }
    }
}
