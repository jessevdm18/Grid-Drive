using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip blockedClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip buttonClip;

    [SerializeField, Range(0f, 1f)]
    private float sfxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMove()
    {
        if (moveClip != null)
        {
            audioSource.PlayOneShot(moveClip, sfxVolume);
        }
    }

    public void PlayBlocked()
    {
        if (blockedClip != null)
        {
            audioSource.PlayOneShot(blockedClip, sfxVolume);
        }
    }

    public void PlayWin()
    {
        if (winClip != null)
        {
            audioSource.PlayOneShot(winClip, sfxVolume);
        }
    }

    public void PlayCoin()
    {
        if (coinClip != null)
        {
            audioSource.PlayOneShot(coinClip, sfxVolume);
        }
    }

    public void PlayButton()
    {
        if (buttonClip != null)
        {
            audioSource.PlayOneShot(buttonClip, sfxVolume);
        }
    }
}
