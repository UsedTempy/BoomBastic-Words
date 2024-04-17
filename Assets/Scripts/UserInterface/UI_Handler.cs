using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Handler : MonoBehaviour {
    [Header("UI Visuals")]
    public UI_Preset[] UI_Presets;
    private Dictionary<string, UI_Preset> UI_Colour_Presets = new Dictionary<string, UI_Preset>();
    [SerializeField] private string ActivePreset;

    [Header("Game Bomb Settings")] 
    [SerializeField] private int AmountOfPlayers;
    [SerializeField] private Vector2 CenterPlayerPosition;
    [SerializeField] private GameObject PlayerTemplate;
    [SerializeField] private Transform PlayerTemplateParent;

    private Dictionary<int, string> PlayerLayout = new Dictionary<int, string>();
    private List<GameObject> ActivePlayerTemplates = new List<GameObject>();

    void Start() {
        InitializePresets();
        UpdateUIPreset(ActivePreset);
    }

    void Update() {
        UpdatePlayerLayout();
        RenderPlayerLayout();
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

    public static List<Vector2> GeneratePointsAround(Vector2 center, float radius, int numPoints) {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < numPoints; i++) {
            float angle = i * 2 * (float)Mathf.PI / numPoints;
            float x = center.x + radius * (float)Mathf.Cos(angle);
            float y = center.y + radius * (float)Mathf.Sin(angle);
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    private void UpdatePlayerLayout() {
        PlayerLayout.Clear();

        for (int i = 0; i < AmountOfPlayers; i++) {
            PlayerLayout.Add(i, "Player " + i.ToString());
        }

        for (int i = 0; i < ActivePlayerTemplates.Count; i++) {
            Destroy(ActivePlayerTemplates[i]);
        }

        ActivePlayerTemplates.Clear();
    }

    private void RenderPlayerLayout() {
        List<Vector2> generatedPoints = GeneratePointsAround(CenterPlayerPosition, Mathf.Clamp(Mathf.Sqrt(PlayerLayout.Count), 3f, Mathf.Infinity), PlayerLayout.Count);

        for (int i = 0; i < generatedPoints.Count; i++) {
            Vector2 offset = generatedPoints[i];
            GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateParent);
            NewPlayerTemplate.transform.Translate(new Vector3(offset.x, offset.y, 0));
            ActivePlayerTemplates.Add(NewPlayerTemplate);
        }
    }

    private void UpdateUIPreset(string NewPreset) {
        if (!UI_Colour_Presets.ContainsKey(NewPreset)) return; // If the key doesn't exist it'll return
        ApplyUIPreset(UI_Colour_Presets[NewPreset]);
    }
}
