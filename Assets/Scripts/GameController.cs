using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Neocortex.Samples;
using System.Collections.Generic;

public enum Activity
{
    Eat,
    Play,
    Rest,
    Study,
    Exercise
}

public enum Day
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public class GameController : MonoBehaviour
{
    [Header("Pet")]
    [SerializeField] private PettingPhaseManager pettingManager;

    [Header("UI Buttons")]
    [SerializeField] private Button eatButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button restButton;
    [SerializeField] private Button studyButton;
    [SerializeField] private Button exerciseButton;

    [SerializeField] private TMP_Text currentDayText;
    [SerializeField] private TMP_Text dailyActivityCountText;

    public Day today = Day.Monday;
    public int week = 1;
    public int dailyActivityCount = 0;
    private int maxDailyActivities = 5;
    private bool isDayEnded = false;
    private List<string> dailyActivities = new List<string>();

    private void Awake()
    {
        // Wire buttons
        eatButton.onClick.AddListener(() => OnActivityClicked(Activity.Eat));
        playButton.onClick.AddListener(() => OnActivityClicked(Activity.Play));
        restButton.onClick.AddListener(() => OnActivityClicked(Activity.Rest));
        studyButton.onClick.AddListener(() => OnActivityClicked(Activity.Study));
        exerciseButton.onClick.AddListener(() => OnActivityClicked(Activity.Exercise));
        
        // Subscribe to day completed event
        if (pettingManager != null)
        {
            pettingManager.OnDayCompleted.AddListener(OnDayCompleted);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        if (pettingManager != null)
        {
            pettingManager.OnDayCompleted.RemoveListener(OnDayCompleted);
        }
    }

    private void Start()
    {
        Debug.Log($"Today is {today}");
        Debug.Log($"Save file location: {Application.persistentDataPath}");
        UpdateUITexts();
    }

    private void OnActivityClicked(Activity activity)
    {
        if(pettingManager != null && pettingManager.isPetInCoolDown())
        {
            Debug.Log("Pet is in cooldown. Please wait until the next day.");
            return;
        }

        Debug.Log($"Activity clicked: {activity}");

        switch(activity)
        {
            case Activity.Eat: dailyActivities.Add("eat"); break;
            case Activity.Play: dailyActivities.Add("play"); break;
            case Activity.Rest: dailyActivities.Add("rest"); break;
            case Activity.Study: dailyActivities.Add("study"); break;
            case Activity.Exercise: dailyActivities.Add("exercise"); break;
        }

        dailyActivityCount++;
        UpdateUITexts();

        if (dailyActivityCount >= maxDailyActivities)
        {
            Debug.Log("Max daily activities reached. Moving to next day.");
            DayEnds();
        }
    }

    public void DayEnds()
    {
        isDayEnded = true;

        if(pettingManager != null)
        {
            // Pass day, week to PettingPhaseManager - it handles the rest
            pettingManager.OnMultipleActivitiesCompleted(dailyActivities, today.ToString(), week);
        }
        
        dailyActivities.Clear();
    }

    // New method called when AI response completes
    private void OnDayCompleted()
    {
        Debug.Log("AI response completed. Updating to next day.");
        UpdateDay();
    }

    public void UpdateDay()
    {
        dailyActivityCount = 0;
        isDayEnded = false;

        if (today == Day.Sunday)
        {
            today = Day.Monday;
            week++;
        }
        else
        {
            today++;
        }

        UpdateUITexts();
    }

    private void UpdateUITexts()
    {
        if (dailyActivityCountText != null)
        {
            dailyActivityCountText.text = $"Activities today: {dailyActivityCount}/{maxDailyActivities}";
        }

        if (currentDayText != null)
        {
            currentDayText.text = $"Week {week} - {today}";
        }
    }
}
