using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class EditItem : MonoBehaviour, IPointerClickHandler {
    public ToggleGroup subPanel;
    public Image[] subImages;

    bool isMouseDown;
    float holdTime;
    Toggle toggle;

    void Awake () {
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerClick (PointerEventData data) {
        if (data.button == PointerEventData.InputButton.Left) {
            if ((subPanel != null) && subPanel.gameObject.activeSelf) {
                subPanel.gameObject.SetActive(false);
            }
        } else if (data.button == PointerEventData.InputButton.Right) {
            toggle.isOn = true;

            if (subPanel != null) {
                subPanel.gameObject.SetActive(!subPanel.gameObject.activeSelf);
            }
        }
    }

    public void OnSelected () {
        if (toggle.isOn) {
        } else {
            if (subPanel != null) subPanel.gameObject.SetActive(false);
        }
    }

    public void OnSubSelected (int index) {
        toggle.image.sprite = subImages[index].sprite;
        toggle.image.color = subImages[index].color;
        toggle.image.transform.rotation = subImages[index].transform.rotation;
        if (subPanel != null) subPanel.gameObject.SetActive(false);
    }
}
