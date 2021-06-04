using System;
using System.Collections.Generic;
using System.Linq;

public class TimingData
{
    private List<GameEvent> events;
    private List<SpeedChangeData> cachedSpeedChanges;

    public double initSpeed;    // 초기 속도
    public double initBpm;      // 초기 BPM

    public TimingData()
    {
        events = new List<GameEvent>();
        cachedSpeedChanges = new List<SpeedChangeData>();
    }

    // 이벤트 추가
    public void AddEvent(GameEvent eventToAdd)
    {
        events.Add(eventToAdd);
    }

    // 특정 타입에 맞는 이벤트 찾기 (시작 인덱스 포함)
    public T FindEvent<T>(int startIndex = 0)
    {
        for (int i = startIndex; i < events.Count; i++)
        {
            if (events[i] is T result) return result;
        }
        return default;
    }

    // 특정 시간에 해당하는 첫 번째 이벤트 찾기
    public T FindFirstEventByTime<T>(double timeOffset) where T : GameEvent
    {
        foreach (T evt in GetEvents<T>())
        {
            if (evt.timeOffset >= timeOffset)
            {
                return evt;
            }
        }
        return default;
    }

    // 마지막 이벤트 찾기
    public GameEvent GetLastEvent()
    {
        return events.Last();
    }

    // 모든 이벤트 가져오기
    public IEnumerable<GameEvent> GetEvents()
    {
        return events;
    }

