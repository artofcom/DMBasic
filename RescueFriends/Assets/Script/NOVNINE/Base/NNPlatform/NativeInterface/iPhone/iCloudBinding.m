#import <Foundation/Foundation.h>
#import "JSONKit.h"

// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil

bool _iCloudIsAvailable()
{
    if(NSClassFromString(@"NSUbiquitousKeyValueStore")) { // is iOS 5?
        if([NSUbiquitousKeyValueStore defaultStore]) {  // is iCloud enabled
            return true;
        }
    }
    return false;
}

bool _iCloudSynchronize()
{
    return [[NSUbiquitousKeyValueStore defaultStore] synchronize];
}

void _iCloudRemoveObjectForKey( const char* aKey )
{
    [[NSUbiquitousKeyValueStore defaultStore] removeObjectForKey:GetStringParam(aKey)];
}

bool _iCloudHasKey( const char* key )
{
    return [[NSUbiquitousKeyValueStore defaultStore] objectForKey:GetStringParam(key)] != nil;
}

const char* _iCloudAllKeys()
{
    NSDictionary* dict = [[NSUbiquitousKeyValueStore defaultStore] dictionaryRepresentation];
    NSArray* keys = [dict allKeys];
    NSString* json = keys.JSONString;
	if(json)
		return MakeStringCopy(json);
	return MakeStringCopy(@"[]");
}

void _iCloudRemoveAll()
{
    NSUbiquitousKeyValueStore* store = [NSUbiquitousKeyValueStore defaultStore];
    NSDictionary* dict = [store dictionaryRepresentation];
    NSArray* keys = [dict allKeys];
    for(id k in keys) {
        [store removeObjectForKey:k];
    }
}


const char* _iCloudStringForKey( const char* key )
{
    NSString* val = [[NSUbiquitousKeyValueStore defaultStore] stringForKey:GetStringParam(key)];
	if(val)
		return MakeStringCopy( val );
	return MakeStringCopy( @"" );
}

void _iCloudSetString( const char* aString, const char* aKey )
{
    [[NSUbiquitousKeyValueStore defaultStore] setString:GetStringParam(aString) forKey:GetStringParam(aKey)];
}

int _iCloudIntForKey( const char* aKey )
{
    return (int)[[NSUbiquitousKeyValueStore defaultStore] longLongForKey:GetStringParam(aKey)];
}

void _iCloudSetInt( int value, const char* aKey )
{
    [[NSUbiquitousKeyValueStore defaultStore] setLongLong:(long long)value forKey:GetStringParam(aKey)];
}

bool _iCloudBoolForKey( const char* aKey )
{
    return [[NSUbiquitousKeyValueStore defaultStore] boolForKey:GetStringParam(aKey)];
}

void _iCloudSetBool( bool value, const char* aKey )
{
    [[NSUbiquitousKeyValueStore defaultStore] setBool:value forKey:GetStringParam(aKey)];
}
