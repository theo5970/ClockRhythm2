using UnityEngine;

// 카메라 이동 이벤트처리
public class CameraMoveProcessor : EventProcessor<CameraMoveEvent>
{
    private Camera cam;         // 카메라
    private Vector2 startPos;   // 이동 시작위치
    private Vector3 initPos;    // 초기 위치

    public override void OnEventBegin(CameraMoveEvent evt)
    {
        // 이동 시작위치 저장
        startPos = cam.transform.position;
    }

    public override void OnEventEnd(CameraMoveEvent evt)
    {
        // 이벤트가 끝났으니 정확하게 한 번 더 적용
        Vector3 newPos = startPos + evt.moveOffset;
        newPos.z = -10;
        cam.transform.position = newPos;
    }

    public override void OnEventUpdate(CameraMoveEvent evt, float t)
    {
        // t를 통해 보간된 위치를 카메라에 적용
        Vector3 newPos = EasingVector2(evt.easingFunc, startPos, startPos + evt.moveOffset, t);
        newPos.z = -10;
        cam.transform.position = newPos;
    }

    public override void Start()
    {
        base.Start();

        cam = Camera.main;
        startPos = cam.transform.position;
        initPos = cam.transform.position;
    }

    public override void Init()
    {
        cam.transform.position = initPos;
    }

    // 2차원 벡터를 가감속 함수로 보간
    private Vector3 EasingVector2(EasingFunc func, Vector2 a, Vector2 b, float t)
    {
        float x = func(a.x, b.x, t);
        float y = func(a.y, b.y, t);
        return new Vector3(x, y, 0);
    }
}
