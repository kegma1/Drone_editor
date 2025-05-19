using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class UIChangesAcrossScenes : MonoBehaviour
{
    private Color buttonColor = new Color32(0x13, 0x5D, 0x66, 0xFF);
    private Color panelColor = new Color32(27, 127, 139, 115);
    private Color fileItemColor = new Color32(100, 220, 230, 255);
    private Color inputFieldColor = new Color32(10, 60, 66, 255);
    private Color checkboxColor = new Color32(10, 60, 66, 255);
    private Color checkmarkColor = Color.white;
    private Color textColor = Color.white;
    private Color fileTextColor = Color.black;


    private Font newUnityFont;
    private TMP_FontAsset newTMPFont;

    private HashSet<GameObject> processedElements = new HashSet<GameObject>();

    void OnEnable()
    {
        newTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/BrunoAceSC-Regular SDF");
        newUnityFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        SceneManager.sceneLoaded += OnSceneLoaded;
        Canvas.willRenderCanvases += OnCanvasRender;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Canvas.willRenderCanvases -= OnCanvasRender;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        processedElements.Clear();
        ApplyUIChanges();
    }

    void OnCanvasRender()
    {
        ApplyUIChanges();
    }

    void ApplyUIChanges()
    {
        ChangeButtonColors();
        ChangeFonts();
        ChangePanelsAndBackgrounds();
        ChangeInputFields();
        ChangeToggles();
    }

    void ChangeButtonColors()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            if (processedElements.Contains(btn.gameObject))
                continue;

            processedElements.Add(btn.gameObject);

            Image img = btn.GetComponent<Image>();
            if (img != null)
                img.color = buttonColor;

            Text legacyText = btn.GetComponentInChildren<Text>(true);
            if (legacyText != null)
            {
                legacyText.color = textColor;
                if (newUnityFont != null)
                {
                    int originalSize = legacyText.fontSize;
                    legacyText.font = newUnityFont;
                    legacyText.fontSize = Mathf.Max(1, originalSize - 2);
                }
            }

            TMP_Text tmpText = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                tmpText.color = textColor;
                if (newTMPFont != null)
                {
                    float originalSize = tmpText.fontSize;
                    tmpText.font = newTMPFont;
                    tmpText.fontSize = Mathf.Max(1, originalSize - 2);
                }
            }

            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color32(0xA1, 0xD0, 0xFF, 0xFF);
            btn.colors = colors;
        }
    }

    void ChangeFonts()
    {
        Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (Text text in allTexts)
        {
            if (processedElements.Contains(text.gameObject))
                continue;

            processedElements.Add(text.gameObject);

            bool isFileText = IsFileTextElement(text.gameObject);
            text.color = isFileText ? fileTextColor : textColor;

            if (newUnityFont != null)
            {
                text.font = newUnityFont;
            }
        }

        TMP_Text[] allTMPTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text tmpText in allTMPTexts)
        {
            if (processedElements.Contains(tmpText.gameObject))
                continue;

            processedElements.Add(tmpText.gameObject);

            bool isFileText = IsFileTextElement(tmpText.gameObject);
            tmpText.color = isFileText ? fileTextColor : textColor;

            if (newTMPFont != null)
            {
                tmpText.font = newTMPFont;
            }
        }
    }


    void ChangePanelsAndBackgrounds()
    {
        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);

        foreach (Image img in allImages)
        {
            GameObject go = img.gameObject;

            if (processedElements.Contains(go))
                continue;

            if (go.GetComponent<Button>() != null || 
                go.GetComponent<InputField>() != null || 
                go.GetComponent<TMP_InputField>() != null)
                continue;

            string name = go.name.ToLower();
            string parentName = go.transform.parent != null ? go.transform.parent.name.ToLower() : "";

            if (name.Contains("sky") || parentName.Contains("sky"))
                continue;

            if (name.Contains("file") || parentName.Contains("file"))
                img.color = fileItemColor;
            else
                img.color = panelColor;

            processedElements.Add(go);
        }
    }


    void ChangeInputFields()
    {
        TMP_InputField[] tmpInputs = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        foreach (TMP_InputField input in tmpInputs)
        {
            GameObject go = input.gameObject;

            if (processedElements.Contains(go))
                continue;

            Image bg = input.GetComponent<Image>();
            if (bg != null)
                bg.color = inputFieldColor;

            input.textComponent.color = textColor;
            if (newTMPFont != null)
            {
                float originalSize = input.textComponent.fontSize;
                input.textComponent.font = newTMPFont;
                input.textComponent.fontSize = Mathf.Max(1, originalSize - 2);
            }

            processedElements.Add(go);
        }

        InputField[] legacyInputs = FindObjectsByType<InputField>(FindObjectsSortMode.None);
        foreach (InputField input in legacyInputs)
        {
            GameObject go = input.gameObject;

            if (processedElements.Contains(go))
                continue;

            Image bg = input.GetComponent<Image>();
            if (bg != null)
                bg.color = inputFieldColor;

            Text text = input.textComponent;
            if (text != null)
            {
                text.color = textColor;
                if (newUnityFont != null)
                {
                    int originalSize = text.fontSize;
                    text.font = newUnityFont;
                    text.fontSize = Mathf.Max(1, originalSize - 2);
                }
            }

            processedElements.Add(go);
        }
    }

    void ChangeToggles()
    {
        Toggle[] allToggles = FindObjectsByType<Toggle>(FindObjectsSortMode.None);

        foreach (Toggle toggle in allToggles)
        {
            GameObject go = toggle.gameObject;

            if (processedElements.Contains(go))
                continue;

            Image bg = toggle.targetGraphic as Image;
            if (bg != null)
                bg.color = checkboxColor;

            if (toggle.graphic is Image checkmarkImg)
                checkmarkImg.color = checkmarkColor;

            processedElements.Add(go);
        }
    }
    bool IsFileTextElement(GameObject go)
    {
        string name = go.name.ToLower();
        string parentName = go.transform.parent != null ? go.transform.parent.name.ToLower() : "";

        return name.Contains("file") || name.Contains(".json") || parentName.Contains("file") || parentName.Contains(".json");
    }

}
