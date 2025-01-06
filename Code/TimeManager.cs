using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField, Header("TimeSE")]
    private AudioSource _timeSE;
    private bool _SEIsPlaying;

    public bool isRewinding = false;
    public bool isPaused = false;
    public bool isRunning = false;

    public float recordTime { get; private set; } = 10f; // 再生可能時間：必ずここで編集すること

    public float elapsedTime = 0f;
    public bool overMaxTime; // 時間の上限を超えると再生不可
    public bool underMinTime; // 時間がゼロ以下になると逆再生不可

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = true;
        underMinTime = true;
        overMaxTime = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        CountTime();
        TimeRangeJudgment();
    }

    private void HandleInput()
    {
        if (CanPlay())
        {
            StartPlaying();
        }
        else if (CanRewind())
        {
            StartRewinding();
        }
        else
        {
            PauseTime();
        }
    }

    private bool CanPlay()
    {
        bool isRightArrow = Input.GetKey(KeyCode.RightArrow);
        bool isLeftClick = Input.GetMouseButton(0);
        bool isLeftArrow = Input.GetKey(KeyCode.LeftArrow);
        bool isRightClick = Input.GetMouseButton(1);

        return (isRightArrow || isLeftClick) && !(isLeftArrow || isRightClick) && !overMaxTime;
    }

    private bool CanRewind()
    {
        bool isLeftArrow = Input.GetKey(KeyCode.LeftArrow);
        bool isRightClick = Input.GetMouseButton(1);
        bool isRightArrow = Input.GetKey(KeyCode.RightArrow);
        bool isLeftClick = Input.GetMouseButton(0);

        return (isLeftArrow || isRightClick) && !(isRightArrow || isLeftClick) && !underMinTime;
    }

    private void StartPlaying()
    {
        isRunning = true;
        isRewinding = false;
        isPaused = false;

        if (_timeSE != null && !_SEIsPlaying && Time.timeScale != 0)
        {
            _timeSE.PlayOneShot(_timeSE.clip);
            _SEIsPlaying = true;
        }
    }

    private void StartRewinding()
    {
        isRunning = false;
        isRewinding = true;
        isPaused = false;

        if (_timeSE != null && !_SEIsPlaying && Time.timeScale != 0)
        {
            _timeSE.PlayOneShot(_timeSE.clip);
            _SEIsPlaying = true;
        }
    }

    private void PauseTime()
    {
        isRunning = false;
        isRewinding = false;
        isPaused = true;

        _SEIsPlaying = false;
    }

    void CountTime()
    {
        if (isRunning)
        {
            elapsedTime += Time.fixedDeltaTime;
        }
        else if (isRewinding)
        {
            elapsedTime -= Time.fixedDeltaTime;
        }
    }

    void TimeRangeJudgment()
    {
        if (elapsedTime > recordTime)
        {
            overMaxTime = true;
        }
        else if (elapsedTime <= 0)
        {
            underMinTime = true;
        }
        else
        {
            overMaxTime = false;
            underMinTime = false;
        }
    }
}
