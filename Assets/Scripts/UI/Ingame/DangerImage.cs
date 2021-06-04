using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// 미스 발생했을 때 경고(?)를 주기 위한 분위기 이미지
public class DangerImage : MonoBehaviour
{
    private NoteJudgement judgement;
    private Image image;

    void Start()
    {
        judgement = NoteJudgement.Instance;
        judgement.OnNoteProcess += Judgement_OnNoteProcess;

        image = GetComponent<Image>();
        image.color = Color.clear;  // 색상을 투명하게 해서 일단 안보이게 한다.
    }

    private void Judgement_OnNoteProcess(JudgementInfo info)
    {
        // 만약 판정이 Miss라면 깜빡이게 페이드한다.
        if (info.type == JudgementType.Miss)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine());
        }
    }

    // 페이드 코루틴 (0.2초동안 흰색 => 흰색 투명으로 페이드 아웃)
    IEnumerator FadeRoutine()
    {
        Color startColor = Color.white;
        Color endColor = startColor;
        endColor.a = 0;

        float t = 0;
        while (t < 1)
        {
            image.color = Color.Lerp(startColor, endColor, t);

            t += Time.deltaTime / 0.2f;
            yield return null;
        }
    }
}
