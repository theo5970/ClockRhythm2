using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour
{
    private ParticleSystem popParticle;

    private SpriteRenderer foreground;
    private SpriteRenderer background;
    private Transform foregroundTransform;

    private Clockhand clockhand;

    public double detectTime { get; private set; }
    private float detectPosition;
    private float detectForwardPosition;
    private Color backgroundColor = new Color(1, 1, 1, 0.7f);
    private Color backgroundStartColor = new Color(1, 1, 1, 0.5f);

    private bool isPopped = false;

    // 반지름, 각도를 가지고 위치 업데이트
    private void UpdatePosition(float radius, float angle)
    {
        angle += 90f;

        float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
        float y = radius * Mathf.Sin(Mathf.Deg2Rad * angle);

        transform.localPosition = new Vector3(x, y, 0);
    }


    // 이벤트를 매개변수로 받아 초기화
    public void Init(NoteEvent noteEvent)
    {
        GameManager gameManager = GameManager.Instance;

        detectTime = noteEvent.timeOffset;
        detectPosition = (float) gameManager.timingData.CalculatePosition(detectTime);
        detectForwardPosition = (float) gameManager.timingData.CalculatePosition(detectTime, true);
    }

    public void SetPopParticle(ParticleSystem particle)
    {
        popParticle = particle;
    }


    void OnEnable()
    {
        clockhand = Clockhand.Instance;

        // 자식 0번째에 있는 배경을 가져온다.
        Transform backgroundTransform = transform.GetChild(0);
        background = backgroundTransform.GetComponent<SpriteRenderer>();
        background.color = Color.clear;

        // 자식 1번째에 있는 전경을 가져온다.
        foregroundTransform = transform.GetChild(1);
        foregroundTransform.localScale = new Vector3(0, 0, 1);
        foreground = foregroundTransform.GetComponent<SpriteRenderer>();
        foreground.color = Color.white;
    }

    void Update()
    {
        double deltaAngle = clockhand.forwardAngle - detectForwardPosition;

        if (deltaAngle < 270)
        {
            float t = (float)((90 - deltaAngle) / 90);
            t = Mathf.Clamp(t, 0, 1);

            float t2 = (float)((180 - deltaAngle) / 180);
            float scale = EasingFunction.EaseInOutSine(0.5f, 2, t);
            float foregroundScale = EasingFunction.EaseInCubic(0.1f, 1, t);
            UpdatePosition(Mathf.Lerp(0f, 10.9f, t2), detectPosition);

            // 배경색
            background.color = Color.Lerp(backgroundStartColor, backgroundColor, t2);

            // 전경색
            // foreground.color = Color.Lerp(Color.clear, Color.white, t);

            // 노트 전체크기
            transform.localScale = new Vector3(scale, scale, 1);

            // 전경 크기
            foregroundTransform.localScale = new Vector3(foregroundScale, foregroundScale, 1);
        } 

        if (deltaAngle < 0 && !isPopped)
        {
            ShowPopEffect();
            background.enabled = false;
            foreground.enabled = false;

            isPopped = true;
        }
    }

    public void ShowPopEffect()
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = transform.position;
        emitParams.applyShapeToPosition = true;

        popParticle.Emit(emitParams, 20);
    }

    // 끝났다면 터지는 파티클 효과를 뿌린다.
    public void Finish()
    {
        if (!isPopped)
        {
            ShowPopEffect();
        }
        Destroy(gameObject);
    }

    // 미스났을 때 처리
    public void FinishMiss()
    {
        Destroy(gameObject);
    }
}
