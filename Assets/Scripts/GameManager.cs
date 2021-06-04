using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum GameStateType
{
    Idle, Playing, Ready, Paused
}

public struct LoadSettings
{
    public bool isInternal;
    public int levelID;
    public string levelPath;
}

public class GameManager : MonoBehaviour
{
    public static LoadSettings loadSettings;    // 레벨 로드 설정

    public static GameManager Instance;         // 싱글턴 인스턴스

    public double inputOffset { get; private set; }
    public double startTime;

    public GameStateType gameState { get; private set; }

    public event Action OnGameStart;    // 게임 시작 이벤트
    public event Action OnGamePause;    // 게임 일시정지 이벤트
    public event Action OnGameReady;    // 게임 일시정지 -> 준비 이벤트
    public event Action OnGameResume;   // 게임 재개 이벤트
    public event Action OnGameEnd;      // 게임 끝났을 때 이벤트

    public AudioSource musicSource;     // 음악 AudioSource 컴포넌트
    public float musicProgress          // 음악 진행 (%)
    {
        get
        {
            if (!isMusicEnd)
            {
                return (musicSource.time / musicSource.clip.length);
            }
            else
            {
                // 음악이 끝났다면 그냥 1 리턴
                return 1;
            }
        }
    }

    public Text debugText;


    private IEnumerator musicEndCoroutine;
    private bool isMusicEnd = false;    // 음악이 끝났는가?
    private bool isGameEnd = false;     // 게임이 끝났는가?
    private double lastEventTime;       // 마지막 게임이벤트의 시간

    private TimeManager timeManager;
    private DataManager dataManager;
    public LevelData currentLevel { get; private set; } // 현재 로드된 레벨

    public TimingData timingData;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
        {
            Destroy(gameObject);
        }

        if (loadSettings.isInternal)
        {
            LoadInternalLevel(loadSettings.levelPath);
        }

        // 게임 상태 설정
        gameState = GameStateType.Idle;

        Time.timeScale = 1;
        AudioListener.pause = false;

        // 커서 안 보이게 + 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LoadInternalLevel(string levelPath)
    {
        // 레벨 로드하기
        TextAsset levelFile = Resources.Load<TextAsset>("Levels/" + levelPath);
        currentLevel = LevelLoader.Load(levelFile.text);
        timingData = currentLevel.timingData;

        // 음악파일 리소스 로드
        try
        {
            string musicResPath = "Musics/" + currentLevel.settings.musicName;
            musicSource.clip = Resources.Load<AudioClip>(musicResPath);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        // 마지막 게임이벤트 시간 구하기
        var lastEvent = timingData.GetLastEvent();
        if (lastEvent.isContinuous)
        {
            lastEventTime = lastEvent.endTimeOffset;
        }
        else
        {
            lastEventTime = lastEvent.timeOffset;
        }
        lastEventTime += 1.0;

        // 로드된 이벤트들 출력
        foreach (GameEvent evt in timingData.GetEvents())
        {
            Debug.Log(evt.GetType() + " " + evt);
        }
    }

    private List<GameEvent> gameEvents;

    void Start()
    {
        timeManager = TimeManager.Instance;
        dataManager = DataManager.Instance;

        inputOffset = dataManager.saveData.inputOffset;

        gameEvents = new List<GameEvent>(timingData.GetEvents());

        OnGameStart += () =>
        {
            eventIndex = Utils.FindIndexOfFirstEvent(gameEvents, startTime);
        };
    }

    private void Update()
    {
        // 게임 상태에 따라 업데이트 다르게 하기
        switch (gameState)
        {
            // 시작하기 전에 대기 중이라면
            case GameStateType.Idle:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    musicSource.time = (float)startTime;
                    musicSource.PlayScheduled(AudioSettings.dspTime);
                    if (musicEndCoroutine != null)
                    {
                        StopCoroutine(musicEndCoroutine);
                    }
                    musicEndCoroutine = WaitForMusicEnd();
                    StartCoroutine(musicEndCoroutine);

                    TimeManager.Instance.StartByNow(startTime + currentLevel.settings.offset - inputOffset);

                    OnGameStart?.Invoke();
                    gameState = GameStateType.Playing;
                }
                break;

            // 게임 플레이중이라면
            case GameStateType.Playing:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    AudioListener.pause = true;
                    Time.timeScale = 0;

                    // 커서 보이게 + 잠금 해제
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    OnGamePause?.Invoke();
                    gameState = GameStateType.Paused;
                }

                if (timeManager.gameTime > lastEventTime)
                {
                    if (!isGameEnd)
                    {
                        isGameEnd = true;
                        OnGameEnd?.Invoke();
                    }
                }
                break;

            // 일시중지 상태라면
            case GameStateType.Paused:
                break;
        }

        TestDebugText();


    }

    // 게임 다시시작 (씬 다시 불러오기)
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 게임 일시중지 해제
    public void ResumeGame()
    {
        gameState = GameStateType.Ready;

        // 커서 안 보이게 + 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        OnGameReady?.Invoke();
        StartCoroutine(ResumeReadyRoutine());
    }

    // 음악 재생이 끝날 때까지 대기하는 코루틴
    private IEnumerator WaitForMusicEnd()
    {
        yield return new WaitUntil(() => (!musicSource.isPlaying && isGameEnd));

        isMusicEnd = true;
    }

    // 일시중지 되었다가 다시 플레이할때의 코루틴
    private IEnumerator ResumeReadyRoutine()
    {
        double startDspTime = AudioSettings.dspTime;

        yield return new WaitForSecondsRealtime(1.0f);

        gameState = GameStateType.Playing;

        Time.timeScale = 1;
        AudioListener.pause = false;
        OnGameResume?.Invoke();
    }

    // 디버그 텍스트
    int eventIndex = 0;
    private void TestDebugText()
    {
        if (gameState != GameStateType.Playing || eventIndex >= gameEvents.Count) return;

        GameEvent currentEvent = gameEvents[eventIndex];
        GameEvent actualEvent = (eventIndex > 0) ? gameEvents[eventIndex - 1] : gameEvents[0];
        debugText.text = string.Format("Current Beat: {0}", actualEvent.beatOffset);

        if (timeManager.gameTime >= currentEvent.timeOffset)
        {
            eventIndex++;
        }

        /*debugText.text = string.Format("{0:F4} / {1:F4}", timeManager.gameTime, musicSource.time - inputOffset);*/
    }


}
