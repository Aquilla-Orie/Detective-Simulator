using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LMNT
{
    public class LMNTSpeech : MonoBehaviour 
    {
          private AudioSource _audioSource;
          private string _apiKey;
          private List<Voice> _voiceList;
          private DownloadHandlerAudioClip _handler;

          public string voice;
          public string dialogue;

          public 

          void Awake()
          {
	          _audioSource = gameObject.GetComponent<AudioSource>();
	          if (_audioSource == null) {
		          _audioSource = gameObject.AddComponent<AudioSource>();
	          }
	          _apiKey = LMNTLoader.LoadApiKey();
              _voiceList = LMNTLoader.LoadVoices();
          }

          public IEnumerator Prefetch()
          {
                if (_handler != null) {
                  yield break;
                }

                WWWForm form = new WWWForm();
                form.AddField("voice", LookupByName(voice));
                form.AddField("text", dialogue);
                using (UnityWebRequest request = UnityWebRequest.Post(Constants.LMNT_SYNTHESIZE_URL, form)) {
                    _handler = new DownloadHandlerAudioClip(Constants.LMNT_SYNTHESIZE_URL, AudioType.WAV);
                    request.SetRequestHeader("X-API-Key", _apiKey);
                    // TODO: do not hard-code; find a clean way to get package version at runtime
                    request.SetRequestHeader("X-Client", "unity/0.1.0");
                    request.downloadHandler = _handler;
                    yield return request.SendWebRequest();

                    _audioSource.clip = _handler.audioClip;
                    Debug.Log("Prefetch done");
                }
          }

          public IEnumerator Talk() {
                _handler = null;
                _audioSource.clip = null;

                if (_handler == null) {
                  StartCoroutine(Prefetch());
                }
                if (_audioSource.clip == null) {
                  yield return new WaitUntil(() => _audioSource.clip != null);
                }
                _audioSource.Play();
            Debug.Log("Talking");
          }

        public IEnumerator TalkWithAnim(Animator anim)
        {
            _handler = null;
            _audioSource.clip = null;

            if (_handler == null)
            {
                StartCoroutine(Prefetch());
            }
            if (_audioSource.clip == null)
            {
                yield return new WaitUntil(() => _audioSource.clip != null);
            }
            _audioSource.Play();
            anim.SetTrigger("switch");
            yield return new WaitWhile(() => _audioSource.isPlaying);
            anim.SetTrigger("switch");
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        private string LookupByName(string name) {
            return _voiceList.Find(v => v.name == name).id;
          }
    }

}
