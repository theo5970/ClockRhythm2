using UnityEngine;

// TimingData가 제대로 작동하는지 확인하는 테스트
public class TimingDataTest : MonoBehaviour
{
    [Range(0, 4f)]
    public float testTime;

    public TextAsset testLevel;

    private TimingData timingData;

    // Use this for initialization
    void Start()
    {
        LevelData levelData = LevelLoader.Load(testLevel.text);
        timingData = levelData.timingData;

        foreach (NoteEvent noteEvent in timingData.GetEvents<NoteEvent>())
        {
            Debug.Log($"Note: {noteEvent}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.right * (float)timingData.CalculatePosition(testTime);
    }
}
