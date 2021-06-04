using UnityEngine;

// 시침 표시
public class HourClockhand : MonoBehaviour
{
    public float divider;           // 각도를 얼마로 나눈 값으로 적용할건가?
    private Clockhand clockhand;

    void Start()
    {
        clockhand = Clockhand.Instance;
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, clockhand.easingAngle / divider);
    }
}
