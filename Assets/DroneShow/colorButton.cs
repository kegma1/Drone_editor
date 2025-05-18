using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class UIChangesAcrossScenes : MonoBehaviour
{
    private Color buttonColor = new Color32(0x13, 0x5D, 0x66, 0xFF);
    private Color textColor = Color.white;

    private Font newUnityFont;
    private TMP_FontAsset newTMPFont;

    private HashSet<GameObject> processedElements = new HashSet<GameObject>();

    void OnEnable()
    {
        newTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/BrunoAceSC-Regular SDF");
        newUnityFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Updated line

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
        processedElements.Clear(); // Reset for new scene
        ApplyUIChanges();
    }

    void OnCanvasRender()
    {
        ApplyUIChanges(); // Apply to any new elements that appear
    }

    void ApplyUIChanges()
    {
        ChangeButtonColors();
        ChangeFonts();
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
            {
                img.color = buttonColor;
            }

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

            if (newUnityFont != null)
            {
                int originalSize = text.fontSize;
                text.font = newUnityFont;
                text.fontSize = Mathf.Max(1, originalSize - 2);
            }
        }

        TMP_Text[] allTMPTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text tmpText in allTMPTexts)
        {
            if (processedElements.Contains(tmpText.gameObject))
                continue;

            processedElements.Add(tmpText.gameObject);

            if (newTMPFont != null)
            {
                float originalSize = tmpText.fontSize;
                tmpText.font = newTMPFont;
                tmpText.fontSize = Mathf.Max(1, originalSize - 2);
            }
        }
    }
}
