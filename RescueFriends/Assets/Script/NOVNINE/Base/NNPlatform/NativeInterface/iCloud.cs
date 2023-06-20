/**
 * @file iCloud.cs
 * @brief native iCloud Interface
 * @author Choi YongWu(amugana@bitmango.com)
 * @version 1.0
 * @date 2013-07-05
 */
#if UNITY_IPHONE
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class iCloud
{

    [DllImport("__Internal")]
    private static extern bool _iCloudIsAvailable();

    [DllImport("__Internal")]
    private static extern bool _iCloudSynchronize();

    [DllImport("__Internal")]
    private static extern void _iCloudRemoveObjectForKey( string aKey );

    [DllImport("__Internal")]
    private static extern bool _iCloudHasKey( string key );

    [DllImport("__Internal")]
    private static extern string _iCloudStringForKey( string key );

    [DllImport("__Internal")]
    private static extern string _iCloudAllKeys();

    [DllImport("__Internal")]
    private static extern void _iCloudRemoveAll();

    [DllImport("__Internal")]
    private static extern void _iCloudSetString( string aString, string aKey );

    [DllImport("__Internal")]
    private static extern int _iCloudIntForKey( string aKey );

    [DllImport("__Internal")]
    private static extern void _iCloudSetInt( int value, string aKey );

    [DllImport("__Internal")]
    private static extern bool _iCloudBoolForKey( string aKey );

    [DllImport("__Internal")]
    private static extern void _iCloudSetBool( bool value, string aKey );

    // Checks to see if iCloud is available on the current device
    public static bool IsAvailable()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudIsAvailable();
        return false;
    }

    // Synchronizes the values to disk. This gets called automatically at launch and at shutdown so you should rarely ever need to call it.
    public static bool Synchronize()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudSynchronize();
        return false;
    }

    // Removes the object from iCloud storage
    public static void RemoveObjectForKey( string aKey )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _iCloudRemoveObjectForKey(aKey);
    }

    // Checks to see if the given key exists
    public static bool HasKey( string key )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudHasKey(key);
        return false;
    }

    // Gets the string value for the key
    public static string StringForKey( string key )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudStringForKey(key);
        return string.Empty;
    }

    // Gets all the keys currently stored in iCloud
    public static List<object> AllKeys()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer ) {
            var keys = _iCloudAllKeys();
            return keys.listFromJson<object>();
        }
        return new List<object>();
    }

    // Removes all data stored in the key/value store
    public static void RemoveAll()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _iCloudRemoveAll();
    }

    // Sets the string value for the key
    public static void SetString( string aKey, string aString )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _iCloudSetString(aString, aKey);
    }

    // Gets the int value for the key
    public static int IntForKey( string aKey )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudIntForKey(aKey);
        return 0;
    }

    // Sets the int value for the key
    public static void SetInt( string aKey, int value)
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _iCloudSetInt(value, aKey);
    }

    // Gets the bool value for the key
    public static bool BoolForKey( string aKey )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return _iCloudBoolForKey(aKey);
        return false;
    }

    // Sets the string value for the key
    public static void SetBool( string aKey, bool value )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _iCloudSetBool(value, aKey);
    }
}

#endif

