using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
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

    [Header("Lobby Creator Elements")] 
    [SerializeField] private GameObject PublicButton;
    [SerializeField] private GameObject PrivateButton;
    [SerializeField] private GameObject CreateRoomPanel;
    [SerializeField] private GameObject JoinRoomPanel;

    [Header("Modules")]
    [SerializeField] private LobbyHandler LobbyHandler;

    [Header("Private Variables")]
    [SerializeField] private bool IsProcessing = false;

    [Header("Loading Screen Elements")] 
    [SerializeField] private GameObject LoadingScreenElement;
    [SerializeField] private GameObject LoadingImage;
    [SerializeField] private GameObject LoadingText;

    [Header("UserInterface Page")] 
    [SerializeField] private GameObject ServerList;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject GameContent;
    [SerializeField] private GameObject BackgroundContent;
    private Dictionary<string, GameObject> Pages = new Dictionary<string, GameObject>();

    private bool IsPrivateLobby = false;

    void Start() {
        InitializePresets();
        UpdateUIPreset(ActivePreset);

        LeanTween.rotateAround(LoadingImage, Vector3.back, 360, 4f).setLoopClamp();
        Pages.Add("ServerList", ServerList);
        Pages.Add("LoadingScreen", LoadingScreen);
        Pages.Add("GameContent", GameContent);

        SetUserInterfaceState("ServerList", true);
    }

    void Update() {
        UpdatePlayerLayout();
        RenderPlayerLayout();
        HandleLoadingEffects();
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
        List<Vector2> generatedPoints = GeneratePointsAround(CenterPlayerPosition, Mathf.Clamp(Mathf.Sqrt(PlayerLayout.Count * 1.5f), 3f, Mathf.Infinity), PlayerLayout.Count);

        for (int i = 0; i < generatedPoints.Count; i++) {
            Vector2 offset = generatedPoints[i];
            GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateParent);
            NewPlayerTemplate.transform.Translate(new Vector3(offset.x, offset.y, 0));
            NewPlayerTemplate.SetActive(true);

            ActivePlayerTemplates.Add(NewPlayerTemplate);
        }
    }

    private void UpdateUIPreset(string NewPreset) {
        if (!UI_Colour_Presets.ContainsKey(NewPreset)) return; // If the key doesn't exist it'll return
        ApplyUIPreset(UI_Colour_Presets[NewPreset]);
    }

    public void SetLoadingState(bool State) {
        if (IsProcessing == State) return;
        IsProcessing = State;

        LoadingScreenElement.SetActive(State);
    }

    private void HandleLoadingEffects() {
        if (IsProcessing == false) return;
        float SineWave = Mathf.Sin(Time.time);
        LoadingImage.transform.localScale = new Vector3(5 + SineWave, 5 + SineWave, 1);
    }

    public void CreateButtonFunction(TMP_InputField Field) {
        if (IsProcessing) return;
        if (Field.text.Length <= 4) return;
        SetLoadingState(true);

        LobbyHandler.CreateLobby(Field.text, 12);
    }

    public void SetUserInterfaceState(string State, bool Active) {
        foreach (KeyValuePair<string, GameObject> Page in Pages) {
            Page.Value.SetActive(Page.Key == State ? Active : false);

            if (State == "GameContent") {
                BackgroundContent.transform.Find("Shapes").gameObject.SetActive(!Active);
            }
        }
    }

    public void JoinPrivateButtonFunction(TMP_InputField Field) {
        if (IsProcessing) return;
        SetLoadingState(true);

        Debug.Log("Join Code: " + Field.text);
        Field.text = "";
    }

    public void SetLobbyState(bool NewState) {
        IsPrivateLobby = NewState;

        PublicButton.transform.Find("Top").GetComponent<Image>().color = IsPrivateLobby ? new Color(0, 0.6274f, 1) : new Color(0, 0.488642f, 0.7987421f);
        PrivateButton.transform.Find("Top").GetComponent<Image>().color = IsPrivateLobby ? new Color(0, 0.488642f, 0.7987421f) : new Color(0, 0.6274f, 1);
    }
}
