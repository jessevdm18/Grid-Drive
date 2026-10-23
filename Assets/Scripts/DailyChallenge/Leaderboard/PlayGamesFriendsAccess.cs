using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
#endif

/// <summary>
/// Google Play Games friends consent and player IDs.
/// Android player builds only. Editor never calls Play Games.
/// </summary>
public static class PlayGamesFriendsAccess
{
    public enum Status
    {
        Unsupported = 0,
        NotAuthenticated = 1,
        ConsentRequired = 2,
        Denied = 3,
        Unavailable = 4,
        Loaded = 5,
        Failed = 6
    }

    public sealed class Result
    {
        public Status Status;
        public List<string> PlayerIds = new List<string>();
    }

    public static Task<Result> LoadAsync(bool showConsentUi)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return LoadAndroidAsync(showConsentUi);
#else
        return Task.FromResult(new Result { Status = Status.Unsupported });
#endif
    }

    /// <summary>
    /// Opens Google Play Games via the public https://play.google.com/games link.
    /// The system chooses the installed Play Games app or the browser.
    /// This is not an add-friends flow.
    /// </summary>
    public static void OpenPlayGames()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject(
                "android.content.Intent", "android.intent.action.VIEW"))
            using (var uriClass = new AndroidJavaClass("android.net.Uri"))
            using (var uri = uriClass.CallStatic<AndroidJavaObject>(
                "parse", "https://play.google.com/games"))
            {
                intent.Call<AndroidJavaObject>("setData", uri);
                activity.Call("startActivity", intent);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Friends] Open Play Games failed: " + ex.GetType().Name);
        }
#endif
    }

    /// <summary>
    /// True when the current Play Games app is installed, so its Android app
    /// details screen can be opened. Does not use the older Games package.
    /// </summary>
    public static bool CanOpenPlayGamesSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return IsPackageInstalled("com.google.android.play.games");
#else
        return false;
