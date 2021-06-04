using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 일시중지 메뉴 관련 처리
public class PauseMenu : MonoBehaviour
{
    private GameManager gameManager;

    public GameObject pauseMenu;    // 일시중지 메뉴
    public Button continueButton;   // 계속 버튼
    public Button restartButton;    // 다시시작 버튼
    public Button exitButton;       // 나가기 버튼

    void Start()
    {
        pauseMenu.SetActive(false);

        gameManager = GameManager.Instance;
        gameManager.OnGamePause += () =>
        {
            pauseMenu.SetActive(true);
        };

        continueButton.onClick.AddListener(() =>
        {
            gameManager.ResumeGame();
            pauseMenu.SetActive(false);
        });

        restartButton.onClick.AddListener(() =>
        {
            gameManager.RestartGame();
            pauseMenu.SetActive(false);
        });

        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MenuScene");
        });


    }

}
