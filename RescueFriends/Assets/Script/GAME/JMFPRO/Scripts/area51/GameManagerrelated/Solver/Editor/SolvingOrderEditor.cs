#if !NN_DEPLOY
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SolvingOrder))]
public class SolvingOrderEditor : Editor {
    enum CONFIG_LIST { DEFAULT, DUMMY, CUSTOM, COUNT };

    public override void OnInspectorGUI() {
        SolvingOrder _target = (SolvingOrder)target;
        DrawSelectBox(_target);
        DrawSpeicalOrder(_target);
        DrawOrder(_target);
        DrawPositionScore(_target);
        DrawButton(_target);
    }

    void DrawSelectBox(SolvingOrder _target) {
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.Space();

        string[] fileList = new string[(int)CONFIG_LIST.COUNT];
        for (int i = 0; i < fileList.Length; i++) {
            fileList[i] = ((CONFIG_LIST)i).ToString();
        }

        _target.fileIndex = EditorGUILayout.Popup("Selected Solver", _target.fileIndex, fileList);

        EditorGUILayout.Space();

        _target.retryCount = EditorGUILayout.IntField("Retry Count", _target.retryCount);
        _target.timeScale = EditorGUILayout.IntField("Time Scale", _target.timeScale);
        EditorGUILayout.Space();
        if (GUILayout.Button("Run Solver", EditorStyles.miniButton, GUILayout.ExpandWidth(true))) {
            _target.Solve();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Load Config", EditorStyles.miniButton, GUILayout.ExpandWidth(true))) {
            _target.LoadOrders();
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    void DrawSpeicalOrder(SolvingOrder _target) {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.PrefixLabel("Special Pieces");
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        _target.scoreSpecialFive = EditorGUILayout.IntSlider("Special Five", _target.scoreSpecialFive, 0, 99);
        _target.scoreBomb = EditorGUILayout.IntSlider("Bomb", _target.scoreBomb, 0, 99);
        _target.scoreVertical = EditorGUILayout.IntSlider("Vertical", _target.scoreVertical, 0, 99);
        _target.scoreHorizontal = EditorGUILayout.IntSlider("Horizontal", _target.scoreHorizontal, 0, 99);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    void DrawOrder(SolvingOrder _target) {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.PrefixLabel("Blockers");
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        _target.scoreFrost = EditorGUILayout.IntSlider("Frost", _target.scoreFrost, 0, 99);
        _target.scoreSnow = EditorGUILayout.IntSlider("Snow", _target.scoreSnow, 0, 99);
        _target.scoreSnowman = EditorGUILayout.IntSlider("Snow Man", _target.scoreSnowman, 0, 99);
        _target.scoreCage = EditorGUILayout.IntSlider("Cage", _target.scoreCage, 0, 99);
        _target.scoreShaded = EditorGUILayout.IntSlider("Shaded", _target.scoreShaded, 0, 99);
        _target.scorePenguin = EditorGUILayout.IntSlider("Penguin", _target.scorePenguin, 0, 99);
        _target.scoreCake = EditorGUILayout.IntSlider("Cake", _target.scoreCake, 0, 99);
        _target.scoreSpiralsnow = EditorGUILayout.IntSlider("Spiral Snow", _target.scoreSpiralsnow, 0, 99);
        _target.scoreTimebomb = EditorGUILayout.IntSlider("Time Bomb", _target.scoreTimebomb, 0, 99);
        _target.scoreMystery = EditorGUILayout.IntSlider("Mystery", _target.scoreMystery, 0, 99);
        _target.scoreYetiHealth = EditorGUILayout.IntSlider("Yeti Health", _target.scoreYetiHealth, 0, 99);
        _target.scoreTreasure = EditorGUILayout.IntSlider("Treasure Goal Distance", _target.scoreTreasure, 0, 99);

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

    }

    void DrawPositionScore(SolvingOrder _target) {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.PrefixLabel("PositionScore");
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        if (GUILayout.Button("Add Position ", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
            _target.positionScoreInfo.Add("");
        }
        EditorGUILayout.LabelField("example)", "x,y:object:score");
        EditorGUILayout.Space();
        for (int i = 0; i < _target.positionScoreInfo.Count; i++) {
            _target.positionScoreInfo[i] = EditorGUILayout.TextField("position_"+i.ToString(), _target.positionScoreInfo[i]);
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    void DrawButton(SolvingOrder _target) {
        _target.exportFileName = EditorGUILayout.TextField("Save File Name", _target.exportFileName);
        EditorGUILayout.Space();

        if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.ExpandWidth(true))) {
            _target.SaveOrders();
        }
        EditorGUILayout.Space();
    }

}
#endif
