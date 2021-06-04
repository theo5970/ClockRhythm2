using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clockhand : MonoBehaviour
{
    public static Clockhand Instance;

    void Awake()
    {
        // 싱글턴 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
        {
            Destroy(gameObject);
        }
    }

    public AnimationCurve handCurve;
    public float speedPerSecond = 30f;

    private GameManager gameManager;
    private TimeManager timeManager;
    private TimingData timingData;
    private List<NoteEvent> noteEvents;

    // 가감속 함수 대리자
    private EasingFunc movingFunc = EasingFunction.Linear;

    public float angle { get; private set; }            // 역방향 적용한 각도
    public float forwardAngle { get; private set; }     // 순방향 각도
    public float easingAngle { get; private set; }      // 역방향+가감속 적용한 각도

    void Start()
    {
        timeManager = TimeManager.Instance;

        gameManager = GameManager.Instance;
        gameManager.OnGameStart += () =>
        {
            // 첫번째 노트의 인덱스를 구하고 시간을 lastTime에 저장한다.
            noteIndex = Utils.FindIndexOfFirstEvent(noteEvents, gameManager.startTime);

            int previousEventIndex = noteIndex - 1;
            if (previousEventIndex >= 0)
            {
                lastTime = noteEvents[previousEventIndex].timeOffset;
            }
            else
            {
                lastTime = 0;
            }
        };

        timingData = gameManager.timingData;
        noteEvents = timingData.GetEventsList<NoteEvent>();
    }

    private void Update()
    {
        if (gameManager.gameState == GameStateType.Playing)
        {
            // 게임 플레이중이라면 각도를 계산하고 회전에 적용
            CalculateAngle();

            transform.rotation = Quaternion.Euler(0, 0, easingAngle);
        }
    }

    private int noteIndex = 0;
    private double lastTime = 0;

    public void SetMovingFunction(EasingFunc func)
    {
        movingFunc = func;
    }

    private void CalculateAngle()
    {
        // 역방향 적용한 시계 각도
        angle = (float)timingData.CalculatePosition(timeManager.gameTime);

        // 순방향 적용한 시계 각도
        forwardAngle = (float)timingData.CalculatePosition(timeManager.gameTime, true);


        // 시계바늘의 움직임을 Easing Function으로 할 수 있게 한다.
        if (noteIndex < noteEvents.Count)
        {
            NoteEvent currentEvent = noteEvents[noteIndex];

            // 현재 노트이벤트와 이전 노트이벤트의 시간차
            double deltaEventTime = (currentEvent.timeOffset - lastTime);
            float t = 0;

            if (deltaEventTime != 0)
            {
                // 현재 노트이벤트와 게임시간으로 t를 0~1 사이로 맞춘다.
                t = 1.0f - (float)((currentEvent.timeOffset - timeManager.gameTime) / deltaEventTime);

                // t를 0 ~ 1 사이로 범위 제한
                t = Mathf.Clamp(t, 0, 1);
            }

            // 가감속 함수를 이용해 새로 시간 계산
            double newGameTime = lastTime + (movingFunc(0, 1, t) * deltaEventTime);

            // 역방향+가감속 적용한 각도를 계산한다.
            easingAngle = (float)timingData.CalculatePosition(newGameTime);

            // 만약 현재 노트이벤트의 시간이 지났다면 다음 노트이벤트를 가져온다.
            if (timeManager.gameTime > currentEvent.timeOffset)
            {
                noteIndex++;
                lastTime = currentEvent.timeOffset;
            }
        }
        else
        {
            easingAngle = angle;
        }


    }

}
