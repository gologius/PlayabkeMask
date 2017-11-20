using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionTest : MonoBehaviour {

    public Text upperMotionNameText;
    public Text lowerMotionNameText;

    private MotionPlayer motionPlayer; //GetComponent()による自動取得
    private int upperCounter = 0;
    private int lowerCounter = 0;

    void Start()
    {
        motionPlayer = GetComponent<MotionPlayer>();
    }

    public void changeUpperMotion()
    {
        upperCounter++;
        upperCounter %= motionPlayer.clips.Count;

        playMotion();
    }

    public void changeLowerMotion()
    {
        lowerCounter++;
        lowerCounter %= motionPlayer.clips.Count;

        playMotion();
    }


    private void playMotion()
    {
        GetComponent<MotionPlayer>().playFromID(upperCounter, lowerCounter);
        
        upperMotionNameText.text = motionPlayer.clips[upperCounter].name;
        lowerMotionNameText.text = motionPlayer.clips[lowerCounter].name;
    }
}
