using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Runtime bootstrap via reflection so this class has no hard dependency on
/// loading Firebase.* types at probe time. Actual work lives in FirebaseManager.
/// </summary>
public static class FirebaseRuntimeBootstrap
{
    private static bool afterSceneLoadFallbackRan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnSubsystemRegistration()
    {
        afterSceneLoadFallbackRan = false;
        TryInvokeFirebaseManager("ResetStaticsForPlayMode");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FirebaseInit] BootstrapState isPlaying=" + Application.isPlaying
        );
#endif
        // Reset again: reliable with Disable Domain Reload (Unity 6).
        TryInvokeFirebaseManager("ResetStaticsForPlayMode");
        TryInvokeFirebaseManager("EnsureInstance");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        object instance = TryGetFirebaseManagerProperty("Instance");
        if (instance == null)
        {
            Debug.LogError(
                "[FirebaseInit] BootstrapEarlyReturn Reason=EnsureInstance left Instance null"
            );
        }
        else
        {
            Debug.Log("[FirebaseInit] Stage=ManagerExistsAfterBootstrap");
        }
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[FirebaseInit] BootstrapEarlyReturn Reason=not_playing_after_scene_load"
            );
#endif
            return;
        }

        object instance = TryGetFirebaseManagerProperty("Instance");
        if (instance != null)
        {
            return;
        }

        if (afterSceneLoadFallbackRan)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[FirebaseInit] BootstrapEarlyReturn Reason=after_scene_load_fallback_already_ran"
            );
#endif
            return;
        }

        afterSceneLoadFallbackRan = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[FirebaseInit] Stage=AfterSceneLoadFallbackCreating");
#endif
        TryInvokeFirebaseManager("EnsureInstance");
    }

    private static Type FindFirebaseManagerType()
    {
        // Prefer already-loaded Assembly-CSharp without forcing Firebase type init early.
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = assembly.GetName().Name;
            if (name != "Assembly-CSharp" && name != "Assembly-CSharp-firstpass")
            {
                continue;
            }

            try
            {
                Type type = assembly.GetType("FirebaseManager", throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    "[FirebaseInit] Exception=GetType(FirebaseManager) failed: " + ex
                );
#endif
            }
        }

        try
        {
            return Type.GetType("FirebaseManager, Assembly-CSharp", throwOnError: false);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(
                "[FirebaseInit] Exception=Type.GetType(FirebaseManager) failed: " + ex
            );
#endif
            return null;
        }
    }

    private static void TryInvokeFirebaseManager(string methodName)
    {
        try
        {
            Type type = FindFirebaseManagerType();
            if (type == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    "[FirebaseInit] BootstrapEarlyReturn Reason=FirebaseManager_type_not_found method=" +
                    methodName
                );
#endif
                return;
            }

            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Static
            );
            if (method == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    "[FirebaseInit] BootstrapEarlyReturn Reason=method_not_found method=" +
                    methodName
                );
#endif
                return;
            }

            method.Invoke(null, null);
        }
        catch (TargetInvocationException tie)
        {
            Exception inner = tie.InnerException ?? tie;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(
                "[FirebaseInit] Exception=Invoke " + methodName + " => " + inner
            );
#endif
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(
                "[FirebaseInit] Exception=Invoke " + methodName + " => " + ex
            );
#endif
        }
    }

    private static object TryGetFirebaseManagerProperty(string propertyName)
    {
        try
        {
            Type type = FindFirebaseManagerType();
            if (type == null)
            {
                return null;
            }

            PropertyInfo property = type.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Static
            );
            return property != null ? property.GetValue(null) : null;
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(
                "[FirebaseInit] Exception=GetProperty " + propertyName + " => " + ex
            );
#endif
            return null;
        }
    }
}
