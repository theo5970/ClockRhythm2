using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text comboText;                              // 콤보 텍스트
    public Text accuracyText;                           // 정확도 텍스트

    public int currentCombo { get; private set; }       // 현재 콤보 수
    public int totalCombo { get; private set; }         // 전체 콤보 수
    private int maxCombo = 0;                           // 플레이 중 획득한 콤보의 최댓값

    private GameManager gameManager;
    private DataManager dataManager;
    private NoteJudgement judgement;

    private double resultAccuracy = 0;                  // 정확도 결과 (최대 80)
    private double sumOfAccuracy = 0;                   // 정확도 가중치의 합
    private double comboScore = 0;                      // 콤보 점수 (최대 20)

    void Start()
    {
        judgement = NoteJudgement.Instance;
        judgement.OnNoteProcess += Judgement_OnNoteProcess;

        gameManager = GameManager.Instance;
        dataManager = DataManager.Instance;

        // 레벨이 다 끝나면 점수를 세이브파일에 저장한다.
        gameManager.OnGameEnd += () =>
        {
            SaveScore();
        };

        var timingData = gameManager.timingData;
        totalCombo = timingData.CountEvent<NoteEvent>() + timingData.CountEvent<HoldNoteEvent>();
    }

    // 점수를 세이브파일에 저장
    public void SaveScore()
    {
        double score = CalculateScore();

        var saveData = dataManager.saveData;
        int id = GameManager.loadSettings.levelID;

        // 딕셔너리에 레벨ID 키가 존재하면
        if (saveData.scores.ContainsKey(id))
        {
            // 최고 기록을 넘어섰을 경우에만 저장한다.
            if (score > saveData.scores[id])
            {
                saveData.scores[id] = score;
            }
        }
        else
        {
            // 기록 정보가 없으면 딕셔너리에 새로 추가
            saveData.scores.Add(id, score);
        }

        // 세이브파일 저장
        dataManager.WriteFile();
    }

    // 점수 계산
    private double CalculateScore()
    {
        return resultAccuracy + comboScore;
    }

    /// <summary>
    /// 시간에 따라 정확도 단위를 계산합니다.
    /// </summary>
    /// <param name="value">기준 시간</param>
    /// <param name="rangeStart">최소 간격</param>
    /// <param name="rangeEnd">최대 간격</param>
    /// <param name="accuracyMin">최소 정확값</param>
    /// <param name="accuracyMax">최대 정확값</param>
    /// <param name="weight">가중치 결과</param>
    private void CalculateAccuracy(double value, double rangeStart, double rangeEnd, double accuracyMin, double accuracyMax, out double weight)
    {
        // ex
        // rangeStart: 0.05
        // rangeEnd : 0.15
        // value: 0.13

        double t = 1.0 - ((value - rangeStart) / (rangeEnd - rangeStart));
        weight = accuracyMin + t * (accuracyMax - accuracyMin);
    }

    // 노트가 판정되었을 때
    private void Judgement_OnNoteProcess(JudgementInfo info)
    {
        // 미스가 나면 콤보를 초기화시킨다.
        if (info.type == JudgementType.Miss)
        {
            currentCombo = 0;
        }
        else if (info.type != JudgementType.None)
        {
            currentCombo++;
            maxCombo = Mathf.Max(currentCombo, maxCombo);
        }

        double weight = 0;    // 정확도 가중치
        switch (info.type)
        {
            case JudgementType.TruePerfect:     // 트루 퍼펙트
                weight = 1;
                break;
            case JudgementType.Perfect:         // 퍼펙트
                CalculateAccuracy(info.absDiffTime, JudgementTiming.TruePerfect, JudgementTiming.Perfect, 0.9, 1, out weight);
                break;
            case JudgementType.Good:            // 굿
                CalculateAccuracy(info.absDiffTime, JudgementTiming.Perfect, JudgementTiming.Good, 0.75, 0.9, out weight);
                break;
            case JudgementType.Bad:             // 배드
                CalculateAccuracy(info.absDiffTime, JudgementTiming.Good, JudgementTiming.Bad, 0.5, 0.75, out weight);
                break;
            case JudgementType.Miss:            // 미스
                weight = 0;
                break;
        }

        sumOfAccuracy += weight;
        
        // 가중치 합계를 전체 콤보수로 나누고 80을 곱해서 점수에 반영
        resultAccuracy = 80 * (sumOfAccuracy / totalCombo);

        // 콤보 점수 계산
        comboScore = 20 * (1d * maxCombo / totalCombo);

        UpdateUI();
    }

    // UI 업데이트
    private void UpdateUI()
    {
        comboText.text = currentCombo + "x";
        accuracyText.text = string.Format("{0:F2}%", CalculateScore());
    }
}
