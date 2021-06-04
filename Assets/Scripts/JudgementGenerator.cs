using UnityEngine;

// 판정 이미지 생성기
public class JudgementGenerator : MonoBehaviour
{
    private NoteJudgement judgement;

    [Header("Prefab")]
    public GameObject displayPrefab;    // 판정 이미지 오브젝트의 프리팹

    // 판정 스프라이트들
    [Header("Judgement Images")]
    public Sprite truePerfect;
    public Sprite perfect;
    public Sprite good;
    public Sprite bad;
    public Sprite miss;

    void Start()
    {
        judgement = NoteJudgement.Instance;

        // 노트판정 이벤트 등록
        judgement.OnNoteProcess += Judgement_OnNoteProcess;
    }

    // 노트가 판정되었을 때 판정 이미지 오브젝트를 그 노트의 위쪽에 생성한다.
    private void Judgement_OnNoteProcess(JudgementInfo info)
    {
        Camera cam = Camera.main;

        GameObject judgeDisplayObj = Instantiate(displayPrefab);
        judgeDisplayObj.transform.position = info.notePosition + (2 * cam.transform.up);
        judgeDisplayObj.transform.rotation = cam.transform.rotation;

        var display = judgeDisplayObj.GetComponent<JudgementDisplay>();

        switch (info.type)
        {
            case JudgementType.TruePerfect:
                display.SetSprite(truePerfect);
                break;
            case JudgementType.Perfect:
                display.SetSprite(perfect);
                break;
            case JudgementType.Good:
                display.SetSprite(good);
                break;
            case JudgementType.Bad:
                display.SetSprite(bad);
                break;
            case JudgementType.Miss:
                display.SetSprite(miss);
                break;
        }
    }
}
