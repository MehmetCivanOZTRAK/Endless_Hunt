using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private Slider loadingSlider;
    private float progress = 0f;

    public void ShowLoadingScreen(string sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(string scene_Id)
    {

        AsyncOperation operation = SceneManager.LoadSceneAsync(scene_Id);
        loadingPanel.SetActive(true);
        while (!operation.isDone)
        {
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            loadingText.text = progress * 100f + "%";
            yield return null;
        }
    }
}