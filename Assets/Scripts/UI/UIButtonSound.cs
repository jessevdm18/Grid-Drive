using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;
    private AudioManager audioManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        audioManager = FindFirstObjectByType<AudioManager>();
        button.onClick.AddListener(PlayClickSound);

        Debug.Log(
            "UIButtonSound " + gameObject.name +
            " | Button=" + (button != null) +
            " | AudioManager=" + (audioManager != null)
        );
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        Debug.Log("UIButtonSound " + gameObject.name + " clicked");
        audioManager?.PlayButton();
    }
}
