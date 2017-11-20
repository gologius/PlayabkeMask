using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class MotionPlayer : MonoBehaviour
{
    public List<AnimationClip> clips = new List<AnimationClip>();

    [Header("<マスク設定>")]
    [SerializeField, Tooltip("上半身にモーション適用するためのマスク")]
    private AvatarMask upperBodyMask;
    [SerializeField, Tooltip("下半身にモーション適用するためのマスク")]
    private AvatarMask lowerBodyMask;
    
    private Animator animator; //GetComponent()による自動取得
  
    private PlayableGraph graph;
    private AnimationClipPlayable beforeUpperPlayable;
    private AnimationClipPlayable beforeLowerPlayable;
    private AnimationClipPlayable nowUpperPlayable;
    private AnimationClipPlayable nowLowerPlayable;
    private AnimationLayerMixerPlayable allLayerMixer;
    private AnimationLayerMixerPlayable upperLayerMixer;
    private AnimationLayerMixerPlayable lowerLayerMixer;
    private AnimationPlayableOutput output;
  
    //============================================================================================================

    void Awake()
    {
        graph = PlayableGraph.Create();
    }

    void Start()
    {
        animator = this.GetComponent<Animator>();
       
        upperLayerMixer = AnimationLayerMixerPlayable.Create(graph, 2);
        lowerLayerMixer = AnimationLayerMixerPlayable.Create(graph, 2);
        allLayerMixer = AnimationLayerMixerPlayable.Create(graph, 2);
        upperLayerMixer.SetLayerMaskFromAvatarMask(0, upperBodyMask);
        lowerLayerMixer.SetLayerMaskFromAvatarMask(0, lowerBodyMask);
        output = AnimationPlayableOutput.Create(graph, "output", animator);
    }

    void Update()
    {
        //アニメーションが終了しているか調べる
        if (nowUpperPlayable.IsValid())
        {
            if (nowUpperPlayable.GetTime() >= nowUpperPlayable.GetAnimationClip().length)
                onFinishMotion(nowUpperPlayable);
        }

        if (nowLowerPlayable.IsValid())
        {
            if (nowLowerPlayable.GetTime() >= nowLowerPlayable.GetAnimationClip().length)
                onFinishMotion(nowLowerPlayable);
        }

    }

    void OnDestroy()
    {
        graph.Destroy();
    }

    //============================================================================================================
    //Animation切り替え
    
    public void playFromID(int upperID, int lowerID)
    {
        play(clips[upperID], clips[lowerID]);
    }

    private void play(AnimationClip upperClip, AnimationClip lowerClip)
    {
        //切断
        graph.Disconnect(upperLayerMixer, 0);
        graph.Disconnect(upperLayerMixer, 1);
        graph.Disconnect(lowerLayerMixer, 0);
        graph.Disconnect(lowerLayerMixer, 1);
        graph.Disconnect(allLayerMixer, 0);
        graph.Disconnect(allLayerMixer, 1);

        if (beforeUpperPlayable.IsValid())
            beforeUpperPlayable.Destroy();
        if (beforeLowerPlayable.IsValid())
            beforeLowerPlayable.Destroy();

        //更新
        beforeUpperPlayable = nowUpperPlayable;
        beforeLowerPlayable = nowLowerPlayable;
        nowUpperPlayable = AnimationClipPlayable.Create(graph, upperClip);
        nowLowerPlayable = AnimationClipPlayable.Create(graph, lowerClip);

        //再接続
        upperLayerMixer.ConnectInput(1, beforeUpperPlayable, 0);
        upperLayerMixer.ConnectInput(0, nowUpperPlayable, 0);
        lowerLayerMixer.ConnectInput(1, beforeLowerPlayable, 0);
        lowerLayerMixer.ConnectInput(0, nowLowerPlayable, 0);

        allLayerMixer.ConnectInput(0, upperLayerMixer, 0);
        allLayerMixer.ConnectInput(1, lowerLayerMixer, 0);

        output.SetSourcePlayable(allLayerMixer);

        //再生
        graph.Play();
        StartCoroutine(fadeCoroutine(0.2f));
    }

    /// <summary>
    /// AnimatorのCrossFade()のようなもの
    /// 参考
    /// http://tsubakit1.hateblo.jp/entry/2017/07/30/032008
    /// </summary>
    /// <param name="duration">次のAnimationに「完全に」遷移する時間</param>
    /// <returns></returns>
    IEnumerator fadeCoroutine(float duration)
    {
        // 指定時間でアニメーションをブレンド
        float waitTime = Time.timeSinceLevelLoad + duration;
        yield return new WaitWhile(() =>
        {
            var diff = waitTime - Time.timeSinceLevelLoad;
            if (diff <= 0)
            {
                upperLayerMixer.SetInputWeight(1, 0);
                upperLayerMixer.SetInputWeight(0, 1);
                lowerLayerMixer.SetInputWeight(1, 0);
                lowerLayerMixer.SetInputWeight(0, 1);
                return false;
            }
            else
            {
                var rate = Mathf.Clamp01(diff / duration);
                upperLayerMixer.SetInputWeight(1, rate);
                upperLayerMixer.SetInputWeight(0, 1 - rate);
                lowerLayerMixer.SetInputWeight(1, rate);
                lowerLayerMixer.SetInputWeight(0, 1 - rate);
                return true;
            }
        });
    }

    void onFinishMotion(AnimationClipPlayable nowPlayable)
    {
        nowPlayable.SetTime(0f); //ループさせるために時間を巻き戻す
    }
}