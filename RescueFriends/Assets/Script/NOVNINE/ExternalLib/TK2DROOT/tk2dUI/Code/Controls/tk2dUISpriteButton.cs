/**
* @file tk2dUISpriteButton.cs
* @brief
* @author amugana
* @version 1.0
* @date 2013-12-24
*/

using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/tk2dUISpriteButton")]
public class tk2dUISpriteButton : tk2dUIBaseItemControl
{
    public string buttonDownSprite;
    public string buttonUpSprite;

    [SerializeField]
    private bool useOnReleaseInsteadOfOnUp = false;

    public bool UseOnReleaseInsteadOfOnUp
    {
        get {
            return useOnReleaseInsteadOfOnUp;
        }
    }

    private bool isDown = false;
    private tk2dBaseSprite sprite;

    void Start()
    {
        sprite = gameObject.GetComponent<tk2dBaseSprite>();
        SetState();
    }

    void OnEnable()
    {
        if (uiItem) {
            uiItem.OnDown += ButtonDown;
            if (useOnReleaseInsteadOfOnUp) {
                uiItem.OnRelease += ButtonUp;
            } else {
                uiItem.OnUp += ButtonUp;
            }
        }
    }

    void OnDisable()
    {
        if (uiItem) {
            uiItem.OnDown -= ButtonDown;
            if (useOnReleaseInsteadOfOnUp) {
                uiItem.OnRelease -= ButtonUp;
            } else {
                uiItem.OnUp -= ButtonUp;
            }
        }
    }

    private void ButtonUp()
    {
        isDown = false;
        SetState();
    }

    private void ButtonDown()
    {
        isDown = true;
        SetState();
    }

    private void SetState()
    {
        if(isDown) {
            if(!string.IsNullOrEmpty(buttonDownSprite))
                sprite.spriteName = buttonDownSprite;
        } else {
            if(!string.IsNullOrEmpty(buttonUpSprite))
                sprite.spriteName = buttonUpSprite;
        }
    }

    public void InternalSetUseOnReleaseInsteadOfOnUp(bool state)
    {
        useOnReleaseInsteadOfOnUp = state;
    }
}

