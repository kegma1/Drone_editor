using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIChangesAcrossScenes : MonoBehaviour
{
    
    private Color buttonColor = new Color32(0x13, 0x5D, 0x66, 0xFF);
 
    private Color textColor = Color.white;

    private Font newUnityFont;

    private TMP_FontAsset newTMPFont; 

    void OnEnable()
    {
        newTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/BrunoAceSC-Regular SDF");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangeButtonColors();
        ChangeFonts();
    }

    void ChangeButtonColors()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = buttonColor;
            }


            Text text = btn.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = textColor;
                if (newUnityFont != null)
                    text.font = newUnityFont;
            }


            TMP_Text tmpText = btn.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.color = textColor;
                if (newTMPFont != null)
                    tmpText.font = newTMPFont;
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
            if (newUnityFont != null)
            {
                text.font = newUnityFont;
            }
        }

        TMP_Text[] allTMPTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text tmpText in allTMPTexts)
        {
            if (newTMPFont != null)
            {
                tmpText.font = newTMPFont; 
            }
        }
    }
}
