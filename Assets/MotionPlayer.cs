using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class MotionPlayer : MonoBehaviour
{
    private class Converter
    {
        public bool loop = false;
        public AnimationClipPlayable beforePlayable;
        public AnimationClipPlayable nowPlayable;
        public AnimationMixerPlayable mixer;
        private PlayableGraph graph;

        public Converter(PlayableGraph graph)
        {
            this.beforePlayable = AnimationClipPlayable.Create(graph, null);
            this.mixer = AnimationMixerPlayable.Create(graph, 2);
            this.graph = graph;
        }

        //AnimationClipの切り替え
        public void reconnect(AnimationClip clip)
        {
            //切断
            graph.Disconnect(mixer, 0);
            graph.Disconnect(mixer, 1);

            if (beforePlayable.IsValid())
                beforePlayable.Destroy();

            beforePlayable = nowPlayable;
            nowPlayable = AnimationClipPlayable.Create(graph, clip);

            //再接続
            mixer.ConnectInput(1, beforePlayable, 0);
            mixer.ConnectInput(0, nowPlayable, 0);
        }

        //Animationの再生が終了しているか
        public bool isPlayFinish()
        {
            if (nowPlayable.IsValid() == false)
                return false;

            //予定されている再生時間を超えていれば、再生終了とみなす
            if (nowPlayable.GetTime() >= nowPlayable.GetAnimationClip().length)
            {
                return true;
            }

            return false;
        }
    }

    [SerializeField]
    private AvatarMask upperMask;

    [Tooltip("Motionが完全に遷移するまでにかかる時間")]
    public float fadeDuration = 0.2f;

    //Playable API
    private List<Converter> converters = new List<Converter>();
    private AnimationLayerMixerPlayable layerMixer;
    private AnimationPlayableOutput output;
    private PlayableGraph graph;

    //GetComponent()による自動取得
    private Animator animator;

    //============================================================================================================

    void Awake()
    {
        animator = this.GetComponent<Animator>();

        //PlayableAPIの準備
        graph = PlayableGraph.Create();
        converters.Add(new Converter(graph));
        converters.Add(new Converter(graph));
        layerMixer = AnimationLayerMixerPlayable.Create(graph, 2);
        layerMixer.SetLayerMaskFromAvatarMask(1, upperMask);
        output = AnimationPlayableOutput.Create(graph, "output", animator);
        graph.Play();
    }

    void Update()
    {
        //アニメーションが終了しているか調べる
        foreach (var conv in converters)
        {
            if (conv.isPlayFinish() && conv.loop)
            {
                conv.nowPlayable.SetTime(0f); //時間を巻き戻しループ再生
            }
        }
    }

    void OnDestroy()
    {
        graph.Destroy();
    }

    //============================================================================================================

    public void play(AnimationClip clip, int layer, bool loop)
    {
        Converter conv = converters[layer];
        conv.loop = loop;

        //切断
        graph.Disconnect(layerMixer, layer);

        //再接続
        conv.reconnect(clip);
        layerMixer.ConnectInput(layer, conv.mixer, 0);

        //出力
        output.SetSourcePlayable(layerMixer);
        StartCoroutine(fadeCoroutine(fadeDuration, conv));
    }

    /// <summary>
    /// AnimatorのCrossFade()のようなもの
    /// 参考
    /// http://tsubakit1.hateblo.jp/entry/2017/07/30/032008
    /// </summary>
    /// <param name="duration">次のAnimationに「完全に」遷移する時間</param>
    /// <returns></returns>
    IEnumerator fadeCoroutine(float duration, Converter conv)
    {
        // 指定時間でアニメーションをブレンド
        float waitTime = Time.timeSinceLevelLoad + duration;
        yield return new WaitWhile(() =>
        {
            var diff = waitTime - Time.timeSinceLevelLoad;
            if (diff <= 0)
            {
                conv.mixer.SetInputWeight(1, 0);
                conv.mixer.SetInputWeight(0, 1);
                return false;
            }
            else
            {
                var rate = Mathf.Clamp01(diff / duration);
                conv.mixer.SetInputWeight(1, rate);
                conv.mixer.SetInputWeight(0, 1 - rate);
                return true;
            }
        });
    }
}