using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private RawImage[] _framePrefabs;
    [SerializeField] private Image _imagePanel;

    private List<RawImage> _generatedImages;

    private int[] _positions3Images = new int[] { -600, 0, 600 };
    private int[] _positions4Images = new int[] { -600, -200, 200, 600 };

    private TextAnimator _textAnimator;

    private void Start()
    {
        _generatedImages = new List<RawImage>();
        _textAnimator = GetComponent<TextAnimator>();

        _textAnimator.AnimateText("Loading...", true);

        InvokeRepeating("GenerateRandomImages", 0, 10f);
    }

    private void OnDisable()
    {
        CancelInvoke();
        ClearImages();
    }

    private void GenerateRandomImages()
    {
        //Clear previously generated images, if any
        ClearImages();

        System.Random rand = new System.Random();
        //Get the images from the asset folder
        var images = Resources.LoadAll<Texture2D>("Detective Loading Screen Images");

        //Randomize number of images to show
        int numToShow = Random.Range(3, 5);

        //pick random image texture
        //Make sure there are no repeating images
        var news = images.OrderBy(x => rand.Next()).Take(numToShow).ToList();


        for (int i = 0; i < news.Count(); i++)
        {
            var prefab = Instantiate(_framePrefabs[rand.Next(_framePrefabs.Length)], _imagePanel.rectTransform);
            prefab.rectTransform.GetChild(0).GetComponent<RawImage>().texture = news[i];
            prefab.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, (Random.value * 101) - 50));

            _generatedImages.Add(prefab);
        }

        AnimateImagesToPosition();
    }

    private void ClearImages()
    {
        if (_generatedImages != null && _generatedImages.Count > 0)
        {
            StopAllCoroutines();
            foreach (RawImage image in _generatedImages)
            {
                Destroy(image.gameObject);
            }
            _generatedImages.Clear();
        }
    }

    private void AnimateImagesToPosition()
    {
        var positions = _generatedImages.Count == 3 ? _positions3Images : _positions4Images;
        for (int i = 0; i < _generatedImages.Count; i++)
        {
            StartCoroutine(LerpPosition(_generatedImages[i], positions[i]));
        }
    }

    private IEnumerator LerpPosition(RawImage image, int targetxPos, float duration = 5)
    {
        yield return new WaitForSeconds(1f);

        float time = 0;
        Vector2 startPosition = image.rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(targetxPos, image.rectTransform.anchoredPosition.y);
        while (time < duration)
        {
            image.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        image.rectTransform.anchoredPosition = targetPosition;
    }
}
