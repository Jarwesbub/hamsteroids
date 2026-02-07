using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Neocortex.Data;

namespace Neocortex.Samples
{
    public enum LifeStage
    {
        Child,
        Teenager,
        Adult
    }

    public class PettingPhaseManager : MonoBehaviour
    {
        [Header("Neocortex Components")]
        [SerializeField] private NeocortexSmartAgent smartAgent;
        [SerializeField] private NeocortexThinkingIndicator thinkingIndicator;
        [SerializeField] private NeocortexChatPanel chatPanel;

        [Header("Pet Mood")]
        [SerializeField] private PetMoodIcon petMoodIcon;

        [Header("Settings")]
        [SerializeField] private float aiResponseTimeout = 15f;

        public bool useSmartAgent = true;
        private bool isCoolDown = false;
        private Coroutine timeoutCoroutine;

        public UnityEvent OnDayCompleted = new UnityEvent();

        private Dictionary<string, int> petTraits = new Dictionary<string, int>()
        {
            {"eat", 0},
            {"play", 0},
            {"rest", 0},
            {"study", 0},
            {"exercise", 0}
        };

        private void Awake()
        {
            if (smartAgent != null)
            {
                smartAgent.OnChatResponseReceived.AddListener(OnResponseReceived);
                smartAgent.OnRequestFailed.AddListener(OnRequestFailed);
            }
            else
            {
                Debug.LogError("NeocortexSmartAgent not assigned in PettingPhaseManager!");
            }
        }

        private void OnDestroy()
        {
            if (smartAgent != null)
            {
                smartAgent.OnChatResponseReceived.RemoveListener(OnResponseReceived);
                smartAgent.OnRequestFailed.RemoveListener(OnRequestFailed);
            }
        }

        public bool isPetInCoolDown()
        {
            return isCoolDown;
        }

        public void OnMultipleActivitiesCompleted(List<string> activities, string day, int week)
        {
            isCoolDown = true;

            LifeStage lifeStage = GetLifeStageFromWeek(week);

            foreach (string activity in activities)
            {
                if (petTraits.ContainsKey(activity))
                {
                    petTraits[activity]++;
                }
            }

            SaveManager.Instance.SaveDayData(day, week, lifeStage.ToString(), activities);

            if (useSmartAgent && smartAgent != null)
            {
                string activitiesList = string.Join(", ", activities);
                string prompt = $"My pet is a {lifeStage} (life stage). Today I gave it these activities: {activitiesList}. Current traits: {TraitSummary()}. How does it feel about the day?";
                
                Debug.Log($"Sending AI request: {prompt}");
                smartAgent.TextToText(prompt);

                if (thinkingIndicator != null)
                {
                    thinkingIndicator.Display(true);
                }

                if (chatPanel != null)
                {
                    chatPanel.AddMessage($"[{lifeStage}] Day completed with activities: {activitiesList}", true);
                }

                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
                timeoutCoroutine = StartCoroutine(AIResponseTimeout());
            }
            else
            {
                Debug.Log("Smart agent disabled or not assigned. Skipping AI response.");
                StartCoroutine(CoolDownWaitTimer());
            }
        }

        private LifeStage GetLifeStageFromWeek(int week)
        {
            if (week == 1)
            {
                return LifeStage.Child;
            }
            else if (week == 2)
            {
                return LifeStage.Teenager;
            }
            else
            {
                return LifeStage.Adult;
            }
        }

        private IEnumerator AIResponseTimeout()
        {
            yield return new WaitForSeconds(aiResponseTimeout);

            if (isCoolDown)
            {
                Debug.LogWarning("AI response timeout reached. Proceeding without AI response.");
                HandleAIFailure("Request timed out");
            }
        }

        private IEnumerator CoolDownWaitTimer()
        {
            yield return new WaitForSeconds(2f);

            if (isCoolDown)
            {
                isCoolDown = false;
                OnDayCompleted?.Invoke();
            }
        }

        private void OnRequestFailed(string errorMessage)
        {
            Debug.LogWarning($"AI request failed: {errorMessage}");
            
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            HandleAIFailure(errorMessage);
        }

        private void HandleAIFailure(string reason)
        {
            if (!isCoolDown) return; // Already handled
            
            if (thinkingIndicator != null)
            {
                thinkingIndicator.Display(false);
            }

            if (chatPanel != null)
            {
                chatPanel.AddMessage($"AI response failed: {reason}. Continuing to next day...", false);
            }

            isCoolDown = false;
            OnDayCompleted?.Invoke();
        }

        private void OnResponseReceived(ChatResponse response)
        {
            Debug.Log("AI response received successfully!");

            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            if (thinkingIndicator != null)
            {
                thinkingIndicator.Display(false);
            }

            if (petMoodIcon != null)
            {
                petMoodIcon.SetEmotion(response.emotion);
            }

            isCoolDown = false;
            OnDayCompleted?.Invoke();
        }

        private string TraitSummary()
        {
            List<string> summary = new List<string>();
            foreach (var kvp in petTraits)
            {
                summary.Add($"{kvp.Key}:{kvp.Value}");
            }
            return string.Join(", ", summary);
        }
    }
}