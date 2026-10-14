using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

/// <summary>
/// Strongly typed Firebase Cloud Messaging boundary for Firebase Unity SDK <b>13.15.0</b>.
///
/// 13.15.0 uses token-based auto-init:
///   FirebaseMessaging.TokenRegistrationOnInitEnabled
///   FirebaseMessaging.GetTokenAsync()
/// RegistrationOnInitEnabled / RegisterAsync() are 13.16.0+ and must not be called.
/// </summary>
public static class FirebaseMessagingBridge
{
    private static EventHandler<MessageReceivedEventArgs> messageHandler;
    private static EventHandler<TokenReceivedEventArgs> tokenHandler;
    private static bool handlersAttached;

    /// <summary>Messaging package is installed and referenced.</summary>
    public static bool IsSdkAvailable => true;

    public static string SdkStatus =>
        "Firebase.Messaging 13.15.0 (TokenRegistrationOnInitEnabled + GetTokenAsync)";

    /// <summary>
    /// Opt-in control for 13.15.0: TokenRegistrationOnInitEnabled.
    /// Keep false until the user enables Daily notifications.
    /// </summary>
    public static void SetTokenRegistrationOnInitEnabled(bool enabled)
    {
        try
        {
            FirebaseMessaging.TokenRegistrationOnInitEnabled = enabled;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[DailyNotify] Failed to set TokenRegistrationOnInitEnabled: " +
                ex.GetType().Name);
        }
    }

    public static async Task<string> GetTokenOrEmptyAsync()
    {
        try
        {
            string token = await FirebaseMessaging.GetTokenAsync();
            return token ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[DailyNotify] GetTokenAsync failed: " + ex.GetType().Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// 13.15.0-compatible registration: obtain/create FCM token (never log it).
    /// Replaces RegisterAsync() which does not exist in 13.15.0.
    /// </summary>
    public static async Task<bool> EnsureTokenAsync()
    {
        string token = await GetTokenOrEmptyAsync();
        return !string.IsNullOrEmpty(token);
    }

    public static async Task<bool> SubscribeAsync(string topic)
    {
        if (string.IsNullOrEmpty(topic))
        {
            return false;
        }

        try
        {
            await FirebaseMessaging.SubscribeAsync(topic);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[DailyNotify] SubscribeAsync failed: " + ex.GetType().Name);
            return false;
        }
    }

    public static async Task<bool> UnsubscribeAsync(string topic)
    {
        if (string.IsNullOrEmpty(topic))
        {
            return false;
        }

        try
        {
            await FirebaseMessaging.UnsubscribeAsync(topic);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[DailyNotify] UnsubscribeAsync failed: " + ex.GetType().Name);
            return false;
        }
    }

    public static void AttachHandlers(
        EventHandler<MessageReceivedEventArgs> onMessageReceived,
        EventHandler<TokenReceivedEventArgs> onTokenReceived)
    {
        if (handlersAttached)
        {
            return;
        }

        messageHandler = onMessageReceived;
        tokenHandler = onTokenReceived;

        if (messageHandler != null)
        {
            FirebaseMessaging.MessageReceived += messageHandler;
        }

        if (tokenHandler != null)
        {
            FirebaseMessaging.TokenReceived += tokenHandler;
        }

        handlersAttached = true;
    }

    public static bool TryReadDailyChallengePayload(
        MessageReceivedEventArgs args,
        out string type,
        out string dayId)
    {
        type = string.Empty;
        dayId = string.Empty;
        if (args == null || args.Message == null || args.Message.Data == null)
        {
            return false;
        }

        IDictionary<string, string> data = args.Message.Data;
        if (data.TryGetValue("type", out string typeValue))
        {
            type = typeValue ?? string.Empty;
        }

        if (data.TryGetValue("dayId", out string dayValue))
        {
            dayId = dayValue ?? string.Empty;
        }

        return !string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(dayId);
    }
}
