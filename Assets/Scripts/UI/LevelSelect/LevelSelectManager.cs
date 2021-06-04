using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LevelSelectManager : MonoBehaviour
{
    private DataManager dataManager;

    public Transform itemRoot;              // 레벨선택 버튼들의 부모 트랜스폼
    public Button startButton;              // 시작 버튼
    public Button calibrateButton;          // 보정 버튼
    public Button resetCalibrateButton;     // 보정 초기화 버튼
    public Button exitButton;               // 종료 버튼

    private LevelSelectButton[] buttons;
    public LevelSelectButton selectedButton
    {
        get;
        private set;
    }

    // Use this for initialization
    void Start()
    {
        dataManager = DataManager.Instance;

        // 수직 동기화 해제 & 최대 FPS 설정
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;

        buttons = itemRoot.GetComponentsInChildren<LevelSelectButton>();

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            button.SetManager(this);
            button.Init();
        }

        // 시작 버튼 비활성화
        startButton.interactable = false;

        // 시작 버튼이 클릭되었을 때
        startButton.onClick.AddListener(() =>
        {
            if (selectedButton == null) return;

            // 레벨 로딩설정 초기화
            GameManager.loadSettings = new LoadSettings
            {
                isInternal = true,
                levelID = selectedButton.levelID,
                levelPath = selectedButton.levelPath
            };

            if (dataManager.saveData.IsCalibrated)
            {
                // 보정이 되었다면 바로 게임 씬으로 이동
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                // 보정이 안되었을경우 보정 씬으로 먼저 이동
                Calibrator.nextSceneName = "GameScene";
                SceneManager.LoadScene("CalibrationScene");
            }
        });

        // 보정 버튼을 클릭했을 때 보정 씬으로 이동
        calibrateButton.onClick.AddListener(() =>
        {
            Calibrator.nextSceneName = "MenuScene";
            SceneManager.LoadScene("CalibrationScene");
        });

        // 보정 초기화 버튼
        resetCalibrateButton.onClick.AddListener(() =>
        {
            var saveData = dataManager.saveData;
            saveData.IsCalibrated = false;
            saveData.inputOffset = 0;
        });

        // 종료 버튼을 클릭했을 때 게임 종료
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    // 레벨 선택버튼을 사용자가 클릭했을 때
    public void Select(LevelSelectButton button)
    {
        startButton.interactable = true;
        selectedButton = button;
    }

    // 데이터 매니저에서 해당 레벨에 대한 최고기록% 가져오기
    public double GetBestRecord(LevelSelectButton button)
    {
        // 세이브데이터에 존재하면 그 기록을 아니면 0을 리턴한다.
        var saveData = dataManager.saveData;
        if (saveData.scores.ContainsKey(button.levelID))
        {
            return saveData.scores[button.levelID];
        }
        else
        {
            return 0;
        }
    }
}
