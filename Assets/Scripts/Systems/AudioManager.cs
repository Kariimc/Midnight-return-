using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private int         _sfxPoolSize = 16;

        private AudioSource[]  _sfxPool;
        private int            _sfxPoolIdx;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildPool();
        }

        private void BuildPool()
        {
            _sfxPool = new AudioSource[_sfxPoolSize];
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                _sfxPool[i] = go.AddComponent<AudioSource>();
                _sfxPool[i].spatialBlend = 0f; // 2D
            }
        }

        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            var src   = _sfxPool[_sfxPoolIdx++ % _sfxPoolSize];
            src.clip  = clip;
            src.volume = volume;
            src.pitch  = pitch + Random.Range(-0.03f, 0.03f); // micro-randomize pitch
            src.Play();
        }

        public void PlayMusic(AudioClip clip, float fadeIn = 1f)
        {
            if (clip == null || _musicSource == null) return;
            if (_musicSource.isPlaying)
            {
                StartCoroutine(CrossFade(clip, fadeIn));
            }
            else
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }

        private System.Collections.IEnumerator CrossFade(AudioClip newClip, float duration)
        {
            float t = 0f, startVol = _musicSource.volume;
            while (t < duration * 0.5f) { t += Time.deltaTime; _musicSource.volume = Mathf.Lerp(startVol, 0f, t / (duration * 0.5f)); yield return null; }
            _musicSource.clip = newClip;
            _musicSource.Play();
            t = 0f;
            while (t < duration * 0.5f) { t += Time.deltaTime; _musicSource.volume = Mathf.Lerp(0f, startVol, t / (duration * 0.5f)); yield return null; }
            _musicSource.volume = startVol;
        }
    }
}
