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
    private static DataSaveAndLoad m_instance; // �̱����� �Ҵ�� ����
    private StringBuilder SavePath = new StringBuilder();
    // �̱��� ���ٿ� ������Ƽ
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
        GameManager.Instance.AddNewLog("����Ϸ�!");
        //�÷��̾��� ��������ġ����
        SaveMapNameAndPlayerTransformDataData SaveData = new SaveMapNameAndPlayerTransformDataData();
        Scene CurScene = SceneManager.GetActiveScene();
        SaveData.SetData(CurScene.name, GameManager.Instance.GetPlayerTransform.transform);
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/"), json);
    }

    public bool LoadGame()
    {
        GameManager.Instance.AddNewLog("�ҷ�����!");
        string FilePath = MakeFilePath("PlayerLocationData", "/SaveData/LastMapAndTransformData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("���̺������� ã�����߽��ϴ�.");
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
