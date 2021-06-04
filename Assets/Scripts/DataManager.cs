using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

// 세이브파일에 저장될 데이터 형식
[System.Serializable]
public class SaveData
{
    public bool IsCalibrated = false;       // 보정 여부
    public double inputOffset = 0;          // 입력 오프셋
    public Dictionary<int, double> scores;  // 각 레벨별 점수%

    public SaveData()
    {
        scores = new Dictionary<int, double>();
    }
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private string savePath;
    public SaveData saveData;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 저장경로 결정
        savePath = Application.persistentDataPath + "/save.dat";
        Debug.Log(savePath);

        // 세이브파일 로드하기
        ReadFile();
    }

    // 로드
    public void ReadFile()
    {
        // 파일이 존재하지 않으면 빈 객체 만들고 파일로 저장
        if (!File.Exists(savePath))
        {
            saveData = new SaveData();

            WriteFile();
            return;
        }

        // 파일에서 읽어들여서 JSON 파싱 및 반영
        using (StreamReader reader = new StreamReader(savePath))
        {
            string json = reader.ReadToEnd();
            saveData = JsonConvert.DeserializeObject<SaveData>(json);
        }
    }

    // 파일에 저장
    public void WriteFile()
    {
        // JSON으로 직렬화 후 파일로 저장
        using (StreamWriter writer = new StreamWriter(savePath))
        {
            string json = JsonConvert.SerializeObject(saveData);
            writer.Write(json);
        }
    }


}
