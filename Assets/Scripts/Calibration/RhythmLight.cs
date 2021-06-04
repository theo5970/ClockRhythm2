using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RhythmLight : MonoBehaviour
{
    const float flashDuration = 0.25f;      // 깜빡임 지속시간
    const float maxScale = 1.3f;            // 커지는 최대 크기

    private Image fillImage;                // 안쪽 채워진 이미지

    void Start()
    {
        // 안쪽 채워진 이미지: 두 번째 자식
        fillImage = transform.GetChild(1).GetComponent<Image>();
    }

    // 깜빡이는 코루틴 시작
    public void Tick()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color startColor = Color.white;     // 시작 색상 (흰색)
        Color endColor = startColor;        // 끝 색상 (흰색 투명)
        endColor.a = 0;

        // 페이드 기능 수행
        float t = 0;
        while (t < 1)
        {
            // 안쪽 채워진 이미지 색상 보간
            fillImage.color = Color.Lerp(startColor, endColor, EasingFunction.EaseInSine(0, 1, t));

            // 크기 보간
            float scale = Mathf.Lerp(maxScale, 1, EasingFunction.EaseOutSine(0, 1, t));
            transform.localScale = new Vector3(scale, scale, 1);

            // 시간 변화량 더하기
            t += Time.deltaTime / flashDuration;
            yield return null;
        }

        // 정확하게 적용하기 위해 한 번 더 적용
        fillImage.color = endColor;
        transform.localScale = Vector3.one;
    }
}
