using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 보정 기능
public class Calibrator : MonoBehaviour
{
    // 다음에 넘어갈 씬 이름
    public static string nextSceneName;

    const float bpm = 120.0f;               // 분당 박자 수 (Beats Per Minute)
    private float crotchet => (60f / bpm);  // 1박자 (4분음표)의 시간

    private DataManager dataManager;
    private TimeManager timeManager;
    private List<double> offsetList;
    private bool isMusicEnd = false;
    private double lastTime = 0;
    private int lightIndex = 0;

    private RhythmLight[] lights;

    // 인스펙터
    public AudioSource music;
    public double musicOffset = 0;
    public Text offsetText;
    public Text resultText;
    public Transform lightsRoot;

    void Awake()
    {
        offsetList = new List<double>();
    }

    IEnumerator Start()
    {
        lightIndex = 0;
        timeManager = GetComponent<TimeManager>();
        dataManager = DataManager.Instance;
        resultText.gameObject.SetActive(false);

        lights = lightsRoot.GetComponentsInChildren<RhythmLight>();

        // 1초 뒤 음악 & 게임시간 측정 시작
        music.PlayScheduled(AudioSettings.dspTime + 1.0);
        timeManager.StartByNow(musicOffset - 1);

        // 다음 코루틴 실행
        yield return WaitForEndMusic();
    }

    IEnumerator WaitForEndMusic()
    {
        // 음악이 다 끝날 때까지 대기
        yield return new WaitUntil(() => !music.isPlaying);
        isMusicEnd = true;

        SolveResult();

        // 1초 대기후 결과 텍스트 표시
        yield return new WaitForSecondsRealtime(1.0f);
        resultText.gameObject.SetActive(true);

        // 아무 키나 누르면 다음 씬으로 이동
        yield return new WaitUntil(() => Input.anyKeyDown);
        SceneManager.LoadScene(nextSceneName);
    }

    // 결과 계산하기 (평균 & 분산)
    private void SolveResult()
    {
        if (offsetList.Count < 30)
        {
            resultText.text = $"입력 {offsetList.Count}회는 충분하지 않습니다. 다시 시도해주세요.";
            return;
        }

        // 오프셋 평균 구하기
        double average = 0;
        foreach (double offset in offsetList)
        {
            average += offset;
        }
        average /= offsetList.Count;

        // 편차를 이용해서 분산 구하기
        double disperation = 0; // 분산
        foreach (double offset in offsetList)
        {
            double deviation = offset - average;
            disperation += deviation * deviation;
        }

        disperation /= offsetList.Count;

        // 표준편차 (Standard Deviation) 구하기
        double sd = Math.Sqrt(disperation);

        // 결과를 텍스트에 출력
        offsetText.text = string.Format("평균 오프셋: {0:F0}ms (편차: {1:F0}ms)", average * 1000, sd * 1000);

        double result = average;

        // 시간 편차에 따라 메시지 출력하기
        if (sd <= 0.01)
        {
            resultText.text = "사람 맞으신가요? 너무 정확하지 말입니다;;";
        }
        else if (sd <= 0.02)
        {
            resultText.text = "박자 감각이 정말 좋으시네요!";
        }
        else if (sd <= 0.03)
        {
            resultText.text = "리듬게임 예전에 좀 해보셨나 보군요.";
        }
        else if (sd <= 0.05)
        {
            resultText.text = "입력이 좀 어긋나네요.";
        }
        else if (sd <= 0.1)
        {
            resultText.text = "입력이 좀 많이 어긋나군요 ㅠ";
        }
        else
        {
            resultText.text = "혹시 막 누르셨나요..? 어쩔 수 없이 기본 보정으로 시작할게요.";

            // 입력이 너무 어긋나면 기본 보정으로 결정
            result = 0.15;
        }

        resultText.text += "\n\n계속하려면 아무 키나 누르세요.";

        // 입력 오프셋을 세이브 파일에 저장하기
        var saveData = dataManager.saveData;
        saveData.IsCalibrated = true;
        saveData.inputOffset = result;

        dataManager.WriteFile();
    }

    void Update()
    {
        // 음악이 끝나기 전까지 보정기능 계속 실행
        if (!isMusicEnd)
        {
            UpdateCalibration();
        }
    }

    void UpdateCalibration()
    {
        // 다음 박자 차례라면
        if (timeManager.gameTime > lastTime + crotchet)
        {
            // 이전 시간에 한 박자 시간을 더한다.
            lastTime += crotchet;

            // 리듬 조명 순서대로 깜빡거리게 하기
            lightIndex++;
            if (lightIndex >= lights.Length)
            {
                lightIndex = 0;
            }

            lights[lightIndex].Tick();
        }

        if (Input.anyKeyDown)   // 아무 키나 눌러졌다면
        {
            double diffTime = timeManager.gameTime - lastTime;
            if (diffTime > 0.5 * crotchet) // 시간차가 0.5박자 이상이면 1박자 시간 빼기
            {
                diffTime -= crotchet;
            }

            // 오프셋 출력하고 리스트에 추가
            offsetText.text = $"오프셋: {(diffTime * 1000):F0}ms";
            offsetList.Add(diffTime);
        }
    }
}