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
    [SerializeField] private PetController petController;

    [Header("UI Buttons")]
    [SerializeField] private Button eatButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button restButton;
    [SerializeField] private Button studyButton;
    [SerializeField] private Button exerciseButton;

    [SerializeField] private TMP_Text currentDayText;
    [SerializeField] private TMP_Text dailyActivityCountText;

    [SerializeField] private TMP_Text statsText;

    private DayNightCycle dayNightCycle;

    public Day today = Day.Monday;
    public int week = 1;
    public int dailyActivityCount = 0;
    private int maxDailyActivities = 5;
    private bool isDayEnded = false;
    private bool isPetActive = false;
    private List<string> dailyActivities;

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
        dayNightCycle = GetComponent<DayNightCycle>();

        // Initialize daily activities for the current day
        dailyActivities = new List<string>();

        // Load saved game data and restore day/week
        GameData gameData = SaveManager.Instance.GetGameData();
        if (gameData != null && gameData.dayHistory.Count > 0)
        {
            DayData lastDay = gameData.dayHistory[gameData.dayHistory.Count - 1];
            
            // Parse the day string back to enum
            if (System.Enum.TryParse(lastDay.day, out Day savedDay))
            {
                today = savedDay;
            }
            week = lastDay.week;
            Debug.Log($"Loaded game state: Week {week} - {today}");
        }
        else
        {
            Debug.Log("No saved data found. Starting fresh.");
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
        // Update new day.
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

    private void OnActivityClicked(Activity activity)
    {
        if (pettingManager != null && pettingManager.isPetInCoolDown())
        {
            Debug.Log("Pet is in cooldown. Please wait until the next day.");
            return;
        }
        else if (isPetActive)
        {
            Debug.Log("Pet is moving. Please wait until it reaches its destination.");
            return;
        }

        Debug.Log($"Activity clicked: {activity}");

        isPetActive = true;

        switch (activity)
        {
            case Activity.Eat: dailyActivities.Add("eat"); petController.Eat(ActivityCompleted); break;
            case Activity.Play: dailyActivities.Add("play"); petController.Play(ActivityCompleted); break;
            case Activity.Rest: dailyActivities.Add("rest"); petController.MoveToRest(ActivityCompleted); break;
            case Activity.Study: dailyActivities.Add("study"); petController.MoveToStudy(ActivityCompleted); break;
            case Activity.Exercise: dailyActivities.Add("exercise"); petController.Exercise(ActivityCompleted); break;
        }
    }

    private void ActivityCompleted()
    {
        Debug.Log("Activity completed by pet.");
        dailyActivityCount++;
        UpdateUITexts();



        if (dailyActivityCount >= maxDailyActivities)
        {
            Debug.Log("Max daily activities reached. Moving to next day.");
            DayEnds();
        }
        else
        {
            isPetActive = false;
        }
    }

    public void DayEnds()
    {
        isDayEnded = true;

        dayNightCycle.SetNightCycle();

        if (petController != null)
        {
            petController.MoveToSleep(() => OnPetReachedBed());
        }
        else
        {
            OnPetReachedBed();
        }
    }


    // New method called when AI response completes
    private void OnDayCompleted()
    {
        Debug.Log("AI response completed. Updating to next day.");
        dayNightCycle.SetDayCycle();
        petController.PetWakeUp(() => OnPetWakeUp());

    }

    public void UpdateNewDay()
    {
        dailyActivityCount = 0;
        isDayEnded = false;
        isPetActive = false;

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

    // Pet Actions:

    private void OnPetWakeUp()
    {
        Debug.Log("Pet woke up. Ready for a new day!");
        UpdateNewDay();
    }


    private void OnPetReachedBed()
    {
        Debug.Log("Pet reached bed. Processing day activities...");

        if (pettingManager != null)
        {
            // Pass day, week to PettingPhaseManager - it handles the rest
            pettingManager.OnMultipleActivitiesCompleted(dailyActivities, today.ToString(), week);
        }
        else
        {
            UpdateNewDay();
        }

        dailyActivities.Clear();
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

        if(statsText != null)
        {
            GameData gameData = SaveManager.Instance.GetGameData();
            
            if (gameData != null && gameData.dayHistory.Count > 0)
            {
                int totalEat = 0;
                int totalPlay = 0;
                int totalRest = 0;
                int totalStudy = 0;
                int totalExercise = 0;
                int totalDays = gameData.dayHistory.Count;

                string lifeStyle = week switch
                {
                    1 => "Child", 
                    2 => "Teenager", 
                    _ => "Adult"
                };

                foreach (DayData day in gameData.dayHistory)
                {
                    totalEat += day.eat;
                    totalPlay += day.play;
                    totalRest += day.rest;
                    totalStudy += day.study;
                    totalExercise += day.exercise;
                }

                statsText.text = $"Total Stats (across {totalDays} days):\n" +
                                 $"Eat: {totalEat}\n" +
                                 $"Play: {totalPlay}\n" +
                                 $"Rest: {totalRest}\n" +
                                 $"Study: {totalStudy}\n" +
                                 $"Exercise: {totalExercise}\n" +
                                 $"Life Stage: {lifeStyle}";
            }
            else
            {
                statsText.text = "No stats yet - complete your first day!";
            }
        }
    }

}
