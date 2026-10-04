using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Android 13+ POST_NOTIFICATIONS runtime permission helper.
/// No-ops on Editor / iOS / older Android. Never blocks gameplay.
/// </summary>
public static class AndroidNotificationPermission
{
    public const string PostNotifications = "android.permission.POST_NOTIFICATIONS";

    public static bool IsRuntimePermissionRequired
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GetAndroidSdkInt() >= 33;
#else
            return false;
#endif
        }
    }

    public static bool HasPermission
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsRuntimePermissionRequired)
            {
                return true;
            }

            return Permission.HasUserAuthorizedPermission(PostNotifications);
#else
            // Editor / non-Android: treat as not granted for opt-in UI honesty,
            // but IsRuntimePermissionRequired is false so enable flow skips the dialog.
            return !IsRuntimePermissionRequired;
#endif
        }
    }

    /// <summary>
    /// Requests POST_NOTIFICATIONS on Android 13+. Returns whether notifications may proceed.
    /// </summary>
    public static async Task<bool> RequestIfNeededAsync()
    {
#if UNITY_EDITOR
        await Task.Yield();
        return false;
#elif UNITY_ANDROID
        if (!IsRuntimePermissionRequired)
        {
            return true;
        }

        if (Permission.HasUserAuthorizedPermission(PostNotifications))
        {
            return true;
        }

        var tcs = new TaskCompletionSource<bool>();
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += _ => tcs.TrySetResult(true);
        callbacks.PermissionDenied += _ => tcs.TrySetResult(false);
        callbacks.PermissionDeniedAndDontAskAgain += _ => tcs.TrySetResult(false);
        Permission.RequestUserPermission(PostNotifications, callbacks);

        // Timeout so a stuck dialog never freezes Settings forever.
        Task delay = Task.Delay(60000);
        Task finished = await Task.WhenAny(tcs.Task, delay);
        if (finished != tcs.Task)
        {
            return Permission.HasUserAuthorizedPermission(PostNotifications);
        }

        return await tcs.Task;
#else
        await Task.Yield();
        // Future Apple: use FirebaseMessaging.RequestPermissionAsync after APNs setup.
        return false;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static int GetAndroidSdkInt()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 0;
        }
    }
#endif
}
