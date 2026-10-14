package com.raddergames.griddrive.notifications;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

/** Delivers the single free-coins alarm. Boot reschedule is handled with the lives receiver. */
public class FreeCoinsReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null) {
            Log.i("FreeCoinNotification", "receiver fired intent=null");
            return;
        }

        String action = intent.getAction();
        Log.i("FreeCoinNotification", "receiver fired action=" + action);
        if (FreeCoinsNotifier.ACTION_SHOW.equals(action)) {
            FreeCoinsNotifier.showFromAlarm(context);
        }
    }
}
