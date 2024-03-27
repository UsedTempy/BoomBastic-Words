using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Handler : MonoBehaviour {
    [Header("UI")]
    public UI_Preset[] UI_Presets;
    private Dictionary<string, UI_Preset> UI_Colour_Presets = new Dictionary<string, UI_Preset>();


    void Start() {
        InitializePresets();
        UpdateUIPreset("Orange");
    }

    private void InitializePresets() {
        for (int i = 0; i < UI_Presets.Length; i++) {
            UI_Preset Preset = UI_Presets[i];
            UI_Colour_Presets.Add(Preset.Preset_Name, Preset);
        }
    }

    private void ApplyUIPreset(UI_Preset Preset) {
        GameObject[] MainColourElements = GameObject.FindGameObjectsWithTag("MainColour");
        GameObject[] ShadingColourElements = GameObject.FindGameObjectsWithTag("ShadingColour");
        GameObject[] ExtraShadingColourElements = GameObject.FindGameObjectsWithTag("ExtraShadingColour");

        for (int i = 0; i < MainColourElements.Length; i++) {
            GameObject MainColourElement = MainColourElements[i];
            MainColourElement.GetComponent<Image>().color = Preset.MainColour;
        }

        for (int i = 0; i < ShadingColourElements.Length; i++) {
            GameObject ShadingColourElement = ShadingColourElements[i];
            ShadingColourElement.GetComponent<Image>().color = Preset.ShadingColour;
        }

        for (int i = 0; i < ExtraShadingColourElements.Length; i++) {
            GameObject ExtraShadingColourElement = ExtraShadingColourElements[i];
            ExtraShadingColourElement.GetComponent<Image>().color = Preset.ExtraShadingColour;
        }
    }

    private void UpdateUIPreset(string NewPreset) {
        if (!UI_Colour_Presets.ContainsKey(NewPreset)) return; // If the key doesn't exist it'll return
        ApplyUIPreset(UI_Colour_Presets[NewPreset]);
    }
}
