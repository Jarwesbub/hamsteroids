using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Sprite SunIcon;
    [SerializeField] private Sprite MoonIcon;
    [SerializeField] private Transform SunMoonHidePos;

    [SerializeField] private Image SunMoon;
    [SerializeField] private Image Windows;

    [SerializeField] private Color DayColor;
    [SerializeField] private Color NightColor;
    [SerializeField] private float transitionDuration = 1f;

    private Coroutine transitionCoroutine;
    private Vector3 sunMoonOriginalPos;

    void Awake()
    {
        Windows.color = DayColor;
        SunMoon.sprite = SunIcon;
        sunMoonOriginalPos = SunMoon.transform.position;
    }

    public void SetDayCycle()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionColor(NightColor, DayColor, SunIcon));
    }

    public void SetNightCycle()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionColor(DayColor, NightColor, MoonIcon));
    }

    private IEnumerator TransitionColor(Color startColor, Color targetColor, Sprite targetSprite)
    {
        float elapsed = 0f;
        float halfDuration = transitionDuration * 0.5f;

        // First half: move to hide position
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Windows.color = Color.Lerp(startColor, targetColor, t * 0.5f);
            SunMoon.transform.position = Vector3.Lerp(sunMoonOriginalPos, SunMoonHidePos.position, t);
            yield return null;
        }

        // Change sprite at midpoint
        SunMoon.sprite = targetSprite;

        // Second half: return to original position
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            Windows.color = Color.Lerp(startColor, targetColor, 0.5f + t * 0.5f);
            SunMoon.transform.position = Vector3.Lerp(SunMoonHidePos.position, sunMoonOriginalPos, t);
            yield return null;
        }

        Windows.color = targetColor;
        SunMoon.transform.position = sunMoonOriginalPos;
    }
}
