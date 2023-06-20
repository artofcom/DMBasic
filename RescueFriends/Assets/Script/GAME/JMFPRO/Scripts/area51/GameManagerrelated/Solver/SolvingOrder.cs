using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SolvingOrder : MonoBehaviour {
#if !NN_DEPLOY
    public bool isLoadConfig = false;
    public enum CONFIG_LIST { DEFAULT, DUMMY, CUSTOM, COUNT };
    public int fileIndex; 
    public int retryCount = 1;
    public int timeScale = 10;


    // Special Piece
    public int scoreSpecialFive = 0;
    public int scoreBomb = 0;
    public int scoreVertical = 0;
    public int scoreHorizontal = 0;

    // Blocker Panel
    public int scoreFrost = 0;
    public int scoreSnow = 0;
    public int scoreSnowman = 0;
    public int scoreCage = 0;
    public int scoreShaded = 0;
    public int scoreCake = 0;

    // Blocker Piece
    public int scoreSpiralsnow = 0;
    public int scoreTimebomb = 0;
    public int scoreMystery = 0;
    public int scoreYetiHealth = 0;
    public int scorePenguin = 0;
    public int scoreTreasure = 0;

    // Panel Position
    public List<string> positionScoreInfo = new List<string>();

    // Level
    public class SolvingOrderSettings {
        public int specialFive;
        public int bomb;
        public int vertical;
        public int horizontal;
        public int frost;
        public int snow;
        public int snowMan;
        public int cage;
        public int shaded;
        public int cake;
        public int penguin;
        public int sprialSnow;
        public int timeBomb;
        public int mystery;
        public int yetiHealth;
        public int treasure;
        public List<string> positionScoreInfo;
    }

    public SolvingOrderSettings orderEditorSettings;
    string orderPath;

	public System.Action OnSolve;
    public string exportFileName;

    void Awake () {
    }

    public void Solve() {
		if (OnSolve != null)
			OnSolve();
    }

    public void SaveOrders () {
        orderEditorSettings.specialFive = scoreSpecialFive;
        orderEditorSettings.bomb = scoreBomb;
        orderEditorSettings.vertical = scoreVertical;
        orderEditorSettings.horizontal = scoreHorizontal;

        orderEditorSettings.frost = scoreFrost;
        orderEditorSettings.snow = scoreSnow;
        orderEditorSettings.snowMan = scoreSnowman;
        orderEditorSettings.cage = scoreCage;
        orderEditorSettings.shaded = scoreShaded;
        orderEditorSettings.cake = scoreCake;

        orderEditorSettings.sprialSnow = scoreSpiralsnow;
        orderEditorSettings.timeBomb = scoreTimebomb;
        orderEditorSettings.mystery = scoreMystery;
        orderEditorSettings.yetiHealth = scoreYetiHealth;
        orderEditorSettings.penguin = scorePenguin;
        orderEditorSettings.treasure = scoreTreasure;
        orderEditorSettings.positionScoreInfo = positionScoreInfo;

        string jsonStr = JsonFx.Json.JsonWriter.Serialize(orderEditorSettings);
        orderPath = Application.dataPath+"/Data/text/Solver/"+exportFileName+".txt";
        System.IO.File.WriteAllText(orderPath, jsonStr);
    }

    public void LoadOrders () {
        string fileName = ((CONFIG_LIST)fileIndex).ToString().ToLower();
        orderPath = Application.dataPath+"/Data/text/Solver/"+fileName+".txt";
        exportFileName = fileName;
        string jsonStr = System.IO.File.ReadAllText(orderPath);
        orderEditorSettings = JsonFx.Json.JsonReader.Deserialize<SolvingOrderSettings>(jsonStr);
        SetOrdersInspector();
    }

	public void SetOrdersInspector() {
        isLoadConfig = true;
        scoreSpecialFive = orderEditorSettings.specialFive;
        scoreBomb = orderEditorSettings.bomb;
        scoreVertical = orderEditorSettings.vertical;
        scoreHorizontal = orderEditorSettings.horizontal;

        scoreFrost = orderEditorSettings.frost;
        scoreSnow = orderEditorSettings.snow;
        scoreSnowman = orderEditorSettings.snowMan;
        scoreCage = orderEditorSettings.cage;
        scoreShaded = orderEditorSettings.shaded;
        scoreCake = orderEditorSettings.cake;

        scoreSpiralsnow = orderEditorSettings.sprialSnow;
        scoreTimebomb = orderEditorSettings.timeBomb;
        scoreMystery = orderEditorSettings.mystery;
        scoreYetiHealth = orderEditorSettings.yetiHealth;
        scorePenguin = orderEditorSettings.penguin;
        scoreTreasure = orderEditorSettings.treasure;
        positionScoreInfo = orderEditorSettings.positionScoreInfo;
    }
#endif
}
