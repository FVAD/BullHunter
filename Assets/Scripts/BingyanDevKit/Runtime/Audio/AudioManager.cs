using System.Collections.Generic;
using UnityEngine;

namespace Bingyan
{
    internal class AudioManager : MonoBehaviour
    {
        internal static AudioManager Instance { get; private set; }

        static AudioManager()
        {
            Instance = new GameObject("DevKit_Audio").AddComponent<AudioManager>();
            DontDestroyOnLoad(Instance.gameObject);

            Instance.Init();
        }

        private AudioMapConfig config;
        private List<SourceState> states;

        private void Init()
        {
            config = AudioMapConfig.Instance;
            config.Init();
            states = new List<SourceState>();
        }

        /// <summary>
        /// 播放<paramref name="player"/>，且保证只有一个AudioSource播放它
        /// </summary>
        internal AudioSource PlaySingleton(AudioRef player, Vector3 position = default)
        {
            var source = Play(player, null, true);
            source.transform.position = position;
            return source;
        }

        /// <summary>
        /// 跟踪<paramref name="target"/>播放<paramref name="player"/>，且保证只有一个AudioSource播放它
        /// </summary>
        internal AudioSource PlaySingleton(AudioRef player, GameObject target) => Play(player, target, true);

        /// <summary>
        /// 播放<paramref name="player"/>
        /// </summary>
        internal AudioSource Play(AudioRef player, Vector3 position = default)
        {
            var source = Play(player, null, false);
            source.transform.position = position;
            return source;
        }

        /// <summary>
        /// 跟踪<paramref name="target"/>播放<paramref name="player"/>
        /// </summary>
        internal AudioSource Play(AudioRef player, GameObject target) => Play(player, target, false);

        private AudioSource Play(AudioRef player, GameObject target, bool singleton)
        {
            if (player.Name.Length == 0) return null;

            SourceState state = null;

            foreach (var item in states)
            {
                if (singleton && item.Name == player.Name && item.Source.isPlaying)
                {
                    item.Target = target;
                    return item.Source;
                }
                if (!item.Source.isPlaying)
                {
                    state = item;
                    if (!singleton) break;
                }
            }

            if (state == null)
            {
                state = new SourceState
                {
                    Source = new GameObject().AddComponent<AudioSource>()
                };
                state.Source.transform.parent = transform;
                states.Add(state);
            }
            state.Name = player.Name;
            state.Target = target;

            var info = config[player.Name];

            state.Source.clip = info.Clips.Length > 0 ? info.Clips[Random.Range(0, info.Clips.Length)] : null;
            state.Source.loop = info.Loop;
            state.Source.pitch = 1 + info.Pitch;
            state.Source.outputAudioMixerGroup = info.Bus;
            state.Source.maxDistance = info.Range;
            state.Source.spatialBlend = info.Range <= 0 ? 0 : 1;
            state.TimeSamples = 0;
            state.Clip = info;
            state.Source.Play();
            return state.Source;
        }

        /// <summary>
        /// 停止播放<paramref name="player"/>
        /// </summary>
        internal void Stop(AudioRef player) => Stop(player.Name);
        internal void Stop(string name)
        {
            if (name.Length == 0) return;

            foreach (var info in states)
                if (info.Target == null && info.Name == name)
                    info.Source.Stop();
        }

        /// <summary>
        /// 停止播放正在跟踪<paramref name="target"/>的<paramref name="player"/>
        /// </summary>
        internal void Stop(AudioRef player, GameObject target)
        {
            foreach (var info in states)
                if (info.Target == target && info.Name == player.Name)
                    info.Source.Stop();
        }

        private void FixedUpdate()
        {
            foreach (var info in states)
                if (info.Source.isPlaying)
                {
                    if (info.Source.timeSamples < info.TimeSamples)
                        info.Source.pitch = 1 + info.Clip.Pitch;
                    info.TimeSamples = info.Source.timeSamples;

                    if (info.Target)
                        info.Source.transform.position = info.Target.transform.position;
                    else if (info.Target != null)
                        Stop(info.Name);
                }
        }

        private class SourceState
        {
            internal string Name;
            internal GameObject Target;
            internal AudioSource Source;
            internal float TimeSamples;
            internal ClipInfo Clip;
        }
    }
}