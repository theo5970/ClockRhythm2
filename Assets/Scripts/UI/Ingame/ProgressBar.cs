using UnityEngine;
using UnityEngine.UI;

// 화면 위에 표시되는 진행바
public class ProgressBar : MonoBehaviour
{
    private GameManager gameManager;
    private Image barImage;

    void Start()
    {
        gameManager = GameManager.Instance;
        barImage = GetComponent<Image>();
    }

    void Update()
    {
        barImage.fillAmount = gameManager.musicProgress;
    }
}
