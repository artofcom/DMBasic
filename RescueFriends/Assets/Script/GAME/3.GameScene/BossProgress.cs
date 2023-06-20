using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using NOVNINE;

public class BossProgress : MonoBehaviour {
    public GameObject background;
    public GameObject foreground;

    public tk2dSprite progressStart;
    public tk2dSlicedSprite progressMiddle;
    public tk2dSprite progressEnd;

    void OnEnable () {
        background.SetActive(true);
        foreground.SetActive(true);
        JMFRelay.OnChangeBossHealth += OnChangeBossHealth;
    }

    public void OnLeaveScene() {
        JMFRelay.OnChangeBossHealth -= OnChangeBossHealth;
    }

    void OnGameReady () {
        progressStart.gameObject.SetActive(true);
        progressMiddle.dimensions = new Vector2(48F, 8F);
        progressEnd.gameObject.SetActive(true);
    }

    void OnChangeBossHealth (int health, int damage) {
        if (health >= JMFUtils.GM.CurrentLevel.bossHealth) {
            progressEnd.gameObject.SetActive(true);
        } else {
            progressEnd.gameObject.SetActive(false);
        }

        if (health <= 0) {
            progressStart.gameObject.SetActive(false);
        } else {
            progressStart.gameObject.SetActive(true);
        }

        float per = ((health * 1F) / JMFUtils.GM.CurrentLevel.bossHealth);
        float newX = 50 * per;
        if (newX <= 0) {
            background.SetActive(false);
            foreground.SetActive(false);
            newX = 0;
        }
        progressMiddle.dimensions = new Vector2(newX, 8F);
    }



}
