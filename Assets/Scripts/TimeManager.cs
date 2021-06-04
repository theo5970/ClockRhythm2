using UnityEngine;

// 게임 플레이 중 정확한 시간을 계산하기 위한 매니저 클래스
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    // Time.unscaledDeltaTime으로 보간된 dspTime (더 부드럽게)
    public double interpolatedDspTime { get; private set; }

    // 노트, 판정 등에 사용되는 실제 시간 (0초부터)
    public double gameTime { get; private set; }

    private bool hasStarted = false;

    public double gameTimeOffset                    // 게임시간 오프셋
    {
        get;
        private set;
    }

    private float previousFrameTime;                // 이전 프레임 시간
    private double lastReportedPlayheadPosition;    // 이전 dspTime

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        gameTime = 0;
        gameTimeOffset = 0;
        hasStarted = false;

        AudioListener.pause = false;
        Time.timeScale = 1;
    }

    // 현재 시간을 기준으로 시간 측정 시작
    public void StartByNow()
    {
        gameTimeOffset = AudioSettings.dspTime;
        hasStarted = true;
    }

    // 현재 시간을 기준으로 시간 측정 시작 (오프셋 포함)
    public void StartByNow(double offset)
    {
        gameTimeOffset = AudioSettings.dspTime - offset;
        hasStarted = true;
    }

    void Update()
    {
        if (!hasStarted) return;

        // 보간 (interpolation)
        float unscaledDeltaTime = Time.unscaledTime - previousFrameTime;
        if (!AudioListener.pause && unscaledDeltaTime < 0.1f)
        {
            interpolatedDspTime += unscaledDeltaTime;
        }

        previousFrameTime = Time.unscaledTime;

        // dspTime이 변화했으면 보간된 시간 변수에도 적용한다.
        if (AudioSettings.dspTime != lastReportedPlayheadPosition)
        {
            lastReportedPlayheadPosition = AudioSettings.dspTime;
            interpolatedDspTime = AudioSettings.dspTime;
        }

        // 게임 시간 = (보간된 시간) - (게임시간 오프셋)
        gameTime = interpolatedDspTime - gameTimeOffset;
    }
}

