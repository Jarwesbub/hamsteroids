using UnityEngine;
using UnityEngine.UI;
using Neocortex.Data;


public class PetMoodIcon : MonoBehaviour
{
    [SerializeField] private Image moodImage;

    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite sadSprite;
    public Sprite excitedSprite;
    public Sprite angrySprite;

    public void SetEmotion(Emotions emotion)
    {
        if (moodImage == null) return;

        switch (emotion)
        {
            case Emotions.Neutral: moodImage.sprite = neutralSprite; break;
            case Emotions.Happy: moodImage.sprite = happySprite; break;
            case Emotions.Angry: moodImage.sprite = angrySprite; break;
        }
    }
}
