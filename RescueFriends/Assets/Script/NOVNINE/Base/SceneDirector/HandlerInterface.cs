using UnityEngine;
using System.Collections;

public interface IHandlerBase
{
	void DoDataExchange();
}

public interface IDirector : IHandlerBase
{

}

public interface IGameScene : IHandlerBase
{
    void OnEnterScene(object param);
    void OnLeaveScene();
	GameObject GetGameObject();
	void OnEscape();
}

public interface IUIOverlay : IHandlerBase
{
    void OnEnterUIOveray(object param);
    void OnLeaveUIOveray();
	GameObject GetGameObject();
}

public interface IPopupWnd : IHandlerBase
{
    void OnEnterPopup(object param);
    void OnLeavePopup();
}

public interface IWidget : IHandlerBase
{

}
