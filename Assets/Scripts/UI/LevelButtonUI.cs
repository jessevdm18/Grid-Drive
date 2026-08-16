using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI-state voor één LevelButton-prefab (Background, nummer, sterren, lock).
/// Unlock-bepaling blijft in LevelSelectUI / SaveManager.
/// Special-mission badge = presentation only (LevelData.objectiveType).
/// </summary>
public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text levelNumberText;
    [SerializeField] private GameObject starsRow;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button button;

    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;

    [SerializeField] private Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Special Mission (optioneel — zelf in prefab toevoegen)")]
    [Tooltip("Root van de special badge. Standaard inactive in prefab.")]
    [SerializeField] private GameObject specialMissionBadge;

    [SerializeField] private Image specialMissionIcon;
    [SerializeField] private TMP_Text specialMissionLabel;

    [Tooltip("Icoon voor TimedAmbulance. Leeg = bestaande Image-sprite laten.")]
    [SerializeField] private Sprite ambulanceMissionIcon;

    [SerializeField] private string timedAmbulanceLabel = "SPECIAL";

    /// <summary>
    /// Button op deze prefab (voor OnClick in LevelSelectUI).
    /// </summary>
    public Button Button => button;

    /// <summary>
    /// Zet nummer, unlocked/locked visuals, sterren en special-mission badge.
    /// </summary>
    public void Setup(
        int displayNumber,
        bool isUnlocked,
        int earnedStars,
        Sprite filledStarSprite,
        Sprite emptyStarSprite,
        LevelObjectiveType objectiveType = LevelObjectiveType.Classic)
    {
        SetLevelNumber(displayNumber);
        ApplyLockState(isUnlocked);
        ApplyStars(isUnlocked ? earnedStars : 0, filledStarSprite, emptyStarSprite);
        ApplySpecialMissionVisual(objectiveType);
    }

    /// <summary>
    /// Zet het zichtbare levelnummer (1-based).
    /// </summary>
    public void SetLevelNumber(int displayNumber)
    {
        if (levelNumberText != null)
        {
            levelNumberText.text = displayNumber.ToString();
        }
    }

    /// <summary>
    /// Toont/verbergt special badge op basis van LevelData.objectiveType.
    /// Onafhankelijk van lock/stars/completed.
    /// </summary>
    public void ApplySpecialMissionVisual(LevelObjectiveType objectiveType)
    {
        if (specialMissionBadge == null)
        {
            return;
        }

        switch (objectiveType)
        {
            case LevelObjectiveType.TimedAmbulance:
                specialMissionBadge.SetActive(true);

                if (specialMissionLabel != null)
                {
                    specialMissionLabel.text = timedAmbulanceLabel;
                }

                if (specialMissionIcon != null && ambulanceMissionIcon != null)
                {
                    specialMissionIcon.sprite = ambulanceMissionIcon;
                }

                break;

            case LevelObjectiveType.MoveLimit:
                specialMissionBadge.SetActive(true);

                if (specialMissionLabel != null)
                {
                    specialMissionLabel.text = timedAmbulanceLabel;
                }

                break;

            case LevelObjectiveType.Classic:
            default:
                specialMissionBadge.SetActive(false);
                break;
        }
    }

    private void ApplyLockState(bool isUnlocked)
    {
        if (isUnlocked)
        {
            if (lockIcon != null)
            {
                lockIcon.SetActive(false);
            }

            if (levelNumberText != null)
            {
                levelNumberText.gameObject.SetActive(true);
            }

            if (starsRow != null)
            {
                starsRow.SetActive(true);
            }

            if (button != null)
            {
                button.interactable = true;
            }

            if (background != null)
            {
                background.color = Color.white;
            }
        }
        else
        {
            if (lockIcon != null)
            {
                lockIcon.SetActive(true);
            }

            if (levelNumberText != null)
            {
                levelNumberText.gameObject.SetActive(false);
            }

            if (starsRow != null)
            {
                starsRow.SetActive(false);
            }

            if (button != null)
            {
                button.interactable = false;
            }

            if (background != null)
            {
                background.color = lockedColor;
            }
        }
    }

    private void ApplyStars(int stars, Sprite filledStarSprite, Sprite emptyStarSprite)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        SetStarImage(star1, stars >= 1, filledStarSprite, emptyStarSprite);
        SetStarImage(star2, stars >= 2, filledStarSprite, emptyStarSprite);
        SetStarImage(star3, stars >= 3, filledStarSprite, emptyStarSprite);
    }

    private static void SetStarImage(
        Image image,
        bool filled,
        Sprite filledStarSprite,
        Sprite emptyStarSprite)
    {
        if (image == null)
        {
            return;
        }

        Sprite sprite = filled ? filledStarSprite : emptyStarSprite;
        if (sprite != null)
        {
            image.sprite = sprite;
        }
    }
}
