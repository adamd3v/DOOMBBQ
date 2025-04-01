using UnityEngine;

namespace NEP.DOOMBBQ.Sound
{
    public class PooledAudio : MonoBehaviour
    {
        private AudioSource source;
        private float time;

        private void Start()
        {
            source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            time += Time.deltaTime;

            if (time >= source.clip.length)
            {
                gameObject.SetActive(false);
                time = 0f;
            }
        }
    }
}