using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class DayData
{
    public string day;
    public int week;
    public string lifeStage;
    public int eat;
    public int play;
    public int rest;
    public int study;
    public int exercise;

    public DayData(string currentDay, int currentWeek, string currentLifeStage, List<string> dailyActivities)
    {
        day = currentDay;
        week = currentWeek;
        lifeStage = currentLifeStage;
        
        // Count occurrences of each activity
        eat = 0;
        play = 0;
        rest = 0;
        study = 0;
        exercise = 0;

        foreach (string activity in dailyActivities)
        {
            switch (activity.ToLower())
            {
                case "eat": eat++; break;
                case "play": play++; break;
                case "rest": rest++; break;
                case "study": study++; break;
                case "exercise": exercise++; break;
            }
        }
    }
}

[System.Serializable]
public class GameData
{
    public List<DayData> dayHistory = new List<DayData>();
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SaveManager");
                instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private GameData gameData;
    private string saveFilePath;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        LoadGameData();
    }

    public void SaveDayData(string day, int week, string lifeStage, List<string> activities)
    {
        DayData dayData = new DayData(day, week, lifeStage, activities);
        gameData.dayHistory.Add(dayData);

        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"Game data saved to: {saveFilePath}");
        Debug.Log($"Total days saved: {gameData.dayHistory.Count}");
    }

    public void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            gameData = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"Loaded {gameData.dayHistory.Count} days from save file");
        }
        else
        {
            Debug.Log("No save file found. Starting fresh.");
            gameData = new GameData();
        }
    }

    public GameData GetGameData()
    {
        return gameData;
    }

    public void ClearSaveData()
    {
        gameData = new GameData();
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save data cleared.");
        }
    }
}