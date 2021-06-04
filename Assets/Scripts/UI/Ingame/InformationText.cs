using UnityEngine;
using UnityEngine.UI;

// 아티스트, 곡 이름 표시
public class InformationText : MonoBehaviour
{
    public Text artistText;
    public Text musicNameText;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        var levelSettings = gameManager.currentLevel.settings;
        artistText.text = levelSettings.artistName;
        musicNameText.text = levelSettings.songName;
    }

}
