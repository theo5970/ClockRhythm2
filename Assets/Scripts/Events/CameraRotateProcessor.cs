using UnityEngine;

// 카메라 회전 이벤트처리
public class CameraRotateProcessor : EventProcessor<CameraRotateEvent>
{
    private float previousAngle;    // 이전 각도
    private float targetAngle;      // 목표 각도
    private Camera cam;             // 카메라

    public override void Start()
    {
        base.Start();
        cam = Camera.main;

        previousAngle = 0;
    }

    public override void Init()
    {
        transform.rotation = Quaternion.identity;
    }

    public override void OnEventBegin(CameraRotateEvent evt)
    {
        // 목표 각도 = 이전 각도 + 회전량
        targetAngle = previousAngle + evt.rotateAmount;
    }

    public override void OnEventEnd(CameraRotateEvent evt)
    {
        // 정확하게 다시 한 번 적용
        cam.transform.rotation = Quaternion.Euler(0, 0, targetAngle);

        // 오버플로 방지를 위해 이전 각도를 360도로 나눈 값으로 저장
        previousAngle = targetAngle % 360.0f;
    }

    public override void OnEventUpdate(CameraRotateEvent evt, float t)
    {
        // t 값을 통해 회전 각도 보간

        float zAngle = evt.easingFunc(previousAngle, targetAngle, t);
        cam.transform.rotation = Quaternion.Euler(0, 0, zAngle);
    }


}
