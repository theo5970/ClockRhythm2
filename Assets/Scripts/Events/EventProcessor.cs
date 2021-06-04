using UnityEngine;
using System.Collections.Generic;

// GameEvent를 인게임에서 실시간 처리하기 위한 추상 제네릭 클래스
public abstract class EventProcessor<T> : MonoBehaviour where T : GameEvent
{
    #region 필드
    private GameManager gameManager;
    private TimeManager timeManager;

    private TimingData timingData;
    private List<T> events;

    private int eventIndex = 0;
    private bool isEventJustBegin = false;      // 이벤트가 시작했는지에 대한 여부
    #endregion
    // Start 함수 (자식 클래스에서 사용하기 위해 virtual 키워드 사용)
    public virtual void Start()
    {
        gameManager = GameManager.Instance;
        timeManager = TimeManager.Instance;

        // 게임 시작 이벤트
        gameManager.OnGameStart += GameManager_OnGameStart;

        timingData = gameManager.timingData;
        events = timingData.GetEventsList<T>();

        isEventJustBegin = true;
    }

    private void GameManager_OnGameStart()
    {
        PreviewAt(gameManager.startTime);
    }

    // 특정 시간에서 미리보기 기능
    public void PreviewAt(double startTime)
    {
        Init(); // 먼저 초기화

        // T 타입 이벤트 중 첫 번째 찾기
        int startIndex = Utils.FindIndexOfFirstEvent(events, startTime);

        if (events.Count == 0)
        {
            return;
        }

        for (int i = startIndex; i < events.Count; i++)
        {
            var evt = events[i];
            if (evt.isContinuous)   // 길이가 있는 이벤트라면
            {
                // 이벤트가 시작되었다면
                if (evt.timeOffset < gameManager.startTime)
                {
                    // 시작되었다고 알리기
                    OnEventBegin(evt);

                    // 이벤트가 끝났다면
                    if (evt.endTimeOffset < gameManager.startTime)
                    {
                        OnEventEnd(evt);
                    }
                    else // 이벤트가 진행중이라면
                    {
                        // 이벤트 길이
                        float duration = (float)(evt.endTimeOffset - evt.timeOffset);

                        // 0 ~ 1의 값으로 시간 조정
                        float t = (float)(gameManager.startTime - evt.timeOffset) / duration;

                        // 진행중이라고 알리기
                        OnEventUpdate(evt, t);
                    }
                }
            }
            else   // 길이가 0인 이벤트라면
            {
                // 이벤트가 이미 진행되었다면
                if (evt.timeOffset < gameManager.startTime)
                {
                    // 시작과 끝을 순서대로 알리기
                    OnEventBegin(evt);
                    OnEventEnd(evt);
                }
            }
        }
    }

    void Update()
    {
        // 이벤트가 하나도 없거나 다음 이벤트가 존재하지 않는 경우 생략
        if (events.Count == 0 || eventIndex >= events.Count)
            return;

        // 현재 이벤트를 가져온다.
        var evt = events[eventIndex];


        if (evt.isContinuous)   // 길이가 있는 이벤트라면
        {
            // 이벤트가 끝났다면 (현재 시간 > 이벤트 종료시간)
            if (timeManager.gameTime > evt.endTimeOffset)
            {
                OnEventEnd(evt);        // 이벤트가 종료되었다고 알리기
                isEventJustBegin = true;
                eventIndex++;
            }
            // 이벤트가 진행 중이라면 (현재 시간 >= 이벤트 시작시간)
            else if (timeManager.gameTime >= evt.timeOffset)
            {
                // 이벤트의 길이 구하기
                float duration = (float)(evt.endTimeOffset - evt.timeOffset);

                // 이벤트의 길이를 기반으로 t(시간)를 0~1의 값으로 바꾸기
                float t = (float)(timeManager.gameTime - evt.timeOffset) / duration;

                if (isEventJustBegin)  
                {
                    // 이벤트가 방금 시작되었다면 시작되었다고 알리기
                    OnEventBegin(evt);
                    isEventJustBegin = false;
                }
                else
                {
                    // 이벤트가 진행 중이라고 알리기
                    OnEventUpdate(evt, t);
                }
            }
        }
        else  // 길이가 없는 이벤트라면
        {
            // 이벤트가 이미 진행되었다면 
            if (timeManager.gameTime > evt.timeOffset)
            {
                // 시작과 끝을 순서대로 알리기
                OnEventBegin(evt);
                OnEventEnd(evt);

                eventIndex++;
            }
        }
    }

    // 추상 메서드들
    public abstract void Init();
    public abstract void OnEventBegin(T evt);
    public abstract void OnEventUpdate(T evt, float t);
    public abstract void OnEventEnd(T evt);
}
