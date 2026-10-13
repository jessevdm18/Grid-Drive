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

/**
 * One local alarm for the moment lives reach the maximum.
 * Replaces any previous alarm with the same id. Not a repeating alarm.
 */
public final class LivesFullNotifier {
    public static final String CHANNEL_ID = "lives";
    public static final int NOTIFICATION_ID = 51005;
    public static final String ACTION_SHOW = "com.raddergames.griddrive.notifications.LIVES_FULL";
    private static final String PREFS = "rushout_lives_notification";
    private static final String KEY_AT = "trigger_at";
    private static final String KEY_TITLE = "title";
    private static final String KEY_BODY = "body";

    private LivesFullNotifier() {
    }

    public static void schedule(Context context, long triggerAtMillis, String title, String body) {
        Context app = context.getApplicationContext();
        if (triggerAtMillis <= System.currentTimeMillis()) {
            cancel(app);
            return;
        }

        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        prefs.edit()
                .putLong(KEY_AT, triggerAtMillis)
                .putString(KEY_TITLE, title)
                .putString(KEY_BODY, body)
                .apply();

        AlarmManager alarms = (AlarmManager) app.getSystemService(Context.ALARM_SERVICE);
        if (alarms == null) {
            return;
        }

        PendingIntent pending = pendingIntent(app);
        try {
            if (Build.VERSION.SDK_INT >= 31 && alarms.canScheduleExactAlarms()) {
                alarms.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
                return;
            }
        } catch (SecurityException ignored) {
            // Exact alarms are not granted. Fall through to an idle-allowed alarm.
        }

        if (Build.VERSION.SDK_INT >= 23) {
            alarms.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
        } else {
            alarms.set(AlarmManager.RTC_WAKEUP, triggerAtMillis, pending);
        }
    }

    public static void cancel(Context context) {
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
    }

    public static void showFromAlarm(Context context) {
        Context app = context.getApplicationContext();
        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        String title = prefs.getString(KEY_TITLE, "LIVES FULL");
        String body = prefs.getString(KEY_BODY, "Your lives are full. Come back and play!");
        ensureChannel(app);

        NotificationManager notifications =
                (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifications == null) {
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

        Notification.Builder builder = Build.VERSION.SDK_INT >= 26
                ? new Notification.Builder(app, CHANNEL_ID)
                : new Notification.Builder(app);
        builder.setContentTitle(title)
                .setContentText(body)
                .setSmallIcon(app.getApplicationInfo().icon)
                .setAutoCancel(true);
        if (content != null) {
            builder.setContentIntent(content);
        }
        notifications.notify(NOTIFICATION_ID, builder.build());
        prefs.edit().clear().apply();
    }

    public static void rescheduleAfterBoot(Context context) {
        Context app = context.getApplicationContext();
        SharedPreferences prefs = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        long triggerAt = prefs.getLong(KEY_AT, 0L);
        if (triggerAt <= 0L) {
            return;
        }

        String title = prefs.getString(KEY_TITLE, "LIVES FULL");
        String body = prefs.getString(KEY_BODY, "Your lives are full. Come back and play!");
        if (triggerAt <= System.currentTimeMillis()) {
            showFromAlarm(app);
            return;
        }

        schedule(app, triggerAt, title, body);
    }

    private static PendingIntent pendingIntent(Context app) {
        Intent intent = new Intent(app, LivesFullReceiver.class);
        intent.setAction(ACTION_SHOW);
        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= 23) {
            flags |= PendingIntent.FLAG_IMMUTABLE;
        }
        return PendingIntent.getBroadcast(app, NOTIFICATION_ID, intent, flags);
    }

    private static void ensureChannel(Context app) {
        if (Build.VERSION.SDK_INT < 26) {
            return;
        }

        NotificationManager notifications =
                (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifications == null) {
            return;
        }

        NotificationChannel channel = new NotificationChannel(
                CHANNEL_ID,
                "Lives",
                NotificationManager.IMPORTANCE_DEFAULT);
        channel.setDescription("Notifies when lives are full");
        notifications.createNotificationChannel(channel);
    }
}
