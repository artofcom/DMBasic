using UnityEngine;
using System.Collections;
using NOVNINE.Diagnostics;

[RequireComponent(typeof(Camera))]
public class RateCameraOrthgraphicSize : MonoBehaviour
{
    float orthographicSize = 0f;
	
    public void UpdateResolution(float widthRate, float heightRate)
    {
		if (orthographicSize == 0f) 
			orthographicSize = GetComponent<Camera>().orthographicSize;
        
		float aspect = widthRate / heightRate;
        if (GetComponent<Camera>().aspect >= aspect)
            return;

        Debugger.Assert(orthographicSize != 0f);
        GetComponent<Camera>().orthographicSize = orthographicSize * aspect / GetComponent<Camera>().aspect;
    }
}

