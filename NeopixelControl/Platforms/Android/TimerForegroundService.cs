using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Neopixel.Client;

namespace NeopixelControl;

[Service]
public class TimerForegroundService : Service
{
    private const string NotificationChannelId = "1000";
    private const int NotificationId = 1;
    private const string NotificationChannelName = "notification";

    public double Seconds { get; set; }

    private void startForegroundService()
    {
        var notifcationManager = GetSystemService(Context.NotificationService) as NotificationManager;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            createNotificationChannel(notifcationManager);
        }

        var notification = new NotificationCompat.Builder(this, NotificationChannelId);
        notification.SetAutoCancel(false);
        notification.SetOngoing(true);
        notification.SetSilent(true);
        notification.SetSmallIcon(Resource.Mipmap.appicon);
        notification.SetContentTitle($"NeopixelTimer");
        notification.SetContentText("A timer is currently displayed on your Neopixel stripe.");
        notification.MActions.Add(new NotificationCompat.Action(Resource.Mipmap.appicon, "Stop", PendingIntent.GetBroadcast(this, 0, new("StopTimer"), PendingIntentFlags.UpdateCurrent)));
        StartForeground(NotificationId, notification.Build());

        NeopixelClient neopixelClient = new("192.168.1.157");

        double secondsPerPixel = Seconds / neopixelClient.Stripe.PixelCount;

        neopixelClient.Fill(System.Drawing.Color.Black);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < neopixelClient.Stripe.PixelCount; i++)
        {
            sw.Restart();
            neopixelClient.Stripe[i] = System.Drawing.Color.Red;

            Task.Delay((int) (secondsPerPixel * 1000) - sw.Elapsed.Seconds).Wait();

            // Update the notification
            notification.SetProgress((int) Seconds, (int) (i * secondsPerPixel), false);
            StartForeground(NotificationId, notification.Build());
        }

        neopixelClient.Fill(System.Drawing.Color.Lime);

        StopForeground(true);
    }

    private void createNotificationChannel(NotificationManager notificationMnaManager)
    {
        NotificationChannel channel = new(NotificationChannelId, NotificationChannelName,
            NotificationImportance.Low);
        notificationMnaManager.CreateNotificationChannel(channel);
    }

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }


    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        Seconds = intent.GetDoubleExtra("seconds", 0);
        startForegroundService();
        return StartCommandResult.NotSticky;
    }
}