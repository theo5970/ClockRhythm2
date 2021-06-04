using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

// 판정 타입
public enum JudgementType
{
    None, TruePerfect, Perfect, Good, Bad, Miss
}

// 판정 정보 구조체
public struct JudgementInfo
{
    public JudgementType type;      // 판정 타입
    public Vector3 notePosition;    // 노트 위치값
    public double absDiffTime;      // 판정시간 차이의 절댓값
}

// 판정 타이밍 상수 값 (초 단위)
public static class JudgementTiming 
{
    public static double TruePerfect = 0.05;
    public static double Perfect = 0.07;
    public static double Good = 0.1;
    public static double Bad = 0.25;
    public static double TooFastLimit = 0.5;
}

// 노트 판정
public class NoteJudgement : MonoBehaviour
{
    public static NoteJudgement Instance;

    public Text autoplayText;

    // 자동 플레이를 할 것인가?
    private static bool _isAutoPlay = false; 

    public bool isAutoPlay
    {
        get => _isAutoPlay;
        private set
        {
            _isAutoPlay = value;
        }
    }

    // 노트가 처리되었을 때의 이벤트
    public event Action<JudgementInfo> OnNoteProcess;

    private Queue<Note> noteQueue;
    private TimeManager timeManager;
    private GameManager gameManager;

    private Note currentNote;       // 현재 노트
    private double timeDiff;        // 판정시간 차이
    private double absTimeDiff;     // 판정시간 차이의 절댓값

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

        noteQueue = new Queue<Note>();
    }

    // 노트 컴포넌트를 노트 큐에 삽입
    public void EnqueueNote(Note note)
    {
        noteQueue.Enqueue(note);
    }

    void Start()
    {
        timeManager = TimeManager.Instance;
        gameManager = GameManager.Instance;

        autoplayText.enabled = isAutoPlay;
    }

    void Update()
    {
        if (noteQueue.Count == 0 || gameManager.gameState != GameStateType.Playing)
        {
            return;
        }

        currentNote = noteQueue.Peek();

        // 판정 시간차
        timeDiff = currentNote.detectTime - timeManager.gameTime;

        // 판정 시간차의 절댓값
        absTimeDiff = Math.Abs(timeDiff);

        if (isAutoPlay)
        {
            // 오토-플레이
            if (timeDiff < 0) ProcessInput();     
        }
        else
        {
            // 왼쪽 버튼 클릭했을 때
            if (Input.anyKeyDown) ProcessInput();
        }

        // Ctrl+A 조합으로 오토플레이 껏다 키게한다.
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
        {
            isAutoPlay = !isAutoPlay;
            autoplayText.enabled = isAutoPlay;
        }

        // 너무 느림
        if (timeDiff < -JudgementTiming.Bad)
        {
            OnNoteProcess?.Invoke(new JudgementInfo
            {
               type = JudgementType.Miss,
               notePosition = currentNote.transform.position
            });
            currentNote.FinishMiss();

            if (noteQueue.Count > 0)
            {
                noteQueue.Dequeue();
            }
        }
    }

    // 입력 처리
    private void ProcessInput()
    {
        JudgementType judgement = JudgementType.None;

        if (absTimeDiff < JudgementTiming.TruePerfect)
        {
            judgement = JudgementType.TruePerfect;
        }
        else if (absTimeDiff < JudgementTiming.Perfect)
        {
            judgement = JudgementType.Perfect;
        }
        else if (absTimeDiff < JudgementTiming.Good)
        {
            judgement = JudgementType.Good;
        }
        else if (absTimeDiff < JudgementTiming.Bad)
        {
            judgement = JudgementType.Bad;
        }
        else if (timeDiff > JudgementTiming.Bad && timeDiff < JudgementTiming.TooFastLimit)
        {
            judgement = JudgementType.Miss;
        }

        if (judgement != JudgementType.None)
        {
            // 판정 이벤트 호출
            OnNoteProcess?.Invoke(new JudgementInfo
            {
                type = judgement,
                notePosition = currentNote.transform.position,
                absDiffTime = absTimeDiff
            });

            currentNote.Finish();
            noteQueue.Dequeue();
        }
    }
}
