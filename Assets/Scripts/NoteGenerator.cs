using UnityEngine;
using System.Collections.Generic;

// 노트 생성기
public class NoteGenerator : MonoBehaviour
{
    public GameObject notePrefab;       // 노트 프리팹
    public Transform noteParent;        // 노트들의 부모 트랜스폼
    public ParticleSystem popParticle;  // 팝 터지는 파티클

    private Clockhand clockhand;        // 시계바늘
    private GameManager gameManager;
    private TimingData timingData;
    private NoteJudgement judgement;

    private List<NoteEvent> normalNoteEvents;
    private Queue<NoteEvent> normalNoteQueue;

    void Start()
    {
        clockhand = Clockhand.Instance;
        judgement = NoteJudgement.Instance;
        gameManager = GameManager.Instance;

        timingData = GameManager.Instance.timingData;

        normalNoteEvents = timingData.GetEventsList<NoteEvent>();
        normalNoteQueue = new Queue<NoteEvent>();

        InitNoteQueue();
    }

    // 노트 큐 초기화
    private void InitNoteQueue()
    {
        int startIndex = Utils.FindIndexOfFirstEvent(normalNoteEvents, gameManager.startTime);

        for (int i = startIndex; i < normalNoteEvents.Count; i++)
        {
            normalNoteQueue.Enqueue(normalNoteEvents[i]);
        }
    }

    // 노트 생성
    private void GenerateNotes()
    {
        if (normalNoteQueue.Count == 0) return;

        NoteEvent noteEvent = normalNoteQueue.Peek();
        float deltaPosition = clockhand.forwardAngle - (float)noteEvent.startNotePosition;

        // 역방향 고려안한 각도 차이가 270도 이하이면 노트를 생성한다.
        if (deltaPosition < 270)
        {
            GameObject noteObj = Instantiate(notePrefab);
            Note note = noteObj.GetComponent<Note>();
            note.Init(noteEvent);
            note.SetPopParticle(popParticle);

            judgement.EnqueueNote(note);

            noteObj.transform.parent = noteParent;
            normalNoteQueue.Dequeue();
        }
    }

    void Update()
    {
        GenerateNotes();
    }
}
