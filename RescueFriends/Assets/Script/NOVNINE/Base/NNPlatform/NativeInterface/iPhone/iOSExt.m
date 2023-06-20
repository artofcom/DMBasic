typedef void(*showShareCallback)(const bool *);
#import <MediaPlayer/MediaPlayer.h>
#import <Foundation/NSUserDefaults.h>
#include "UnityAppController.h"
#import <StoreKit/StoreKit.h>
#import <AdSupport/ASIdentifierManager.h>

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
#define SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(v)  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedAscending)

char* MakeStringCopy (const char* string)
{
  if (string == NULL) return NULL;
  char* res = (char*)malloc(strlen(string) + 1);
  strcpy(res, string);
  return res;
}

const char* _getAdvertisingIdentifier()
{
    NSString* idfaString = [[[ASIdentifierManager sharedManager] advertisingIdentifier]UUIDString];
    return MakeStringCopy([idfaString UTF8String]);
}

const char* _GetISO3CountryCode()
{
    NSLocale *locale =[NSLocale currentLocale];
    NSString *countryCode = [locale objectForKey:NSLocaleCountryCode];
    return MakeStringCopy([countryCode UTF8String]);
}

bool _IsMusicPlaying () 
{
    BOOL isPlaying = NO;
    MPMusicPlayerController* iPodMusicPlayer = [MPMusicPlayerController iPodMusicPlayer];
    if (iPodMusicPlayer.playbackState == MPMusicPlaybackStatePlaying) {
        isPlaying = YES;
    }
    NSLog(@"Music is %@.", isPlaying ? @"on" : @"off");
    return isPlaying;
}

//returns ISO-3166 country codes
const char* _iOSGetCurrentLocaleID ()
{
    /*NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSArray *languages = [defaults objectForKey:@"AppleLanguages"];
    NSString *currentLanguage = [languages objectAtIndex:0];
    return MakeStringCopy([currentLanguage UTF8String]);
	*/
	NSLocale *locale = [NSLocale currentLocale];
	NSString *country = [locale objectForKey:NSLocaleCountryCode];
	if (country != nil)
    	return MakeStringCopy([country UTF8String]);
	else
    	return "US";
}

const char* _iOSGetBundleIdentifier ()
{
    NSString *bundleID = [[NSBundle mainBundle] bundleIdentifier];
    return MakeStringCopy([bundleID UTF8String]);
}

const char* _iOSGetBundleVersion ()
{
    NSString *version = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleVersion"];
    return MakeStringCopy([version UTF8String]);
}

bool _iOSCanOpenURL( const char * _url )
{
    NSString* URL = GetStringParam(_url);
    return [[UIApplication sharedApplication] canOpenURL:[NSURL URLWithString:URL]];
}

void _showShare(const char* _title, const char* _message, const char* _imgPath, showShareCallback callback) {
    NSString* title = GetStringParam(_title);
    NSString* message = GetStringParam(_message);
    NSString* imgPath = GetStringParam(_imgPath);
    NSArray *pair = [message componentsSeparatedByString:@" "];
    NSMutableString *msgBuf = [[NSMutableString alloc] init];
    NSURL* url = nil;
    
    UIImage *image  = nil;
    if(imgPath != nil) {
        NSArray  *documentPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        NSString *documentsDir  = [documentPaths objectAtIndex:0];
        NSString *pngfile = [NSString stringWithFormat:@"%@/%@", documentsDir, imgPath];
        image = [UIImage imageWithContentsOfFile:pngfile];
    }
    for (NSString *tok in pair) {
        if ([tok hasPrefix:@"http"]) {
            url = [NSURL URLWithString:tok];
        } else {
            [msgBuf appendFormat:@" %@", tok];
        }
    }
    NSArray *activityItems = nil;
    if(url == nil && image == nil) {
        activityItems = @[msgBuf];
    } else if(url == nil) { 
        activityItems = @[msgBuf, image];
    } else if(image == nil) {
        activityItems = @[msgBuf, url];
    } else {
        activityItems = @[msgBuf, url, image];
    }
    
    UIActivityViewController *activityVC = [[UIActivityViewController alloc]initWithActivityItems:activityItems applicationActivities:Nil];
    [activityVC setValue:title forKey:@"subject"];
    [activityVC setCompletionHandler:^(NSString *act, BOOL done) {
        NSLog(@"showShare done : %@ : %d", act, (int)done);
        
        if(callback != nil) callback(done);
    }];
    UnityAppController* appcon = (UnityAppController*)([UIApplication sharedApplication].delegate);
    if(SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"8.0")){
        UIView* view = appcon.rootView;
        activityVC.popoverPresentationController.sourceView = view;
        activityVC.popoverPresentationController.permittedArrowDirections = 0;
        activityVC.popoverPresentationController.sourceRect = CGRectMake(view.frame.size.width/2, view.frame.size.height, 0, 0);
    }
    [appcon.rootViewController presentViewController:activityVC animated:YES completion:nil];
}

void _NSLog(const char* _log) {
    NSString* llog = GetStringParam(_log);
    NSLog(llog);
}

