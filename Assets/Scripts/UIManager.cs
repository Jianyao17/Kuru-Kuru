using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    HertaManager manager;
    FPSCounter fpsCounter;

    VisualElement rootElement, settingsPanel;
    Button clearButton, kuruButton, exitButton, settingsButton;
    Toggle rndSizeTgl, rndSpdTgl, dontDestroyTgl;
    Label countDisplay, fpsDisplay;

    private void Awake()
    {
        fpsCounter = gameObject.GetComponent<FPSCounter>();
        manager = GameObject.FindGameObjectWithTag("Player").GetComponent<HertaManager>();
        rootElement = GetComponent<UIDocument>().rootVisualElement;

        settingsPanel = rootElement.Q<VisualElement>("settings-panel");
        fpsDisplay = rootElement.Q<Label>("framerate-count-label");
        countDisplay = rootElement.Q<Label>("herta-count-label");

        rndSizeTgl = rootElement.Q<Toggle>("randSize-toggle");
        rndSpdTgl = rootElement.Q<Toggle>("randSpeed-toggle");
        dontDestroyTgl = rootElement.Q<Toggle>("dontDestroy-toggle");

        settingsButton = rootElement.Q<Button>("settings-button");
        clearButton = rootElement.Q<Button>("clear-button");
        kuruButton = rootElement.Q<Button>("kuru-kuru-button");
        exitButton = rootElement.Q<Button>("exit-button");

        settingsButton.clicked += SettingsButton_clicked;
        clearButton.clicked += manager.ClearHertaList;
        kuruButton.clicked += manager.SpawnHerta;
        exitButton.clicked += ExitButton_clicked;

        rndSizeTgl.RegisterValueChangedCallback(randomSizeToggle);
        rndSpdTgl.RegisterValueChangedCallback(randomSpeedToggle);
        dontDestroyTgl.RegisterValueChangedCallback(dontDestroyToggle);

        settingsPanel.style.display = DisplayStyle.None;
    }

    private void randomSizeToggle(ChangeEvent<bool> evt)
    {
        manager.randomSize = rndSizeTgl.value;
    }
    private void randomSpeedToggle(ChangeEvent<bool> evt)
    {
        manager.randomSpeed = rndSpdTgl.value;
    }
    private void dontDestroyToggle(ChangeEvent<bool> evt)
    {
        manager.dontDestroy = dontDestroyTgl.value;
    }

    private void Update()
    {
        if (int.Parse(countDisplay.text) != manager.HertaCount)
            countDisplay.text = manager.HertaCount.ToString();

        fpsDisplay.text = fpsCounter.CalculateFPS().ToString("000");
    }

    private void SettingsButton_clicked()
    {
        if (settingsPanel.style.display == DisplayStyle.None) settingsPanel.style.display = DisplayStyle.Flex;
        else if (settingsPanel.style.display == DisplayStyle.Flex) settingsPanel.style.display = DisplayStyle.None;

        if (settingsPanel.ClassListContains("settings-panel-visible")) settingsPanel.RemoveFromClassList("settings-panel-visible");
        else settingsPanel.AddToClassList("settings-panel-visible");
    }

    private void ExitButton_clicked()
    {
        Application.Quit();
    }
}
