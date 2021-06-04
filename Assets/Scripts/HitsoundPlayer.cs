using System.Collections.Generic;
using UnityEngine;

// 히트사운드 재생
[RequireComponent(typeof(AudioSource))]
public class HitsoundPlayer : MonoBehaviour
{
    public struct Voice
    {
        public float[] samples;
        public int currentPosition;
    }

    [Range(0, 1f)]
    public float volume;

    public AudioClip hitsound;

    private GameManager gameManager;
    private TimeManager timeManager;
    private TimingData timingData;

    private Queue<NoteEvent> eventQueue;
    private List<Voice> voicePool;
    private int outputSampleRate;

    private float[] hitsoundBuffer;

    public void Awake()
    {
        eventQueue = new Queue<NoteEvent>();
        voicePool = new List<Voice>();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        timeManager = TimeManager.Instance;

        timingData = gameManager.timingData;
        outputSampleRate = AudioSettings.outputSampleRate;

        // 히트사운드 버퍼 데이터 가져오기
        hitsoundBuffer = new float[hitsound.channels * hitsound.samples];
        hitsound.GetData(hitsoundBuffer, 0);

        // 게임이 시작하면 내부 초기화를 해준다.
        gameManager.OnGameStart += Init;
    }

    // 초기화
    public void Init()
    {
        voicePool.Clear();
        eventQueue.Clear();

        // 노트 이벤트를 다 가져와서 이벤트 큐에 넣기
        foreach (NoteEvent evt in timingData.GetEvents<NoteEvent>())
        {
            if (evt.timeOffset >= gameManager.startTime)
                eventQueue.Enqueue(evt);
        }

    }

    // 히트사운드의 정확성을 위해 직접 샘플단위로 처리한다.
    private void OnAudioFilterRead(float[] data, int channels)
    {
        // 오디오 스레드를 통해 시간 계산
        double dspTime = AudioSettings.dspTime - timeManager.gameTimeOffset + gameManager.inputOffset;

        if (gameManager.gameState != GameStateType.Playing) return;

        for (int i = 0; i < data.Length; i += channels)
        {
            // 풀에 재생해야 하는 노트 이벤트들을 다 추가한다.
            while (eventQueue.Count != 0)
            {
                var noteEvent = eventQueue.Peek();
                if (dspTime < noteEvent.timeOffset)
                    break;
                else
                {
                    voicePool.Add(new Voice
                    {
                        samples = hitsoundBuffer,
                        currentPosition = 0
                    });
                    
                    // Dequeue
                    eventQueue.Dequeue();
                }
            }

            // 풀에 있는 히트사운드 Voice들의 샘플을 처리한다.
            float outputL = 0, outputR = 0;
            for (int n = 0; n < voicePool.Count; n++)
            {
                Voice voice = voicePool[n];
                outputL += volume * voice.samples[voice.currentPosition];
                outputR += volume * voice.samples[voice.currentPosition + 1];

                voice.currentPosition += 2;

                voicePool[n] = voice;
                if (voice.currentPosition >= voice.samples.Length)
                {
                    voicePool.RemoveAt(n);
                    n--;
                }

            }

            // 필터에 계산된 값을 넣는다.
            data[i] = outputL;
            data[i + 1] = outputR;

            dspTime += 1.0 / outputSampleRate;
        }
    }
}
