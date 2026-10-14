package com.raddergames.griddrive.notifications;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

/** Delivers the single lives-full alarm, including after reboot. */
public class LivesFullReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null) {
            Log.i("LivesNotification", "receiver fired intent=null");
            return;
        }

        String action = intent.getAction();
        Log.i("LivesNotification", "receiver fired action=" + action);
        if (Intent.ACTION_BOOT_COMPLETED.equals(action)
                || "android.intent.action.QUICKBOOT_POWERON".equals(action)
                || "com.htc.intent.action.QUICKBOOT_POWERON".equals(action)) {
            LivesFullNotifier.rescheduleAfterBoot(context);
            FreeCoinsNotifier.rescheduleAfterBoot(context);
            return;
        }

        if (LivesFullNotifier.ACTION_SHOW.equals(action)) {
            LivesFullNotifier.showFromAlarm(context);
        }
    }
}
