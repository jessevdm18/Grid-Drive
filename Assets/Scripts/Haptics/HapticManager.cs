using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Centrale, lichte haptic API voor Grid Drive.
/// Geen GameObject, geen packages — Android JNI + minimale iOS native helper.
/// </summary>
public static class HapticManager
{
    /// <summary>
    /// Klaar voor toekomstige settings-UI. Geen PlayerPrefs-key tot die bestaat.
    /// </summary>
    public static bool Enabled { get; set; } = true;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void GridDrive_HapticLight();

    [DllImport("__Internal")]
    private static extern void GridDrive_HapticMedium();

    [DllImport("__Internal")]
    private static extern void GridDrive_HapticSuccess();
#endif

    /// <summary>
    /// Subtiele korte tik (vehicle blocked, lichte UI).
    /// </summary>
    public static void PlayLightImpact()
    {
        if (!Enabled)
        {
            return;
        }

        PlayNative(HapticKind.Light);
    }

    /// <summary>
    /// Medium tik (bijv. purchase success — later koppelen).
    /// </summary>
    public static void PlayMediumImpact()
    {
        if (!Enabled)
        {
            return;
        }

        PlayNative(HapticKind.Medium);
    }

    /// <summary>
    /// Success-notificatie (bijv. level win — later koppelen).
    /// </summary>
    public static void PlaySuccess()
    {
        if (!Enabled)
        {
            return;
        }

        PlayNative(HapticKind.Success);
    }

    private enum HapticKind
    {
        Light,
        Medium,
        Success
    }

    private static void PlayNative(HapticKind kind)
    {
#if UNITY_EDITOR
        // Editor: no-op (geen errors).
        return;
#elif UNITY_ANDROID
        PlayAndroid(kind);
#elif UNITY_IOS
        PlayIos(kind);
#else
        return;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Light: korte, lage amplitude. Medium/Success: iets langer/sterker.
    private static void PlayAndroid(HapticKind kind)
    {
        long durationMs;
        int amplitude;

        switch (kind)
        {
            case HapticKind.Medium:
                durationMs = 35L;
                amplitude = 120;
                break;
            case HapticKind.Success:
                durationMs = 45L;
                amplitude = 160;
                break;
            case HapticKind.Light:
            default:
                durationMs = 18L;
                amplitude = 48;
                break;
        }

        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (activity == null)
                {
                    return;
                }

                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = version.GetStatic<int>("SDK_INT");
                    AndroidJavaObject vibrator = GetVibrator(context, sdkInt);
                    if (vibrator == null)
                    {
                        return;
                    }

                    using (vibrator)
                    {
                        bool hasVibrator = vibrator.Call<bool>("hasVibrator");
                        if (!hasVibrator)
                        {
                            return;
                        }

                        if (sdkInt >= 26)
                        {
                            using (AndroidJavaClass vibrationEffectClass =
                                       new AndroidJavaClass("android.os.VibrationEffect"))
                            using (AndroidJavaObject effect =
                                       vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                           "createOneShot",
                                           durationMs,
                                           amplitude))
                            {
                                vibrator.Call("vibrate", effect);
                            }
                        }
                        else
                        {
                            vibrator.Call("vibrate", durationMs);
                        }
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Geen spam; haptics mogen stil falen.
        }
    }

    private static AndroidJavaObject GetVibrator(AndroidJavaObject context, int sdkInt)
    {
        if (sdkInt >= 31)
        {
            using (AndroidJavaObject vibratorManager =
                       context.Call<AndroidJavaObject>("getSystemService", "vibrator_manager"))
            {
                if (vibratorManager == null)
                {
                    return null;
                }

                return vibratorManager.Call<AndroidJavaObject>("getDefaultVibrator");
            }
        }

        return context.Call<AndroidJavaObject>("getSystemService", "vibrator");
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private static void PlayIos(HapticKind kind)
    {
        try
        {
            switch (kind)
            {
                case HapticKind.Medium:
                    GridDrive_HapticMedium();
                    break;
                case HapticKind.Success:
                    GridDrive_HapticSuccess();
                    break;
                case HapticKind.Light:
                default:
                    GridDrive_HapticLight();
                    break;
            }
        }
        catch (System.Exception)
        {
            // Geen spam; haptics mogen stil falen.
        }
    }
#endif
}
