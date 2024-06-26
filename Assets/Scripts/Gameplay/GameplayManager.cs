using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class GameplayManager : NetworkBehaviour {
    [Header("PlayerList")]
    [SerializeField] private GameObject PlayerTemplateContainer;
    [SerializeField] private GameObject PlayerTemplate;

    [Header("Chatbox")]
    [SerializeField] private GameObject InputFieldObject;
    [SerializeField] private GameObject SendMessageObject;
    [SerializeField] private GameObject ChatboxMessageObject;
    [SerializeField] private GameObject ChatboxMessageParent;

    [Header("Player Content")] 
    [SerializeField] private GameObject PlayerContentParent;
    [SerializeField] private GameObject PlayerContentTemplate;

    [Header("Gameplay Content")] 
    [SerializeField] private GameObject ArrowObject;
    [SerializeField] private GameObject TurnPlayGameObject;
    [SerializeField] private GameObject TimerTextObject;
    [SerializeField] private GameObject KeyTemplateObject;
    [SerializeField] private GameObject KeyParentObject;
 
    [Header("Components")]
    [SerializeField] private Sprite HeartActive;
    [SerializeField] private Sprite HeartInactive;

    [Header("Round Conditions")]
    [SerializeField] public GameObject Win;
    [SerializeField] public GameObject Lose;

    [Header("Loading Screen Elements")]
    [SerializeField] public GameObject LoadingScreenElement;
    [SerializeField] private GameObject LoadingImage;
    [SerializeField] private GameObject LoadingText;


    //private List<GameObject> PlayerTemplates = new List<GameObject>();
    private Dictionary<string, GameObject> PlayerTemplates = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> PlayerIcons = new Dictionary<string, GameObject>();

    private List<string> KeysPressed = new List<string>();
    private List<GameObject> KeysPressedGameObjects = new List<GameObject>();

    private List<string> GivenKeys = new List<string>();
    [SerializeField] private List<GameObject> GivenKeysObjects = new List<GameObject>();

    //private List<string> ForcedUsedKey = new List<string>();
    private Dictionary<string, int> ForcedUsedKey = new Dictionary<string, int>();
    private int ForcedKeysUsed = 0;

    private long TimerCounterInt = 0;
    private int forcedCharactersUses = 0;
    private bool IsProcessing = true;

    void Start() {
        LeanTween.rotateAround(LoadingImage, Vector3.back, 360, 4f).setLoopClamp();
    }

    public void AddUserTemplate(string userNamesList) {
        foreach (var playerTemplate in PlayerTemplates) {
            Destroy(playerTemplate.Value);
        }
        PlayerTemplates.Clear();

        foreach (var Username in userNamesList.Split(',')) {
            GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateContainer.transform);
            NewPlayerTemplate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
            NewPlayerTemplate.SetActive(true);

            PlayerTemplates.Add(Username, NewPlayerTemplate);
        }
    }

    public string ReturnMessageField() {
        string InputField = InputFieldObject.GetComponent<TMP_InputField>().text;
        InputFieldObject.GetComponent<TMP_InputField>().text = "";

        return InputField;
    }

    private void HandleLoadingEffects() {
        if (IsProcessing == false) return;
        float SineWave = Mathf.Sin(Time.time);
        LoadingImage.transform.localScale = new Vector3(5 + SineWave, 5 + SineWave, 1);
    }

    public void CreateChatMessage(string Username, string Message) {
        Debug.Log(Username + ": " + Message);
        GameObject newChatMessageObject = Instantiate(ChatboxMessageObject, ChatboxMessageParent.transform);
        newChatMessageObject.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
        newChatMessageObject.transform.Find("Message").GetComponent<TMP_Text>().text = Message;

        newChatMessageObject.SetActive(true);
    }

    public void UpdateLives(string Username, int AmountOfLives) {
        if (!PlayerIcons.ContainsKey(Username)) return;
        GameObject PlayerIcon = PlayerIcons[Username];
            
        if (AmountOfLives == 1) {
            PlayerIcon.transform.Find("Life1").GetComponent<Image>().sprite = HeartActive;
            PlayerIcon.transform.Find("Life2").GetComponent<Image>().sprite = HeartInactive;
        } else if(AmountOfLives == 2) {
            PlayerIcon.transform.Find("Life1").GetComponent<Image>().sprite = HeartActive;
            PlayerIcon.transform.Find("Life2").GetComponent<Image>().sprite = HeartActive;
        } else {
            PlayerIcon.transform.Find("Life1").GetComponent<Image>().sprite = HeartInactive;
            PlayerIcon.transform.Find("Life2").GetComponent<Image>().sprite = HeartInactive;
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

    public void UpdatePlayerTemplates(string userNamesList) {
        foreach (KeyValuePair<string, GameObject> PlayerIcon in PlayerIcons) {
            Destroy(PlayerIcon.Value);
            PlayerIcons.Remove(PlayerIcon.Key);
        }

        List<Vector2> generatedPoints = GeneratePointsAround(new Vector2(0, 0), 4.2464f, userNamesList.Split(',').Length);
        int i = 0;
        foreach (var Username in userNamesList.Split(',')) {
            Vector2 offset = generatedPoints[i];
            i++;

            GameObject NewPlayerTemplate = Instantiate(PlayerContentTemplate, PlayerContentParent.transform);
            NewPlayerTemplate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
            NewPlayerTemplate.transform.Translate(new Vector3(offset.x, offset.y, 0));

            NewPlayerTemplate.SetActive(true);
            PlayerIcons.Add(Username, NewPlayerTemplate);
        }
    }

    public static float GetLookAtRotation(Vector2 center, Vector2 target) {
        Vector2 direction = target - center;
        float angle = Mathf.Atan2(direction.y, direction.x);
        angle = Mathf.Rad2Deg * Mathf.Repeat(angle, Mathf.PI * 2);

        return angle;
    }

    private long ReturnUnixTimeInSeconds() {
        DateTime currentTime = DateTime.UtcNow;
        return ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    }

    private void UpdateKeyObject(GameObject newKeyPressedGameObject, string keyType) {
        if (keyType == "Normal") {
            newKeyPressedGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else if (keyType == "Blurred") {
            newKeyPressedGameObject.GetComponent<Image>().color = new Color(1, 1, 1, .115f);
        }
        else if (keyType == "Highlighted") {
            newKeyPressedGameObject.GetComponent<Image>().color = new Color(0.1415093f, 0.5647725f, 1, 1);
        }
        else {
            newKeyPressedGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }

    private GameObject CreateKeyObject() {
        GameObject newKeyPressedGameObject = Instantiate(KeyTemplateObject, KeyParentObject.transform);
        newKeyPressedGameObject.transform.SetSiblingIndex(99);
        newKeyPressedGameObject.SetActive(true);

        return newKeyPressedGameObject;
    }

    public void HandlePlayerTurn(string Username, long TimeStarted, string RandomCharacters) {
        foreach (var objectKey in KeysPressedGameObjects) Destroy(objectKey);
        foreach (var objectKey2 in GivenKeysObjects) Destroy(objectKey2);
        KeysPressedGameObjects.Clear();
        GivenKeysObjects.Clear();

        KeysPressed.Clear();
        GivenKeys.Clear();
        ForcedUsedKey.Clear();

        LoadingScreenElement.SetActive(false);

        List<Vector2> generatedPoints = GeneratePointsAround(new Vector2(0, 0), 4.2464f, PlayerIcons.Count);
        int keyIndexOf = PlayerIcons.Keys.ToList().IndexOf(Username);

        TurnPlayGameObject.GetComponent<TMP_Text>().text = $"{Username}, type an English word containing:";
        TimerCounterInt = TimeStarted;

        LeanTween.cancel(ArrowObject);
        LeanTween.rotateZ(ArrowObject, GetLookAtRotation(new Vector2(0, 0), generatedPoints[keyIndexOf]), .2f);
        
        foreach (var Character in RandomCharacters) GivenKeys.Add(Character.ToString());
        for (int i = 0; i < GivenKeys.Count; i++) {
            GameObject newKeyPressedGameObject = CreateKeyObject();
            newKeyPressedGameObject.name = GivenKeys[i];
            newKeyPressedGameObject.transform.Find("KeyText").GetComponent<TMP_Text>().text = GivenKeys[i];
            newKeyPressedGameObject.transform.SetSiblingIndex(999);

            UpdateKeyObject(newKeyPressedGameObject, "Blurred");
            GivenKeysObjects.Add(newKeyPressedGameObject);

            if (ForcedUsedKey.ContainsKey(GivenKeys[i])) {
                newKeyPressedGameObject.SetActive(false);
            } else newKeyPressedGameObject.SetActive(true);
        }
    }

    public void HandleKeyPressingObject(string key, bool isRefresh) {
        foreach (var objectKey in KeysPressedGameObjects) Destroy(objectKey);
        foreach (var objectKey in GivenKeysObjects) Destroy(objectKey);
        KeysPressedGameObjects.Clear();
        GivenKeysObjects.Clear();

        if (!isRefresh) {
            KeysPressed.Add(key);
        }

        for (int i = 0; i < KeysPressed.Count; i++) {
            GameObject newKeyPressedGameObject = CreateKeyObject();
            newKeyPressedGameObject.name = KeysPressed[i];
            newKeyPressedGameObject.transform.Find("KeyText").GetComponent<TMP_Text>().text = KeysPressed[i];

            if (GivenKeys.Contains(KeysPressed[i]) && GivenKeys.IndexOf(KeysPressed[i]) == ForcedUsedKey.Count) {
                ForcedUsedKey.Add(KeysPressed[i], i);
            } else UpdateKeyObject(newKeyPressedGameObject, "Normal");

            KeysPressedGameObjects.Add(newKeyPressedGameObject);
        }

        foreach (var forcedKeyData in ForcedUsedKey) {
            UpdateKeyObject(KeysPressedGameObjects[forcedKeyData.Value], "Highlighted");
        }

        for (int i = 0; i < GivenKeys.Count; i++) {
            GameObject newKeyPressedGameObject = CreateKeyObject();
            newKeyPressedGameObject.name = GivenKeys[i];
            newKeyPressedGameObject.transform.Find("KeyText").GetComponent<TMP_Text>().text = GivenKeys[i];
            newKeyPressedGameObject.transform.SetSiblingIndex(999);

            UpdateKeyObject(newKeyPressedGameObject, "Blurred");
            GivenKeysObjects.Add(newKeyPressedGameObject);

            if (ForcedUsedKey.ContainsKey(GivenKeys[i])) {
                newKeyPressedGameObject.SetActive(false);
            }
        }
    }

    public void RemoveAllKeysObjects() {
        KeysPressed.Clear();
        ForcedUsedKey.Clear();
        foreach (var keyObject in KeysPressedGameObjects) Destroy(keyObject);
        KeysPressedGameObjects.Clear();
        foreach (var objectKey in GivenKeysObjects) objectKey.SetActive(true);
    }

    public void RemoveOneKeyObject() {
        int intKeyToRemove = KeysPressed.Count-1;
        if (ForcedUsedKey.ContainsKey(KeysPressed[intKeyToRemove])) {
            ForcedUsedKey.Remove(KeysPressed[intKeyToRemove]);
        }

        foreach (var givenKeyObject in GivenKeysObjects) {
            if (givenKeyObject.name == KeysPressed[intKeyToRemove]) {
                givenKeyObject.SetActive(true);
            }
        }

        KeysPressed.RemoveAt(intKeyToRemove);

        Destroy(KeysPressedGameObjects[intKeyToRemove]);
        KeysPressedGameObjects.RemoveAt(intKeyToRemove);
    }

    public void TakePlayerDamage(string Username) {
        foreach (var playerHandler in FindObjectsOfType<PlayerHandler>()) {
            if (Username != playerHandler.GetUserName()) continue;
            playerHandler.playerClasses[Username].TakeDamage();
        }
    }

    public void ReviveAllUsers() {
        TimerCounterInt = ReturnUnixTimeInSeconds();
        foreach (var playerHandler in FindObjectsOfType<PlayerHandler>()) {
            foreach (var playerObject in playerHandler.playerClasses) {
                playerObject.Value.ResetLives();
            }
        }
    }

    public void AddWinToUser(string Username, int AmountOfWins) {
        if (PlayerTemplates[Username] == null) return;
        PlayerTemplates[Username].transform.Find("Wins").GetComponent<TMP_Text>().text = $"Wins: {AmountOfWins}";
    }

    public void HandleIntermission(string winner) {
        foreach (var playerHandler in FindObjectsOfType<PlayerHandler>()) {
            playerHandler.SetIntermissionScreen(winner);
        }
    }

    void Update() {
        HandleLoadingEffects();

       float timerText = Math.Clamp(10 - (ReturnUnixTimeInSeconds() - TimerCounterInt), 0f, 10f);
       TimerTextObject.GetComponent<TMP_Text>().text = timerText.ToString();
    }
}
