using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional Settings ACCOUNT section. Scene owns layout; this script owns text/button state.
/// Startup never opens this or prompts Google Play — linking is user-initiated only.
/// </summary>
public class AccountLinkUI : MonoBehaviour
{
    [Header("Roots (optional)")]
    [SerializeField] private GameObject accountRoot;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI helperText;

    [Header("Actions")]
    [SerializeField] private Button linkGooglePlayButton;
    [SerializeField] private TextMeshProUGUI linkButtonLabel;

    private bool linkInFlight;
    private bool refreshInFlight;

    private void Awake()
    {
        if (titleText != null)
        {
            titleText.text = "ACCOUNT";
        }
    }

    private void OnEnable()
    {
        if (linkGooglePlayButton != null)
        {
            linkGooglePlayButton.onClick.AddListener(OnLinkGooglePlayClicked);
        }

        ApplyLoadingPresentation();
        _ = RefreshAsync();
    }

    private void OnDisable()
    {
        if (linkGooglePlayButton != null)
        {
            linkGooglePlayButton.onClick.RemoveListener(OnLinkGooglePlayClicked);
        }
    }

    /// <summary>Call when Settings panel opens.</summary>
    public void RefreshFromSettingsOpen()
    {
        ApplyLoadingPresentation();
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (refreshInFlight)
        {
            return;
        }

        refreshInFlight = true;
        try
        {
            await DailyChallengeIdentityService.EnsureSignedInAsync();
            await DailyChallengeIdentityService.RefreshAccountStateAsync();
            ApplyPresentation(lastOutcomeMessage: null);
        }
        finally
        {
            refreshInFlight = false;
        }
    }

    private void OnLinkGooglePlayClicked()
    {
        if (linkInFlight)
        {
            return;
        }

        _ = LinkGooglePlayAsync();
    }

    private async Task LinkGooglePlayAsync()
    {
        linkInFlight = true;
        ApplyLinkingPresentation();
        try
        {
            AccountLinkOutcome outcome = await DailyChallengeIdentityService.LinkGooglePlayAsync();
            ApplyPresentation(outcome.Message);
        }
        finally
        {
            linkInFlight = false;
        }
    }

    private void ApplyLoadingPresentation()
    {
        if (statusText != null)
        {
            statusText.text = "Checking...";
        }

        if (helperText != null)
        {
            helperText.text = string.Empty;
        }

        SetLinkButton("LINK GOOGLE PLAY", interactable: false);
    }

    private void ApplyLinkingPresentation()
    {
        if (statusText != null)
        {
            statusText.text = "Connecting...";
        }

        SetLinkButton("CONNECTING...", interactable: false);
    }

    private void ApplyPresentation(string lastOutcomeMessage)
    {
        AccountLinkState state = DailyChallengeIdentityService.LinkState;
        bool playAvailable = DailyChallengeIdentityService.IsGooglePlayProviderAvailable;

        if (statusText != null)
        {
            switch (state)
            {
                case AccountLinkState.LinkedGooglePlay:
                case AccountLinkState.LinkedMultiple:
                    statusText.text = "Google Play linked";
                    break;
                case AccountLinkState.LinkedApple:
                    statusText.text = "Apple linked";
                    break;
                case AccountLinkState.Error:
                    statusText.text = "Could not connect. Try again.";
                    break;
                case AccountLinkState.Linking:
                    statusText.text = "Connecting...";
                    break;
                default:
                    statusText.text = "Playing as Guest";
                    break;
            }
        }

        if (helperText != null)
        {
            if (!string.IsNullOrEmpty(lastOutcomeMessage) &&
                state != AccountLinkState.LinkedGooglePlay &&
                state != AccountLinkState.LinkedMultiple)
            {
                helperText.text = lastOutcomeMessage;
            }
            else if (state == AccountLinkState.GuestAnonymous || state == AccountLinkState.Unknown)
            {
                helperText.text = playAvailable
                    ? "Link your account to protect your progress."
                    : "Google Play linking will be available in a future update.";
            }
            else
            {
                helperText.text = string.Empty;
            }
        }

        bool linkedPlay =
            state == AccountLinkState.LinkedGooglePlay ||
            state == AccountLinkState.LinkedMultiple;

        if (linkedPlay)
        {
            SetLinkButton("LINKED", interactable: false);
        }
        else if (state == AccountLinkState.Linking || linkInFlight)
        {
            SetLinkButton("CONNECTING...", interactable: false);
        }
        else
        {
            // Button remains visible even when provider unavailable so copy explains why.
            SetLinkButton("LINK GOOGLE PLAY", interactable: playAvailable && !linkInFlight);
        }
    }

    private void SetLinkButton(string label, bool interactable)
    {
        if (linkButtonLabel != null)
        {
            linkButtonLabel.text = label ?? string.Empty;
        }

        if (linkGooglePlayButton != null)
        {
            linkGooglePlayButton.interactable = interactable;
            if (accountRoot != null && !accountRoot.activeSelf)
            {
                accountRoot.SetActive(true);
            }
        }
    }
}
