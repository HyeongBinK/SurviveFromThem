using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SaveMapNameAndPlayerTransformDataData
{
    public string LastMapName;
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;

    public void SetData(string MapName, Transform Tr)
    {
        LastMapName = MapName;
        PlayerPosition = Tr.position;
        PlayerRotation = Tr.rotation;
    }
}

public class DataSaveAndLoad 
{
    private static DataSaveAndLoad m_instance; // 싱글톤이 할당될 변수
    private StringBuilder SavePath = new StringBuilder();
    // 싱글톤 접근용 프로퍼티
    public static DataSaveAndLoad Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new DataSaveAndLoad();
                //DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

   public void SaveGame()
    {
        GameManager.Instance.AddNewLog("저장완료!");
        //플레이어의 마지막위치저장
        SaveMapNameAndPlayerTransformDataData SaveData = new SaveMapNameAndPlayerTransformDataData();
        Scene CurScene = SceneManager.GetActiveScene();
        SaveData.SetData(CurScene.name, GameManager.Instance.GetPlayerTransform.transform);
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/"), json);
    }

    public bool LoadGame()
    {
        GameManager.Instance.AddNewLog("불러오기!");
        string FilePath = MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("세이브파일을 찾지못했습니다.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        SaveMapNameAndPlayerTransformDataData SaveData = JsonUtility.FromJson<SaveMapNameAndPlayerTransformDataData>(SaveFile);

        return true;
    }

    public string MakeFilePath(string FileName, string FileFolder)
    {
        SavePath.Append(Application.streamingAssetsPath);
        SavePath.Append(FileFolder);
        SavePath.Append(FileName);
        SavePath.Append(".Json");

        string Result = SavePath.ToString();
        SavePath.Clear();
        return Result;
    }
}
