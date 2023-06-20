#if UNITY_ANDROID
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using Prime31;

public class NativeInterfaceManager : AndroidJavaProxy 
{

    public System.Action<string> onAlertButtonClicked = null;
    public System.Action onAlertButtonCanceled = null;

	public System.Action<bool> onShareClosed = null;

    public System.Action<string> onTakePhotoSucceeded = null;
    public System.Action onTakePhotoCanceled = null;

	public System.Action<string> onSelectAlbumSucceeded = null;
	public System.Action onSelectAlbumCanceled = null;

    public NativeInterfaceManager() : base("com.bitmango.bitmangoext.NativeInterfaceListener") {
    }

    public void alertButtonClicked( string positiveButton )
    {
        if(onAlertButtonClicked != null )
            onAlertButtonClicked(positiveButton );
    }

    public void alertButtonCanceled()
    {
        if( onAlertButtonCanceled != null )
            onAlertButtonCanceled();
    }

	public void showShareClosed(bool isCanceled)
	{
		if(onShareClosed != null)
			onShareClosed(isCanceled);
	}

    public void takePhotoCanceled()
    {
        if( onTakePhotoCanceled != null )
            onTakePhotoCanceled();
    }

    public void takePhotoSucceeded(string filePath)
    {
        if( onTakePhotoSucceeded != null )
            onTakePhotoSucceeded(filePath);
    }

	public void selectAlbumCanceled()
	{
		if( onSelectAlbumCanceled != null )
			onSelectAlbumCanceled();
	}

	public void selectAlbumSucceeded(string filePath)
	{
		if( onSelectAlbumSucceeded != null )
			onSelectAlbumSucceeded(filePath);
	}
/*
    #region Remote Notifications

    public void remoteRegistrationDidSucceed( string deviceToken )
    {
        Debug.Log("remoteRegistrationDidSucceed");
        if( remoteRegistrationSucceeded != null )
            remoteRegistrationSucceeded( deviceToken );
    }

    public void remoteRegistrationDidFail( string error )
    {
        Debug.Log("remoteRegistrationDidFail");
        if( remoteRegistrationFailed != null )
            remoteRegistrationFailed( error );
    }

    public void remoteUnRegistrationDidSucceed( string dummy )
    {
        Debug.Log("remoteUnRegistrationDidSucceed");
        //if( remoteRegistrationSucceeded != null )
        //	remoteRegistrationSucceeded( deviceToken );
    }

    public void remoteNotificationWasReceived( string json )
    {
        Debug.Log("remoteNotificationReceived");
        if( remoteNotificationReceived != null )
            remoteNotificationReceived( json );//json.dictionaryFromJson() );
    }

    #endregion;
*/	
}
#endif

