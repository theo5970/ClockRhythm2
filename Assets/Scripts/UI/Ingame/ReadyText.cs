using UnityEngine;
using UnityEngine.UI;

// 준비 텍스트 (일시중지 해제 시)
[RequireComponent(typeof(Text))]
public class ReadyText : MonoBehaviour
{
    private GameManager gameManager;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.enabled = false;

        gameManager = GameManager.Instance;
        gameManager.OnGameReady += () =>
        {
            text.enabled = true;
        };
        gameManager.OnGameResume += () =>
        {
            text.enabled = false;
        };
    }

}
