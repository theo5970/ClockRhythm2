using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 레벨 선택 버튼
public class LevelSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Text[] texts;                   // 내부에 있는 텍스트들
    private Image background;               // 배경
    private LevelSelectManager manager;     // 레벨 선택 매니저

    private bool isHover = false;           // 버튼 위에 마우스 커서가 올려져있는가?

    public string musicName;                // 음악 이름
    public string composerName;             // 작곡가 이름
    public string levelPath;                // 레벨 경로
    public int levelID;                     // 레벨 ID (세이브파일 용도)

    void Start()
    {
        texts = GetComponentsInChildren<Text>();
        background = transform.Find("Background").GetComponent<Image>();
    }


    void Update()
    {
        // 선택되었는가?
        bool isSelected = (manager.selectedButton == this);

        // 마우스가 올려져있거나 선택되었으면
        if (isHover || isSelected)
        {
            // 선택되었다면 텍스트 색상을 노란색으로 변경
            if (isSelected)
            {
                SetTextColor(Color.yellow);
            }

            // 배경 색을 좀 더 밝게 변경
            background.color = new Color(255, 255, 255, 0.2f);
        }
        else
        {
            // 기본 색으로 변경
            SetTextColor(Color.white);
            background.color = new Color(255, 255, 255, 0.1f);
        }
    }

    // 레벨 선택 매니저 설정
    public void SetManager(LevelSelectManager manager)
    {
        this.manager = manager;
    }

    // 초기화
    public void Init()
    {
        // 첫 번째 자식 가져오기
        Transform content = transform.GetChild(0);

        Text musicNameText = content.Find("MusicName").GetComponent<Text>();
        Text composerNameText = content.Find("ComposerName").GetComponent<Text>();
        Text bestRecordText = content.Find("BestRecord").GetComponent<Text>();

        // 아티스트, 곡 이름 표시
        musicNameText.text = musicName;
        composerNameText.text = "Composer: " + composerName;

        // 최고기록 가져와서 소수점 둘째 자리까지 표시
        double bestRecord = manager.GetBestRecord(this);
        bestRecordText.text = $"{bestRecord:F2}%";
    }

    // 마우스 커서가 내부에 들어왔다면
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    // 마우스 커서가 내부에서 탈출했다면
    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }

    // 마우스를 올려진 상태에서 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        manager.Select(this);
    }

    // 내부에 있는 모든 텍스트의 색상 설정
    private void SetTextColor(Color color)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].color = color;
        }
    }

}
