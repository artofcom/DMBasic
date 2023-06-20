using UnityEngine;
using System.Collections;
using NOVNINE;

public class Conveyor : Panel {
    public Renderer belt;
    public Texture[] belts;         // center_H, center_V, cornor_LT, cornor_LD, end_U, end_D, end_L
    
    public GameObject[] portals;
    public GameObject[] lights;

    bool moving;

    public bool IsStraight { get; private set; }

    public bool IsLightIsOn {
        get { return lights[0].activeSelf; }
        set {
            foreach (GameObject light in lights) {
                light.SetActive(value);
            }
        }
    }

    // init conveyor's texture.
    public override void UpdatePanel (object _info) {
        if (_info == null) return;
        moving = false;
        IsLightIsOn = false;
        belt.material.mainTextureOffset = Vector2.zero;

        ConveyorPanel.Info info = _info as ConveyorPanel.Info;

        int indexIn = (int)info.In;     // ConveyorPanel.DIRECTION { LEFT, DOWN, RIGHT, UP }
        int indexOut = (int)info.Out;
        IsStraight = (Mathf.Abs(indexOut - indexIn) == 2);

        // [ADJUST SCALE]
        float fScale            = 1.57f;
        belt.transform.localScale       = Vector3.one * fScale;
        belt.transform.localPosition    = Vector3.forward * 7.0f;
        belt.transform.rotation         = Quaternion.Euler(0, 0, 0);

        if (IsStraight) {
            if(ConveyorPanel.DIRECTION.UP==info.In || ConveyorPanel.DIRECTION.DOWN==info.In)
            {
                if(0 == PT.PT.Y)
                    belt.material.mainTexture = belts[5];
                else if(GameManager.HEIGHT-1 == PT.PT.Y)
                    belt.material.mainTexture = belts[4];
                else 
                    belt.material.mainTexture = belts[1];       // Vertical.
            }
            else
            {
                if(0 == PT.PT.X)
                    belt.material.mainTexture = belts[6];
                else if(GameManager.WIDTH-1 == PT.PT.X)
                {
                    belt.material.mainTexture   = belts[6];
                    belt.transform.rotation     = Quaternion.Euler(0, 0, 180);
                }
                else
                    belt.material.mainTexture = belts[0];       // Horize.

                //belt.transform.rotation = Quaternion.Euler(0, 0, indexIn * 90);
            }
        } else {
            switch(info.In)
            {
            case ConveyorPanel.DIRECTION.UP:
                belt.material.mainTexture = belts[3];  
                if(ConveyorPanel.DIRECTION.LEFT == info.Out)
                    belt.transform.localScale = new Vector3(-fScale, fScale, fScale);
                break;
            case ConveyorPanel.DIRECTION.DOWN:
                belt.material.mainTexture = belts[2];  
                if(ConveyorPanel.DIRECTION.LEFT == info.Out)
                    belt.transform.localScale = new Vector3(-fScale, fScale, fScale);
                break;
            case ConveyorPanel.DIRECTION.RIGHT:
                if(ConveyorPanel.DIRECTION.UP == info.Out)
                    belt.material.mainTexture = belts[3];  
                else
                    belt.material.mainTexture = belts[2];  
                break;
            case ConveyorPanel.DIRECTION.LEFT:
                belt.transform.localScale   = new Vector3(-fScale, fScale, fScale);
                if(ConveyorPanel.DIRECTION.UP == info.Out)
                    belt.material.mainTexture = belts[3];  
                else
                    belt.material.mainTexture = belts[2];  
                break;
            default:    break;
            }

            /*
            if ((indexIn == 3) && (indexOut == 0)) {
                belt.transform.localScale = Vector3.one * fScale;
            } else if ((indexIn == 0) && (indexOut == 3)) {
                belt.transform.localScale = new Vector3(fScale, -fScale, fScale);
            } else if (indexIn < indexOut) {
                belt.transform.localScale = Vector3.one * fScale;
            } else {
                belt.transform.localScale = new Vector3(fScale, -fScale, fScale);
            }
            belt.transform.rotation = Quaternion.Euler(0, 0, indexIn * 90);
            */
        }

        for (int i = 0; i < portals.Length; i++) {
            portals[i].SetActive(false);
        }

        if (info.Type != ConveyorPanel.TYPE.MIDDLE) {
            GameObject portal = null;

            if (info.Type == ConveyorPanel.TYPE.BEGIN) {
                portal = portals[(int)info.In];
            } else {
                portal = portals[(int)info.Out];
            }

            portal.SetActive(true);
        }
    }

    public void Move (float duration = 1F) {

        // if (IsStraight) StartCoroutine(CoMove(duration));
        // 연출은 다르게 따로 처리.
    }
    
    IEnumerator CoMove (float duration) {
        if (moving) yield break;
        moving = true;

        float remainTime = duration;

        while (remainTime > 0F) {
            Vector2 newOffset = belt.material.mainTextureOffset;
            newOffset.x -= (Time.deltaTime / duration);
            belt.material.mainTextureOffset = newOffset;
            remainTime -= Time.deltaTime;
            yield return null;
        }

        belt.material.mainTextureOffset = Vector2.zero;

        moving = false;
    }
}