    // 특정 타입에 대한 이벤트 가져오기
    public IEnumerable<T> GetEvents<T>()
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] is T result)
            {
                yield return result;
            }
        }
    }

    public List<T> GetEventsList<T>()
    {
        return new List<T>(GetEvents<T>());
    }

    // 특정 이벤트의 개수를 센다 (LINQ)
    public int CountEvent<T>()
    {
        return (from evt in events
                where evt is T
                select evt).Count();
    }

    /*// [Deprecated] 안 쓸 예정
    public double BeatToTime(double targetBeat)
    {
        double time = 0;
        double bpm = initBpm;

        bool isBpmEventExists = events.Exists(evt => evt is BpmChangeEvent);
        if (isBpmEventExists)
        {
            var firstBpmEvent = FindEvent<BpmChangeEvent>();

            if (targetBeat > firstBpmEvent.beatOffset)
            {
                time += firstBpmEvent.beatOffset * (60d / bpm);
            }
            else
            {
                time += targetBeat * (60d / bpm);
                return time;
            }

        }

        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] is BpmChangeEvent bpmEvent)
            {
                bpm = bpmEvent.newBpm;
                double crotchet = 60d / bpm;
                double deltaBeatCurrent = targetBeat - bpmEvent.beatOffset;

                var nextBpmEvent = FindEvent<BpmChangeEvent>(i + 1);
                if (deltaBeatCurrent > 0)
                {
                    if (nextBpmEvent != null)
                    {
                        double limitBeats = nextBpmEvent.beatOffset - bpmEvent.beatOffset;
                        deltaBeatCurrent = System.Math.Min(deltaBeatCurrent, limitBeats);
                    }
                    time += crotchet * deltaBeatCurrent;
                }
                else break;
            }
        }

        return time;
    }*/

    // 이벤트 시간을 계산하는 새로운 방식
    public void CalculateEventTimes()
    {
        // 박자 오프셋별 오름차순 정렬
        // 만약 박자 오프셋이 서로 같으면 우선순위 별로 내림차순 정렬한다.
        events.Sort((a, b) =>
        {
            int result = a.beatOffset.CompareTo(b.beatOffset);
            return result != 0 ? result : b.priority.CompareTo(a.priority);
        });

        double elapsedTime = 0;
        double currentBpm = initBpm;
        double previousBpm = initBpm;
        double previousBeatOffset = 0;

        int count = events.Count;
        var continuousEvents = new List<GameEvent>();
        
        for (int i = 0; i < count; i++)
        {
            GameEvent evt = events[i];

            elapsedTime += (evt.beatOffset - previousBeatOffset) * (60d / currentBpm);
            evt.timeOffset = elapsedTime;

            bool isBpmEvent = false;
            if (evt is BpmChangeEvent bpmEvent)                 // BPM 변경
            {
                isBpmEvent = true;
                currentBpm = bpmEvent.newBpm;
            }
            if (evt.isContinuous)                               // 길이가 있는 이벤트
            {
                evt.endTimeOffset = evt.timeOffset + evt.beatLength * (60d / currentBpm);
                continuousEvents.Add(evt);
            }
            else if (isBpmEvent)
            {
                // BPM 변화를 롱노트 타이밍에 적용한다.
                for (int k = 0; k < continuousEvents.Count; k++)
                {
                    var cevt = continuousEvents[k];
                    double deltaBeat = cevt.endBeatOffset - evt.beatOffset;

                    if (deltaBeat > 0)
                    {
                        double crotchetDelta = (60d / currentBpm) - (60d / previousBpm);
                        double timeToAdd = crotchetDelta * deltaBeat;
                        cevt.endTimeOffset += timeToAdd;
                    }
                    else
                    {
                        continuousEvents.RemoveAt(k);
                        k--;    // 중요! k번째가 제거되었기 때문에 다음 요소는 k번째에 있다.
                    }
                }
            }

            previousBpm = currentBpm;
            previousBeatOffset = evt.beatOffset;
        }

        CacheSpeedChanges();
    }

    // 속도 변경에 대한 데이터 구조체
    public struct SpeedChangeData
    {
        public double timeOffset;   // 시점
        public double realSpeed;    // 속도
        public SpeedChangeData(double timeOffset, double realSpeed)
        {
            this.timeOffset = timeOffset;
            this.realSpeed = realSpeed;
        }
    }

    // BPM과 박자당 각도로 실제 속도를 계산한다.
    // => [박자당 각도] / [한 박자 시간]
    private double GetRealSpeed(double bpm, double speed)
    {
        // return speed / (bpm / 60d);
        return (speed * bpm) / 60d;
    }

    // 속도 변경에 대한 데이터를 성능을 위해 미리 캐싱해둔다.
    private void CacheSpeedChanges()
    {
        cachedSpeedChanges.Clear();
        double currentSpeed = initSpeed;
        double currentBpm = initBpm;

        for (int i = 0; i < events.Count; i++)
        {
            GameEvent evt = events[i];
            switch (evt)
            {
                case BpmChangeEvent bpmEvent:
                    currentBpm = bpmEvent.newBpm;
                    cachedSpeedChanges.Add(new SpeedChangeData(evt.timeOffset, GetRealSpeed(currentBpm, currentSpeed)));
                    break;
                case SpeedChangeEvent speedEvent:
                    currentSpeed = speedEvent.newSpeed;
                    cachedSpeedChanges.Add(new SpeedChangeData(evt.timeOffset, GetRealSpeed(currentBpm, currentSpeed)));
                    break;
                case StopEvent stopEvent:
                    cachedSpeedChanges.Add(new SpeedChangeData(evt.timeOffset, 0));
                    cachedSpeedChanges.Add(new SpeedChangeData(stopEvent.endTimeOffset, GetRealSpeed(currentBpm, currentSpeed)));
                    break;
            }
        }

        // 시간 오프셋을 기준으로 오름차순 정렬
        cachedSpeedChanges.Sort((a, b) =>
        {
            return a.timeOffset.CompareTo(b.timeOffset);
        });
    }

    // 노트 시작위치를 계산하는 함수.
    public void CalculateNoteStartPositions()
    {
        foreach (INoteStart noteStart in GetEvents<INoteStart>())
        {
            GameEvent gameEvent = noteStart as GameEvent;
            noteStart.startNotePosition = CalculatePosition(gameEvent.timeOffset, true);
        }

        foreach (INoteEnd holdNote in GetEvents<INoteEnd>())
        {
            GameEvent gameEvent = holdNote as GameEvent;
            holdNote.endNotePosition = CalculatePosition(gameEvent.endBeatOffset, true);
        }
    }

    // 판정선 (or 노트) 위치를 계산하는 함수
    // ignoreInverse : 역방향을 무시할 것인지에 대한 여부 (노트 생성을 위해 추가)
    public double CalculatePosition(double targetTime, bool ignoreInverse = false)
    {
        int count = cachedSpeedChanges.Count;
        double previousSpeed = GetRealSpeed(initBpm, initSpeed);
        if (ignoreInverse)
        {
            previousSpeed = System.Math.Abs(previousSpeed);
        }

        double position = previousSpeed * targetTime;
        for (int i = 0; i < count; i++)
        {
            var speedChange = cachedSpeedChanges[i];
            if (speedChange.timeOffset > targetTime)
            {
                continue;
            }

            if (ignoreInverse)
            {
                speedChange.realSpeed = Math.Abs(speedChange.realSpeed);
            }

            position += (speedChange.realSpeed - previousSpeed) * (targetTime - speedChange.timeOffset);
            previousSpeed = speedChange.realSpeed;
        }

        return -1 * position;
    }
}