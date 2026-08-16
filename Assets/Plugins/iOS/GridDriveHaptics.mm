#import <UIKit/UIKit.h>

// Minimale UIKit haptics voor Grid Drive (geen externe package).
extern "C"
{
    void GridDrive_HapticLight(void)
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackGenerator *generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [generator prepare];
            [generator impactOccurred];
        }
    }

    void GridDrive_HapticMedium(void)
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackGenerator *generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            [generator prepare];
            [generator impactOccurred];
        }
    }

    void GridDrive_HapticSuccess(void)
    {
        if (@available(iOS 10.0, *))
        {
            UINotificationFeedbackGenerator *generator =
                [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
        }
    }
}
