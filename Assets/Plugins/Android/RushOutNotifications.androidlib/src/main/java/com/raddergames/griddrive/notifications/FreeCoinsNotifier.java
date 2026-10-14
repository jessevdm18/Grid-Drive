package com.raddergames.griddrive.notifications;

import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.util.Log;

/** One local alarm for the moment all 3 free coin videos are available again. */
public final class FreeCoinsNotifier {
    public static final String CHANNEL_ID = "rewards";
    public static final int NOTIFICATION_ID = 51006;
    public static final String ACTION_SHOW = "com.raddergames.griddrive.notifications.FREE_COINS";
    private static final String TAG = "FreeCoinNotification";
    private static final String PREFS = "rushout_free_coins_notification";
    private static final String KEY_AT = "trigger_at";
    private static final String KEY_TITLE = "title";
    private static final String KEY_BODY = "body";
    private static final String KEY_SKIP = "skip_relevance";

    private FreeCoinsNotifier() {
    }

    public static String schedule(
            Context context,
            long triggerAtMillis,
            String title,
            String body,
            boolean skipRelevance) {
        Context app = context.getApplicationContext();
        if (triggerAtMillis <= System.currentTimeMillis()) {
            Log.i(TAG, "schedule skipped — trigger is not in the future alarmId=" + NOTIFICATION_ID);
            cancel(app, "trigger not in the future");
            return "skipped_past";
        }

        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        prefs.edit()
                .putLong(KEY_AT, triggerAtMillis)
                .putString(KEY_TITLE, title)
                .putString(KEY_BODY, body)
                .putBoolean(KEY_SKIP, skipRelevance)
                .apply();

        AlarmManager alarms = (AlarmManager) app.getSystemService(Context.ALARM_SERVICE);
        if (alarms == null) {
            Log.i(TAG, "schedule failed — AlarmManager missing alarmId=" + NOTIFICATION_ID);
            return "no_alarm_manager";
        }

        PendingIntent pending = pendingIntent(app);
        Log.i(TAG, "pendingIntent created alarmId=" + NOTIFICATION_ID + " replaced=true");
        try {
            if (Build.VERSION.SDK_INT >= 31 && alarms.canScheduleExactAlarms()) {
                alarms.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
                Log.i(TAG, "schedule method=setExactAndAllowWhileIdle alarmId=" + NOTIFICATION_ID);
                return "setExactAndAllowWhileIdle";
            }
        } catch (SecurityException ignored) {
            Log.i(TAG, "exact alarm denied — falling back alarmId=" + NOTIFICATION_ID);
        }

        if (Build.VERSION.SDK_INT >= 23) {
            alarms.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
            Log.i(TAG, "schedule method=setAndAllowWhileIdle alarmId=" + NOTIFICATION_ID);
            return "setAndAllowWhileIdle";
        }

        alarms.set(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
        Log.i(TAG, "schedule method=set alarmId=" + NOTIFICATION_ID);
        return "set";
    }

    public static void cancel(Context context, String reason) {
        Context app = context.getApplicationContext();
        AlarmManager alarms = (AlarmManager) app.getSystemService(Context.ALARM_SERVICE);
        if (alarms != null) {
            alarms.cancel(pendingIntent(app));
        }

        NotificationManager notifications =
                (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifications != null) {
            notifications.cancel(NOTIFICATION_ID);
        }

        app.getSharedPreferences(PREFS, Context.MODE_PRIVATE).edit().clear().apply();
        Log.i(TAG, "alarm cancelled alarmId=" + NOTIFICATION_ID + " reason=" + reason);
    }

    public static void showFromAlarm(Context context) {
        Context app = context.getApplicationContext();
        Log.i(TAG, "receiver fired alarmId=" + NOTIFICATION_ID);
        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        boolean skipRelevance = prefs.getBoolean(KEY_SKIP, false);
        if (!skipRelevance && !NotificationRelevance.freeCoinsStillRelevant(app)) {
            prefs.edit().clear().apply();
            Log.i(TAG, "notification posted=false reason=no longer relevant");
            return;
        }

        String title = prefs.getString(KEY_TITLE, "FREE COINS READY");
        String body = prefs.getString(KEY_BODY, "3 free coin videos are ready to watch!");
        boolean channelReady = ensureChannel(app);
        Log.i(TAG, "channel available=" + channelReady + " channelId=" + CHANNEL_ID);
        post(app, prefs, title, body);
    }

    public static void rescheduleAfterBoot(Context context) {
        Context app = context.getApplicationContext();
        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        long triggerAt = prefs.getLong(KEY_AT, 0L);
        if (triggerAt <= 0L) {
            Log.i(TAG, "boot reschedule skipped — no saved trigger");
            return;
        }

        String title = prefs.getString(KEY_TITLE, "FREE COINS READY");
        String body = prefs.getString(KEY_BODY, "3 free coin videos are ready to watch!");
        boolean skip = prefs.getBoolean(KEY_SKIP, false);
        if (triggerAt <= System.currentTimeMillis()) {
            Log.i(TAG, "boot reschedule — trigger already passed, posting now");
            showFromAlarm(app);
            return;
        }

        schedule(app, triggerAt, title, body, skip);
    }

    private static void post(Context app, SharedPreferences prefs, String title, String body) {
        NotificationManager notifications =
                (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifications == null) {
            Log.i(TAG, "notification posted=false reason=no NotificationManager");
            return;
        }

        Intent launch = app.getPackageManager().getLaunchIntentForPackage(app.getPackageName());
        PendingIntent content = null;
        if (launch != null) {
            launch.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP);
            int flags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= 23) {
                flags |= PendingIntent.FLAG_IMMUTABLE;
            }
            content = PendingIntent.getActivity(app, NOTIFICATION_ID, launch, flags);
        }

        int icon = app.getApplicationInfo().icon;
        if (icon == 0) {
            icon = android.R.drawable.stat_notify_chat;
        }

        Notification.Builder builder = Build.VERSION.SDK_INT >= 26
                ? new Notification.Builder(app, CHANNEL_ID)
                : new Notification.Builder(app);
        builder.setContentTitle(title)
                .setContentText(body)
                .setSmallIcon(icon)
                .setAutoCancel(true);
        if (content != null) {
            builder.setContentIntent(content);
        }

        try {
            notifications.notify(NOTIFICATION_ID, builder.build());
            Log.i(TAG, "notification posted=true alarmId=" + NOTIFICATION_ID + " icon=" + icon);
            prefs.edit().clear().apply();
        } catch (RuntimeException ex) {
            Log.i(TAG, "notification posted=false reason=" + ex.getClass().getSimpleName());
        }
    }

    private static PendingIntent pendingIntent(Context app) {
        Intent intent = new Intent(app, FreeCoinsReceiver.class);
        intent.setAction(ACTION_SHOW);
        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= 23) {
            flags |= PendingIntent.FLAG_IMMUTABLE;
        }
        return PendingIntent.getBroadcast(app, NOTIFICATION_ID, intent, flags);
    }

    private static boolean ensureChannel(Context app) {
        if (Build.VERSION.SDK_INT < 26) {
            return true;
        }

        NotificationManager notifications =
                (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifications == null) {
            return false;
        }

        NotificationChannel channel = new NotificationChannel(
                CHANNEL_ID,
                "Rewards",
                NotificationManager.IMPORTANCE_DEFAULT);
        channel.setDescription("Notifies when free coin videos are ready");
        notifications.createNotificationChannel(channel);
        return notifications.getNotificationChannel(CHANNEL_ID) != null;
    }
}
