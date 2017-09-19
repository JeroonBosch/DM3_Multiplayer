using UnityEngine;

namespace Com.Hypester.DM3
{
    public class AudioController : MonoBehaviour
    {
        // Public field, set in the inspector we can access
        // the audio clip through the singleton instance
        public AudioClip explosionClip;

        // Static singleton property
        public static AudioController Instance { get; private set; }

        void Awake()
        {
            // First we check if there are any other instances conflicting
            if (Instance != null && Instance != this)
            {
                // If that is the case, we destroy other instances
                Destroy(gameObject);
            }

            // Here we save our singleton instance
            Instance = this;

            // Furthermore we make sure that we don't destroy between scenes (this is optional)
            DontDestroyOnLoad(gameObject);
        }

        // Instance method, this method can be accesed through the singleton instance
        public void PlayAudio(AudioClip clip)
        {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
        }
    }
}