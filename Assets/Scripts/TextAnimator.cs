using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TextAnimator : MonoBehaviour
{
    public bool IsAnimating { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    [SerializeField] private TMP_Text _textToAnimate;

    [SerializeField] private float _animTimePerCharacter = .3f;

    private string _text;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _audioClip;
        IsAnimating = false;
    }

    //Animates text and also returns total animation time
    public float AnimateText(string text, bool loop = false)
    {
        _text = text;
        StartCoroutine(Animate(text, loop));

        return _animTimePerCharacter * text.Length;
    }

    private IEnumerator Animate(string text, bool loop)
    {
        IsAnimating = true;
        do
        {
            _textToAnimate.text = "";

            foreach (char letter in text)
            {
                _textToAnimate.text += letter.ToString();

                _audioSource.pitch = Random.Range(.8f, 1.2f); //Change the pitch of the audio to make it sound different each time
                _audioSource.Play();

                yield return new WaitForSeconds(_animTimePerCharacter);
            }
        }
        while (loop);
        IsAnimating = false;
    }

    public void SkipTextAnimation()
    {
        StopAllCoroutines();
        _textToAnimate.text = _text;
    }
}
