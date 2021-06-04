using UnityEngine;
using UnityEngine.UI;

// 시계를 좀 더 디지털틱하게 하기 위한 텍스트
public class AutoplayText : MonoBehaviour
{
    private Clockhand clockhand;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        clockhand = Clockhand.Instance;
    }

    void Update()
    {
        int angle = -1 * Mathf.FloorToInt(clockhand.easingAngle);

        // 각도를 기반으로 시간 계산 (분 단위)
        int time = angle / 6;

        // 만약 역방향으로 가서 음수라면
        // 24시간부터 거꾸로 가게한다.
        while (time < 0)
        {
            time += 1440;
        }

        int former = (time / 60) % 24;  // 시 (Hour) -> 24시간 주기
        int latter = time % 60;         // 분 (Minute)

        // 00:00 처럼 표시
        text.text = former.ToString().PadLeft(2, '0') + ":" + latter.ToString().PadLeft(2, '0');

    }
}
