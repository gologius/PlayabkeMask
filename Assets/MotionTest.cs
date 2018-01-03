using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionTest : MonoBehaviour
{

    public Text upperMotionNameText;
    public Text lowerMotionNameText;

    public List<AnimationClip> clips = new List<AnimationClip>();

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
        upperCounter %= clips.Count;

        playMotion();
    }

    public void changeLowerMotion()
    {
        lowerCounter++;
        lowerCounter %= clips.Count;

        playMotion();
    }

    private void playMotion()
    {
        motionPlayer.play(clips[upperCounter], 1, loop: false);
        motionPlayer.play(clips[lowerCounter], 0, loop: false);

        upperMotionNameText.text = clips[upperCounter].name;
        lowerMotionNameText.text = clips[lowerCounter].name;
    }
}
