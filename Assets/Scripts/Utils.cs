using System.Collections.Generic;

public static class Utils
{
    // 4분음표 시간 구하기
    public static float GetCrotchetInterval(float bpm, float pitch)
    {
        return (60f / bpm) * pitch;
    }

    // 1박자에 30도씩 회전하도록 속도 계산
    public static float CalculateSpeed(float bpm, float pitch)
    {
        return 30f / GetCrotchetInterval(bpm, pitch);
    }

    // 주어진 이벤트리스트에서 특정 시간에 해당하는 첫번째 이벤트의 인덱스 찾기
    public static int FindIndexOfFirstEvent<T>(List<T> eventList, double targetTime) where T : GameEvent
    {
        int result = eventList.Count - 1;
        for (int i = 0; i < eventList.Count; i++)
        {
            T evt = eventList[i];
            if (evt.timeOffset >= targetTime)
            {
                result = i;
                break;
            }
        }
        return result;
    }
}
