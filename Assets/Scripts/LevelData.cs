// 레벨 정보
public struct LevelSettings
{
    public string musicName;        // 음악 리소스 파일명
    public string artistName;       // 아티스트 명 (작곡가)
    public string songName;         // 음악 제목
    public string backgroundName;   // 배경 이미지 이름
    public float bpm;               // bpm (1초당 박자 수 : Beats Per Minute)
    public float pitch;             // 배속
    public float offset;            // 시작 시간
    public float anglePerBeat;      // 한 박자에 몇 도 회전할 것인가?
}

// 레벨 데이터
public class LevelData
{
    public LevelSettings settings;
    public TimingData timingData;

    public LevelData()
    {
        settings = new LevelSettings();
        timingData = new TimingData();
    }
}


