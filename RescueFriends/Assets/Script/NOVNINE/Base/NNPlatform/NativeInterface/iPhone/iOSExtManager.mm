typedef void(*alertCallBack)(const char *);
#import <StoreKit/StoreKit.h>
@interface iOSExtManager : NSObject <UIAlertViewDelegate, SKStoreProductViewControllerDelegate> {
    uint8_t *buffer;
    alertCallBack handler;

};
+ (iOSExtManager*)sharedManager;
@property (assign)alertCallBack handler;
@property (assign)uint8_t *buffer;

-(void)openStore:(int)identifier;
//- (void)showAlert:(NSString*)title message:(NSString*)message positive:(NSString*)positive negative:(NSString*)negative buffer:(uint8_t*)buffer callback:(alertCallBack)callback;
@end

@implementation iOSExtManager

@synthesize buffer;
@synthesize handler;

#pragma mark UIAlertViewDelegate
- (void)alertView:(UIAlertView*)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    UnityPause( 0 );

    // always dump the button clicked
    NSString *title = [alertView buttonTitleAtIndex:buttonIndex];
    handler([title UTF8String]);
}

-(void)openStore:(int)identifier {
    if ([SKStoreProductViewController class]) {
        SKStoreProductViewController *controller = [[SKStoreProductViewController alloc] init];
        controller.delegate = self;
        NSNumber *number = [NSNumber numberWithInt:identifier];
        [controller loadProductWithParameters:@{ SKStoreProductParameterITunesItemIdentifier:number}
                              completionBlock:NULL];
        id rootVC = [[[[[UIApplication sharedApplication] keyWindow] subviews] objectAtIndex:0] nextResponder];
        if(rootVC != nil) {
            [rootVC presentViewController:controller animated:TRUE completion:nil];
        }
        return;
    } else {
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:[NSString stringWithFormat:@"http://itunes.apple.com/app/id%d",identifier]]];
    }
}

- (void)productViewControllerDidFinish:(SKStoreProductViewController *)viewController
{
    if (viewController)
    { [viewController dismissViewControllerAnimated:YES completion:nil]; }
}

#pragma mark NSObject

+ (iOSExtManager*)sharedManager
{
    static iOSExtManager *sharedManager = nil;
    
    if( !sharedManager )
        sharedManager = [[iOSExtManager alloc] init];
    
    return sharedManager;
}


- (id)init
{
    if( ( self = [super init] ) )
    {
        //        _JPEGCompression = 0.8;
        //        _pickerAllowsEditing = NO;
        //        _scaledImageSize = 1.0f;
        //        popoverRect = CGRectMake( 20, 15, 10, 0 );
    }
    return self;
}

@end
// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

static iOSExtManager* iOSEManager = NULL;
extern "C" {
    void _showAlert(const char* _title,const char* _message, const char* _positive, const char* _negative, uint8_t buffer,alertCallBack callback)
    {
        if(iOSEManager == NULL) iOSEManager = [[iOSExtManager alloc] init];
        UnityPause( 1 );
        //UIAlertView *alert = [[[UIAlertView alloc] init] autorelease];
        UIAlertView *alert = [[UIAlertView alloc] init];
        NSString *title = GetStringParam(_title);
        NSString *message = GetStringParam(_message);
        NSString *positive = GetStringParam(_positive);
        NSString *negative = GetStringParam(_negative);
        
        alert.delegate = iOSEManager;
        alert.title = title;
        alert.message = message;
        if(callback != nil) iOSEManager.handler = callback;
        [alert addButtonWithTitle:positive];
        if(negative != nil && negative.length > 0) [alert addButtonWithTitle:negative];
    
        alert.tag = 1111;
        [alert show];
    }

    void _openStore(int identifier) {
        [[iOSExtManager sharedManager] openStore:identifier];
    }
}

