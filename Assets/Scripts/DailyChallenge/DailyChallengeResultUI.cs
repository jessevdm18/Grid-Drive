using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored Daily Challenge completion result panel (Gameplay).
/// Scene owns styling; runtime fills score / moves / time values.
/// </summary>
public class DailyChallengeResultUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreLabelText;
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI movesLabelText;
    [SerializeField] private TextMeshProUGUI movesValueText;
    [SerializeField] private TextMeshProUGUI timeLabelText;
    [SerializeField] private TextMeshProUGUI timeValueText;
    [SerializeField] private TextMeshProUGUI footerText;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinue);
        }
    }

    public void Show(DailyChallengeResult result)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE COMPLETE!";
        }

        if (scoreLabelText != null)
        {
            scoreLabelText.text = "SCORE";
        }

        if (scoreValueText != null)
        {
            scoreValueText.text = DailyChallengeTimeFormat.FormatScore(result.Score);
        }

        if (movesLabelText != null)
        {
            movesLabelText.text = "MOVES";
        }

        if (movesValueText != null)
        {
            movesValueText.text = result.Moves.ToString();
        }

        if (timeLabelText != null)
        {
            timeLabelText.text = "TIME";
        }

        if (timeValueText != null)
        {
            timeValueText.text = DailyChallengeTimeFormat.Format(result.CompletionTimeMilliseconds);
        }

        if (footerText != null)
        {
            footerText.text = "ONE ATTEMPT COMPLETE";
        }

        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void OnContinue()
    {
        Time.timeScale = 1f;
        DailyChallengeContext.ClearSession();
        SceneTransition.LoadScene("MainMenu");
    }
}
