using UnityEngine;
using System.Collections;

public class InputManager : SingletonMonoBehaviour<InputManager>
{
	public static System.Action<Vector3> onMouseDown;
	public static System.Action<Vector3> onMouseUp;
	public static System.Action<Vector3> onMousePress;
	public static System.Action<Vector3> onMouseDrag;
	
	public static System.Action onEscape;
	public static float timeEscapeInterval = 0.6f;
	
	bool buttonDown = false;
	Vector3 mousePosition = Vector3.zero;
	
	// Update is called once per frame
	void Update()
	{
		if (Time.timeScale <= 0)
			return;

		UpdateMouseProcess();
		UpdateKeyProcess();
	}
	
	void UpdateMouseProcess()
	{
		#if UNITY_EDITOR
		#else
		if (Input.touchCount <= 0) return;
		#endif
		Vector3 prevPosition = this.mousePosition;
		
		if (Input.GetMouseButtonDown(0) && buttonDown == false) { //drag start
			buttonDown = true;
			
			#if UNITY_EDITOR
			mousePosition = Input.mousePosition;
			#else
			mousePosition = Input.GetTouch(0).position;
			#endif
			
			if (null != onMouseDown) onMouseDown(mousePosition);
		}
		
		if (Input.GetMouseButtonUp(0) && buttonDown == true) { //if drag is done
			buttonDown = false;
			
			#if UNITY_EDITOR
			mousePosition = Input.mousePosition;
			#else
			mousePosition = Input.GetTouch(0).position;
			#endif
			
			if (null != onMouseUp) onMouseUp(mousePosition);
		}
		
		if (Input.GetMouseButton(0) && buttonDown == true) {
			if (null != onMousePress) onMousePress(mousePosition);
			
			#if UNITY_EDITOR
			mousePosition = Input.mousePosition;
			#else
			mousePosition = Input.GetTouch(0).position;
			#endif
						
			if (prevPosition != Input.mousePosition) {
				if (null != onMouseDrag) onMouseDrag(mousePosition);
			}
		}
	}
	
	void UpdateKeyProcess()
	{
		if (true == CheckAndroidEscapeKeyState()) {
			ResetAndroidEscapeKeyState();
			if (null != onEscape) onEscape();
		}
	}
	
	float timeEscape = 0f;
	bool CheckAndroidEscapeKeyState()
	{
		if (Application.platform == RuntimePlatform.Android || true == Application.isEditor) {
			if (Input.GetKeyUp(KeyCode.Escape) && (Time.time - timeEscape) > timeEscapeInterval) {
				return true;
			}
		}
		
		return false;
	}
	
	void ResetAndroidEscapeKeyState()
	{
		timeEscape = Time.time; // Input Delay..
	}
}

