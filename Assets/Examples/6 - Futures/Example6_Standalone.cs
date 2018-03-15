using System.Collections;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example6_Standalone : MonoBehaviour
    {
        [SerializeField]
        private Example6_Service m_Service = null;

        [SerializeField]
        private AudioSource m_URLAudioBoxOutput = null;

        [SerializeField]
        private string m_URLAudioBox = null;

        [SerializeField]
        private bool m_AudioCompressed = true;

        private void Start()
        {
            DownloadAudio(m_URLAudioBox);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
                return;

            DownloadAudio(m_URLAudioBox);
        }
#endif // UNITY_EDITOR

        private void DownloadAudio(string inAudio)
        {
            var future = Future.Download.AudioClip(inAudio, m_AudioCompressed)
                .OnComplete((clip) =>
                {
                    m_URLAudioBoxOutput.Stop();

                    AudioClip oldClip = m_URLAudioBoxOutput.clip;
                    m_URLAudioBoxOutput.clip = clip;
                    Destroy(oldClip);

                    m_URLAudioBoxOutput.loop = true;
                    m_URLAudioBoxOutput.Play();
                })
                .OnFail((o) =>
                {
                    m_URLAudioBoxOutput.Stop();

                    Debug.Log("Audio download failed: " + o.ToString());
                });

        }
    }
}