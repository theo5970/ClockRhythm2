using UnityEngine;
using UnityEngine.UI;

// 레벨 배경화면
public class BackgroundImage : MonoBehaviour
{
    private GameManager gameManager;
    private Image background;

    void Start()
    {
        background = GetComponent<Image>();

        gameManager = GameManager.Instance;
        var levelSettings = gameManager.currentLevel.settings;

        // 레벨 설정에 있는 배경화면 이름을 기반으로 리소스 폴더에서 로딩
        Sprite backgroundImage = Resources.Load<Sprite>("Backgrounds/" + levelSettings.backgroundName);

        if (backgroundImage != null)
        {
            // 배경이 있다면 불러온 걸 기반으로 표시
            background.enabled = true;
            background.sprite = backgroundImage;
        }
        else
        {
            // 배경이 없다면 그냥 표시하지 않는다.
            background.enabled = false;
        }
    }
}
