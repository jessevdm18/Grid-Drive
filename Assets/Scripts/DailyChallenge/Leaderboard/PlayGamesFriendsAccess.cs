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
                if (silent != SignInStatus.Success &&
                    !PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    SignInStatus manual = await ManualAuthenticateAsync();
                    if (manual != SignInStatus.Success &&
                        !PlayGamesPlatform.Instance.IsAuthenticated())
                    {
                        return new Result { Status = Status.NotAuthenticated };
                    }
                }
            }

            FriendsListVisibilityStatus visibility = await GetVisibilityAsync(forceReload: true);
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

            if (visibility != FriendsListVisibilityStatus.Visible)
            {
                if (!showConsentUi)
                {
                    return new Result { Status = Status.ConsentRequired };
                }

                UIStatus ui = await AskForConsentAsync();
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
            }

            List<string> ids = await LoadAllFriendIdsAsync();
            if (ids == null)
            {
                LoadFriendsStatus last = PlayGamesPlatform.Instance.GetLastLoadFriendsStatus();
                if (last == LoadFriendsStatus.ResolutionRequired)
                {
                    return new Result { Status = Status.ConsentRequired };
                }

                return new Result { Status = Status.Failed };
            }

            Debug.Log("[Friends] Play friends loaded count=" + ids.Count);
            return new Result { Status = Status.Loaded, PlayerIds = ids };
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
                true,
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
