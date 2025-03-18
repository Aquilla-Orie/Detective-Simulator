using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TextAnimator _textAnimator;

    [SerializeField] private Animator _confidentialImageAnimator;

    private void Start()
    {
        float animTime = _textAnimator.AnimateText("Case File");
        Invoke("AnimateConfidentialImage", animTime); //Play the image animation after text is done animating
    }

    private void AnimateConfidentialImage()
    {
        _confidentialImageAnimator.SetTrigger("animate");
        _confidentialImageAnimator.GetComponent<AudioSource>().Play();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
