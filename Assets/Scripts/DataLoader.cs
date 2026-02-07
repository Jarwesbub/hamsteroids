using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    private void Start()
    {
        LoadAndDisplayData();
    }

    public void LoadAndDisplayData()
    {
        GameData data = SaveManager.Instance.GetGameData();

        if (data.dayHistory.Count == 0)
        {
            Debug.Log("No game data found.");
            return;
        }

        Debug.Log($"=== Loaded Game Data ===");
        Debug.Log($"Total days recorded: {data.dayHistory.Count}");
        Debug.Log("");

        foreach (DayData day in data.dayHistory)
        {
            Debug.Log($"Week {day.week} - {day.day} ({day.lifeStage})");
            Debug.Log($"  Eat: {day.eat}, Play: {day.play}, Rest: {day.rest}, Study: {day.study}, Exercise: {day.exercise}");
        }

        Debug.Log("======================");
    }

    public DayData GetLastDayData()
    {
        GameData data = SaveManager.Instance.GetGameData();
        
        if (data.dayHistory.Count > 0)
        {
            return data.dayHistory[data.dayHistory.Count - 1];
        }

        return null;
    }

    public int GetTotalActivityCount(string activityType)
    {
        GameData data = SaveManager.Instance.GetGameData();
        int total = 0;

        foreach (DayData day in data.dayHistory)
        {
            switch (activityType.ToLower())
            {
                case "eat": total += day.eat; break;
                case "play": total += day.play; break;
                case "rest": total += day.rest; break;
                case "study": total += day.study; break;
                case "exercise": total += day.exercise; break;
            }
        }

        return total;
    }

    public List<DayData> GetDaysByWeek(int week)
    {
        GameData data = SaveManager.Instance.GetGameData();
        List<DayData> daysInWeek = new List<DayData>();

        foreach (DayData day in data.dayHistory)
        {
            if (day.week == week)
            {
                daysInWeek.Add(day);
            }
        }

        return daysInWeek;
    }

    public Dictionary<string, int> GetTotalActivities()
    {
        GameData data = SaveManager.Instance.GetGameData();
        Dictionary<string, int> totals = new Dictionary<string, int>
        {
            { "eat", 0 },
            { "play", 0 },
            { "rest", 0 },
            { "study", 0 },
            { "exercise", 0 }
        };

        foreach (DayData day in data.dayHistory)
        {
            totals["eat"] += day.eat;
            totals["play"] += day.play;
            totals["rest"] += day.rest;
            totals["study"] += day.study;
            totals["exercise"] += day.exercise;
        }

        return totals;
    }
}