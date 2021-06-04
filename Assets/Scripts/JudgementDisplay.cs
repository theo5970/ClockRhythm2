using UnityEngine;
using System.Collections;

// 판정 이미지
public class JudgementDisplay : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private TimeManager timeManager;

    // 활성화되면 코루틴 시작
    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeRoutine());
    }

    // 스프라이트 설정
    public void SetSprite(Sprite sprite)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    // 이미지가 작아지면서 점차 사라지게한다.
    IEnumerator FadeRoutine()
    {
        timeManager = TimeManager.Instance;
        float startTime = (float) timeManager.gameTime;

        float t = 0;
        while (t < 1)
        {
            // 스프라이트 색상 보간
            spriteRenderer.color = Color.Lerp(Color.white, Color.clear, EasingFunction.Linear(0, 1, t));

            // 크기 보간
            float scaleX = EasingFunction.EaseOutElastic(1.25f, 0.75f, t);
            float scaleY = EasingFunction.EaseOutElastic(1.0f, 0.75f, t);
            transform.localScale = new Vector3(scaleX, scaleY, 1);

            t = ((float)timeManager.gameTime - startTime) / 1.5f;
            yield return null;
        }

        // 자기 자신 오브젝트를 없앤다.
        Destroy(gameObject);
    }
}