#endif
    }

    public static void OpenPlayGamesSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!CanOpenPlayGamesSettings())
        {
            return;
        }

        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject(
                "android.content.Intent",
                "android.settings.APPLICATION_DETAILS_SETTINGS"))
            using (var uriClass = new AndroidJavaClass("android.net.Uri"))
            using (var uri = uriClass.CallStatic<AndroidJavaObject>(
                "parse", "package:com.google.android.play.games"))
            {
                intent.Call<AndroidJavaObject>("setData", uri);
                activity.Call("startActivity", intent);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Friends] Open Play Games settings failed: " + ex.GetType().Name);
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool IsPackageInstalled(string packageName)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
            {
                packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private const int PageSize = 200;
    private const int MaxPages = 10;

    private static async Task<Result> LoadAndroidAsync(bool showConsentUi)
    {
        try
        {
            PlayGamesPlatform.Activate();
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                SignInStatus silent = await SilentAuthenticateAsync();
                Debug.Log(
                    "[Friends] silent auth=" + silent +
                    " authenticated=" + PlayGamesPlatform.Instance.IsAuthenticated());
                if (silent != SignInStatus.Success &&
                    !PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    SignInStatus manual = await ManualAuthenticateAsync();
                    Debug.Log(
                        "[Friends] manual auth=" + manual +
                        " authenticated=" + PlayGamesPlatform.Instance.IsAuthenticated());
                    if (manual != SignInStatus.Success &&
                        !PlayGamesPlatform.Instance.IsAuthenticated())
                    {
                        return new Result { Status = Status.NotAuthenticated };
                    }
                }
            }
            else
            {
                Debug.Log("[Friends] silent auth=already authenticated");
            }

            FriendsListVisibilityStatus visibility = await GetVisibilityAsync(forceReload: false);
            Debug.Log("[Friends] visibility=" + visibility);
            if (visibility == FriendsListVisibilityStatus.Unknown)
            {
                visibility = await GetVisibilityAsync(forceReload: true);
                Debug.Log("[Friends] visibility refreshed=" + visibility);
            }

            if (visibility == FriendsListVisibilityStatus.NetworkError)
            {
                return new Result { Status = Status.Failed };
            }

            if (visibility == FriendsListVisibilityStatus.NotAuthorized)
            {
                return new Result { Status = Status.NotAuthenticated };
            }

            if (visibility == FriendsListVisibilityStatus.Unavailable)
            {
                return new Result { Status = Status.Unavailable };
            }

            // Load before any consent UI. A granted list succeeds here, including
            // when visibility is still Unknown. AskForLoadFriendsResolution runs
            // only after loadFriends explicitly returns ResolutionRequired.
            Result withoutUi = await TryLoadFriendsAsync();
            if (withoutUi != null)
            {
                return withoutUi;
            }

            LoadFriendsStatus beforeConsent = PlayGamesPlatform.Instance.GetLastLoadFriendsStatus();
            Debug.Log("[Friends] loadFriends status=" + beforeConsent);
            if (beforeConsent != LoadFriendsStatus.ResolutionRequired)
            {
                return ResolutionOrFailure();
            }

            if (!showConsentUi)
            {
                return new Result { Status = Status.ConsentRequired };
            }

            UIStatus ui = await AskForConsentAsync();
            Debug.Log("[Friends] resolution ui=" + ui);
            if (ui == UIStatus.UserClosedUI)
            {
                return new Result { Status = Status.Denied };
            }

            if (ui != UIStatus.Valid)
            {
                return new Result
                {
                    Status = ui == UIStatus.NetworkError ? Status.Failed : Status.Denied
                };
            }

            Result afterConsent = await TryLoadFriendsAsync();
            if (afterConsent != null)
            {
                return afterConsent;
            }

            return ResolutionOrFailure();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Friends] Play friends failed: " + ex.GetType().Name);
            return new Result { Status = Status.Failed };
        }
    }

    private static Task<SignInStatus> SilentAuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        try
        {
            PlayGamesPlatform.Instance.Authenticate(status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(SignInStatus.InternalError);
        }

        return tcs.Task;
    }

    private static Task<SignInStatus> ManualAuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        try
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(SignInStatus.InternalError);
        }

        return tcs.Task;
    }

    private static Task<FriendsListVisibilityStatus> GetVisibilityAsync(bool forceReload)
    {
        var tcs = new TaskCompletionSource<FriendsListVisibilityStatus>();
        try
        {
            PlayGamesPlatform.Instance.GetFriendsListVisibility(
                forceReload,
                status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(FriendsListVisibilityStatus.Unknown);
        }

        return tcs.Task;
    }

    private static Task<UIStatus> AskForConsentAsync()
    {
        var tcs = new TaskCompletionSource<UIStatus>();
        try
        {
            PlayGamesPlatform.Instance.AskForLoadFriendsResolution(
                status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(UIStatus.InternalError);
        }

        return tcs.Task;
    }

    private static async Task<Result> TryLoadFriendsAsync()
    {
        List<string> ids = await LoadAllFriendIdsAsync();
        if (ids == null)
        {
            return null;
        }

        Debug.Log("[Friends] Play friends loaded count=" + ids.Count);
        return new Result { Status = Status.Loaded, PlayerIds = ids };
    }

    private static Result ResolutionOrFailure()
    {
        LoadFriendsStatus last = PlayGamesPlatform.Instance.GetLastLoadFriendsStatus();
        if (last == LoadFriendsStatus.ResolutionRequired)
        {
            return new Result { Status = Status.ConsentRequired };
        }

        if (last == LoadFriendsStatus.NotAuthorized)
        {
            return new Result { Status = Status.Denied };
        }

        return new Result { Status = Status.Failed };
    }

    private static async Task<List<string>> LoadAllFriendIdsAsync()
    {
        var ids = new List<string>();
        var seen = new HashSet<string>();
        LoadFriendsStatus status = await LoadFirstPageAsync();
        Accumulate(ids, seen);
        int pages = 1;
        while (status == LoadFriendsStatus.LoadMore && pages < MaxPages)
        {
            status = await LoadMoreAsync();
            Accumulate(ids, seen);
            pages++;
        }

        if (status != LoadFriendsStatus.Completed && status != LoadFriendsStatus.LoadMore)
        {
            return null;
        }

        return ids;
    }

    private static void Accumulate(List<string> ids, HashSet<string> seen)
    {
        PlayGamesLocalUser local = PlayGamesPlatform.Instance.localUser as PlayGamesLocalUser;
        IUserProfile[] friends = local != null ? local.friends : null;
        if (friends == null)
        {
            return;
        }

        for (int i = 0; i < friends.Length; i++)
        {
            string id = friends[i] != null ? friends[i].id : null;
            if (string.IsNullOrEmpty(id) || !seen.Add(id))
            {
                continue;
            }

            ids.Add(id);
        }
    }

    private static Task<LoadFriendsStatus> LoadFirstPageAsync()
    {
        var tcs = new TaskCompletionSource<LoadFriendsStatus>();
        try
        {
            PlayGamesPlatform.Instance.LoadFriends(
                PageSize,
                false,
                status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(LoadFriendsStatus.InternalError);
        }

        return tcs.Task;
    }

    private static Task<LoadFriendsStatus> LoadMoreAsync()
    {
        var tcs = new TaskCompletionSource<LoadFriendsStatus>();
        try
        {
            PlayGamesPlatform.Instance.LoadMoreFriends(
                PageSize,
                status => tcs.TrySetResult(status));
        }
        catch (Exception)
        {
            tcs.TrySetResult(LoadFriendsStatus.InternalError);
        }

        return tcs.Task;
    }
#endif
}
