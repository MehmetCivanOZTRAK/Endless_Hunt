using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSlider : MonoBehaviour
{
[Header("Settings")]
[SerializeField] private ScrollRect _scrollRect;
[SerializeField] private RectTransform _contentPanel;
[SerializeField] private RectTransform _sampleItem;

[Header("Scene Names")]
[SerializeField] private List<string> _sceneNames;

[SerializeField] private EnergySystem energySystem;

private float itemwidth;
private int currentIndex = 0;
private bool isDragging = false;

private void Start() {
    HorizontalLayoutGroup layoutGroup = _contentPanel.GetComponent<HorizontalLayoutGroup>();
    float spacing = layoutGroup ? layoutGroup.spacing : 0f;
    itemwidth = _sampleItem.rect.width + spacing;
}
 private void Update() {
    if(Input.touchCount > 0)
        {
            isDragging = true;
            return;
        }
       isDragging = false;
    
    EnYakinaHizala();
 }

 private void EnYakinaHizala()
    {
        float currentX = _contentPanel.anchoredPosition.x;

        int index = Mathf.RoundToInt(Mathf.Abs(currentX) / itemwidth);

        if(index < 0) index = 0;
        if(index >= _sceneNames.Count) index = _sceneNames.Count -1;
        currentIndex = index;
        float targetX = index * -itemwidth;

        Vector2 newPosition = new Vector2(Mathf.Lerp(_contentPanel.anchoredPosition.x, targetX, Time.deltaTime * 10f), _contentPanel.anchoredPosition.y);
        _contentPanel.anchoredPosition = newPosition;
    }
    public void StartSelectedLevel()
    {
        string selectedScene = _sceneNames[currentIndex];

        energySystem.StartGame(selectedScene);
    }
}
