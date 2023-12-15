using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static float PanelHeight { get { return panelElement.resolvedStyle.height;  } }
    public static VisualElement panelElement;

    ECS_HertaManager manager;
    FPSCounter fpsCounter;

    VisualElement rootElement, settingsPanel;
    Button clearButton, kuruButton, exitButton, settingsButton;
    Toggle rndSizeTgl, rndSpdTgl, dontDestroyTgl;
    Label countDisplay, fpsDisplay;

    private string[] suffixes = new string[] { "", "K", "M", "B", "T", "Q" };

    private void Awake()
    {
        fpsCounter = gameObject.GetComponent<FPSCounter>();
        manager = GameObject.FindGameObjectWithTag("Player").GetComponent<ECS_HertaManager>();
        rootElement = GetComponent<UIDocument>().rootVisualElement;

        panelElement = rootElement.Q<VisualElement>("panel-element");
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
        clearButton.clicked += manager.ClearHertaEntity;
        kuruButton.clicked += manager.SpawnHertaEntity;
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
        //PanelHeight = panelElement.resolvedStyle.height;
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

    private string ShortenNumbers(float value)
    {
        string shortenNumbers = string.Empty;

        if (1000000 <= value) shortenNumbers = Math.Floor(value / 1000000).ToString() + "K";
        else if (1000 <= value) shortenNumbers = Math.Floor(value / 1000).ToString() + "K";
        else shortenNumbers = value.ToString();
        
        return shortenNumbers;
    }
}
