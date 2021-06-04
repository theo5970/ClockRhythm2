using UnityEngine;
using UnityEngine.UI;

// 시작 텍스트 (맨 처음에)
[RequireComponent(typeof(Text))]
public class StartText : MonoBehaviour
{
    private GameManager gameManager;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.enabled = true;

        gameManager = GameManager.Instance;
        gameManager.OnGameStart += () =>
        {
            text.enabled = false;
        };
    }

}
