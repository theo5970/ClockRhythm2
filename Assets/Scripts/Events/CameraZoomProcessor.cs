using UnityEngine;

// 카메라 확대/축소 이벤트처리
public class CameraZoomProcessor : EventProcessor<CameraZoomEvent>
{
    public float defaultZoom = 15;  // 기본 줌 값
    private Camera cam;             // 카메라

    public override void OnEventBegin(CameraZoomEvent evt) { }
    public override void OnEventEnd(CameraZoomEvent evt)
    {
        // 정확하게 다시 한 번 적용
        cam.orthographicSize = defaultZoom * (evt.endZoom / 100f);
    }
    public override void OnEventUpdate(CameraZoomEvent evt, float t)
    {
        // t 값으로 시작 ~ 종료 크기를 보간한다.
        float interpolatedZoom = evt.easingFunc(evt.startZoom, evt.endZoom, t);

        // 이벤트에서는 퍼센트(%) 단위이므로 기본 크기에 (보간된 크기 / 100)을 적용한다.
        cam.orthographicSize = defaultZoom * (interpolatedZoom / 100f);
    }

    public override void Start()
    {
        base.Start();
        cam = Camera.main;
    }

    public override void Init()
    {
        cam.orthographicSize = defaultZoom;
    }
}
