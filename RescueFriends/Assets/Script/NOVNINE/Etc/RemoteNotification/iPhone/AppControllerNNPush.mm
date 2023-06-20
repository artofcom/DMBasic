#import "UnityAppController.h"
#import "AppControllerNNPush.h"

void UnitySendMessage( const char * className, const char * methodName, const char * param );
@implementation UnityAppController(NNPush)


- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    NSString *devToken = [[[[deviceToken description] stringByReplacingOccurrencesOfString:@"<" withString:@""] stringByReplacingOccurrencesOfString:@">" withString:@""] stringByReplacingOccurrencesOfString:@" " withString:@""];
    UnitySendMessage( "__RemoteNotification__", "OnRegistered", [devToken UTF8String] );
    
}
- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    NSLog(@"RemoteNotification fail : %@", error);
    UnitySendMessage( "__RemoteNotification__", "OnUnregistered", [[error localizedDescription]  UTF8String] );
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo
{
    NSLog(@"Remote Notification didReceiveRemoteNotification: %@", [userInfo description]);
    UnitySendMessage( "__RemoteNotification__", "OnMessage", [ [userInfo objectForKey:@"aps"]  UTF8String] );
    UnitySendMessage( "__RemoteNotification__", "OnMessage", [[userInfo description]  UTF8String] );
}

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
    [[UIApplication sharedApplication] cancelLocalNotification:notification];
    
    if( 0 < application.applicationIconBadgeNumber )
        application.applicationIconBadgeNumber--;
    
    if(application.applicationState == UIApplicationStateActive)
    {
        
        // Foreground에서 알림 수신
    }
    
    if(application.applicationState == UIApplicationStateInactive)
    {
        
        // Background에서 알림 액션에 의한 수신
        // notification.userInfo 이용하여 처리
    }
}


- (void)application:(UIApplication *)application didRegisterUserNotificationSettings:(UIUserNotificationSettings *)settings { NSLog(@"Registering device for push notifications...iOS 8"); // iOS 8
    [application registerForRemoteNotifications];
    
    NSString *iOSversion = [[UIDevice currentDevice] systemVersion];
    NSString *prefix = [[iOSversion componentsSeparatedByString:@"."] firstObject];
    float versionVal = [prefix floatValue];
    
    if (versionVal >= 8)
    {
        if ([[UIApplication sharedApplication] currentUserNotificationSettings].types == UIUserNotificationTypeNone)
        {
            UnitySendMessage( "__RemoteNotification__", "OnUnregistered", [iOSversion UTF8String] );
        }
        //        else
        //        {
        //
        //            NSString *msg = @"Please press ON to enable Push Notification";
        //            UIAlertView *alert_push = [[UIAlertView alloc] initWithTitle:@"Push Notification Service Disable" message:msg delegate:self cancelButtonTitle:@"Cancel" otherButtonTitles:@"Setting", nil];
        //            alert_push.tag = 2;
        //            [alert_push show];
        //
        //            NSLog(@" Push Notification OFF");
        //
        //        }
        
    }
    else
    {
        UIRemoteNotificationType types = [[UIApplication sharedApplication] enabledRemoteNotificationTypes];
        if (types == UIRemoteNotificationTypeNone)
            
        {
            UnitySendMessage( "__RemoteNotification__", "OnUnregistered", [iOSversion UTF8String] );
        }
        //        else
        //        {
        //            NSString *msg = @"Please press ON to enable Push Notification";
        //            UIAlertView *alert_push = [[UIAlertView alloc] initWithTitle:@"Push Notification Service Disable" message:msg delegate:self cancelButtonTitle:@"Cancel" otherButtonTitles:@"Setting", nil];
        //            alert_push.tag = 2;
        //            [alert_push show];
        //
        //            NSLog(@" Push Notification OFF");
        //        }
        
    }
}

//- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)notification completionHandler:(void(^)())completionHandler {
//    NSLog(@"Received push notification: %@, identifier: %@", notification, identifier); // iOS 8
//    //completionHandler();
//}




@end
