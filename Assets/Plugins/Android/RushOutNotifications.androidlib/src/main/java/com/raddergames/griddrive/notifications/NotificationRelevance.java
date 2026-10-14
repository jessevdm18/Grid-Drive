package com.raddergames.griddrive.notifications;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/** Reads Unity PlayerPrefs at delivery time. Does not write economy or lives state. */
final class NotificationRelevance {
    private static final long UNIX_EPOCH_MS = 62135596800000L;
    private static final long TICKS_PER_MS = 10000L;
    private static final long TWO_HOURS_TICKS = 2L * 60L * 60L * 10000000L;
    private static final long RELEVANCE_SLACK_TICKS = 3L * 60L * 10000000L;

    private NotificationRelevance() {
    }

    static boolean livesStillRelevant(Context context) {
        SharedPreferences prefs = playerPrefs(context);
        if (readInt(prefs, "RushOut_LivesNotificationsEnabled", 0) != 1) {
            Log.i("LivesNotification", "relevance=false reason=preference off");
            return false;
        }

        int lives = readInt(prefs, "RushOut_Lives", 5);
        if (lives >= 5) {
            Log.i("LivesNotification", "relevance=false reason=lives already full lives=" + lives);
            return false;
        }

        long nextTicks = readLong(prefs, "RushOut_NextLifeUtcTicks");
        if (nextTicks <= 0L) {
            Log.i("LivesNotification", "relevance=false reason=no next-life timer");
            return false;
        }

        int missingAfterNext = 5 - (lives + 1);
        if (missingAfterNext < 0) {
            missingAfterNext = 0;
        }

        long fullTicks = nextTicks + (TWO_HOURS_TICKS * missingAfterNext);
        long nowTicks = (UNIX_EPOCH_MS + System.currentTimeMillis()) * TICKS_PER_MS;
        if (fullTicks > nowTicks + RELEVANCE_SLACK_TICKS) {
            Log.i("LivesNotification", "relevance=false reason=full time still in the future lives=" + lives);
            return false;
        }

        Log.i("LivesNotification", "relevance=true lives=" + lives);
        return true;
    }

    static boolean freeCoinsStillRelevant(Context context) {
        SharedPreferences prefs = playerPrefs(context);
        if (readInt(prefs, "RushOut_FreeCoinNotificationsEnabled", 0) != 1) {
            Log.i("FreeCoinNotification", "relevance=false reason=preference off");
            return false;
        }

        String today = new SimpleDateFormat("yyyy-MM-dd", Locale.US).format(new Date());
        String saved = prefs.getString("GridDrive_FreeCoinsRewardedDate", "");
        int used = 0;
        if (today.equals(saved)) {
            used = readInt(prefs, "GridDrive_FreeCoinsRewardedUsed", 0);
        }

        if (used < 0) {
            used = 0;
        }

        if (used > 3) {
            used = 3;
        }

        int remaining = today.equals(saved) ? 3 - used : 3;
        if (remaining < 3) {
            Log.i(
                    "FreeCoinNotification",
                    "relevance=false reason=not all available remaining=" + remaining + " date=" + saved);
            return false;
        }

        Log.i("FreeCoinNotification", "relevance=true remaining=3 date=" + saved);
        return true;
    }

    private static SharedPreferences playerPrefs(Context context) {
        Context app = context.getApplicationContext();
        return app.getSharedPreferences(app.getPackageName() + ".v2.playerprefs", Context.MODE_PRIVATE);
    }

    private static int readInt(SharedPreferences prefs, String key, int fallback) {
        try {
            return prefs.getInt(key, fallback);
        } catch (ClassCastException ignored) {
            try {
                return Integer.parseInt(prefs.getString(key, Integer.toString(fallback)));
            } catch (RuntimeException bad) {
                return fallback;
            }
        }
    }

    private static long readLong(SharedPreferences prefs, String key) {
        String raw = "";
        try {
            raw = prefs.getString(key, "");
        } catch (ClassCastException ignored) {
            raw = "";
        }

        if (raw == null || raw.length() == 0) {
            return 0L;
        }

        try {
            return Long.parseLong(raw);
        } catch (NumberFormatException ignored) {
            return 0L;
        }
    }
}
