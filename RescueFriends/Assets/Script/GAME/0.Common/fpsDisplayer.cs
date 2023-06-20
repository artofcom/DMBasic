using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//[RequireComponent(typeof(Text))]
public class fpsDisplayer : MonoBehaviour {

    float _fElTime              = .0f;
    int _cntFrame               = 0;
    float _fCurFPS              = .0f;

    Text _txtUI                 = null;
    TextMesh _txtMesh2          = null;
    string _strDevMode;

    void Awake()
    {
        // release version 일때 dev_move 가 아니면 de-activate.
#if !UNITY_EDITOR && LIVE_MODE
        gameObject.SetActive(false);
#endif
    }
	// Use this for initialization
	void Start () {
        _txtUI                  = GetComponent<Text>();
        _txtMesh2               = GetComponent<TextMesh>();

        StartCoroutine( _coUpdateDevText() );
	}
	
	// Update is called once per frame
	void Update () {
	
        ++_cntFrame;
        _fElTime += Time.deltaTime;

        if(_fElTime >= 1.0f)
        {
            _fCurFPS            = ((float)_cntFrame) / _fElTime;

            _flushText();

            _fElTime            = .0f;
            _cntFrame           = 0;
        }
	}

    void _flushText()
    {
        if(null!=_txtUI)        _txtUI.text     = _makeInfoStr(); 
        else if(null!=_txtMesh2)_txtMesh2.text  = _makeInfoStr();
    }

    string _makeInfoStr()
    {
        string strPumpName      = "";   // (null!=JMFUtils.GM && null!=JMFUtils.GM.Pump) ? JMFUtils.GM.Pump.State.ToString() : "";
        string strMapScroll     =  "[S-OFF]";//WorldSceneHandler.AUTO_SCROLL ? "[S-ON]" : "[S-OFF]";

        //if(null!=Root.Data && null!=Root.Data.currentLevel)
        //    return string.Format("{1} [{2}] fps : {0} - LV:{3} {4} {5}", ((int)_fCurFPS), _strDevMode, Root.GetPostfix(), Root.Data.currentLevel.Index+1, strPumpName, strMapScroll);

        return string.Format("{1} [{2}] fps : {0} {3} {4}", ((int)_fCurFPS), _strDevMode, Root.GetPostfix(), strPumpName, strMapScroll);
    }

    IEnumerator _coUpdateDevText()
    {
#if DEV_MODE
        while(true)
        {
            yield return new WaitForSeconds(1.0f);
            _strDevMode         = "DEV MODE";
            
            yield return new WaitForSeconds(1.0f);
            _strDevMode         = "";
        };
#else
        yield break;
#endif
    }
}
