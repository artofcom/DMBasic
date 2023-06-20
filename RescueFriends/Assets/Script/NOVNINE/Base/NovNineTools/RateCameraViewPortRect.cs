using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class RateCameraViewPortRect : MonoBehaviour
{

    public void UpdateResolution(float widthRate, float heightRate)
    {
        float fResolutionX = Screen.width / widthRate;
        float fResolutionY = Screen.height / heightRate;

        int width = Screen.width;
        int height = Screen.height;

        if(fResolutionX > fResolutionY) {
            if (false == Application.isEditor && width < height)
                width = height;

            float fValue = (fResolutionX - fResolutionY) * 0.5f;
            fValue = fValue / fResolutionX ;

            GetComponent<Camera>().rect = new Rect( width * fValue / width + GetComponent<Camera>().rect.x * (1.0f - 2.0f * fValue), GetComponent<Camera>().rect.y, GetComponent<Camera>().rect.width * (1.0f - 2.0f * fValue)   , GetComponent<Camera>().rect.height );

        } else if(fResolutionX < fResolutionY) {
            if (false == Application.isEditor && height < width)
                height = width;

            float fValue = (fResolutionY - fResolutionX ) * 0.5f;
            fValue = fValue / fResolutionY ;

            GetComponent<Camera>().rect = new Rect( GetComponent<Camera>().rect.x, height * fValue / height + GetComponent<Camera>().rect.y * (1.0f - 2.0f * fValue), GetComponent<Camera>().rect.width , GetComponent<Camera>().rect.height * (1.0f - 2.0f * fValue));
        } else {
            // Do Not Setting Camera
        }
    }

}

