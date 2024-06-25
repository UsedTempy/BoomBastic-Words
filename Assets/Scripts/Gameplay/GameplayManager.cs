using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
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


    private List<GameObject> PlayerTemplates = new List<GameObject>();
    private Dictionary<string, GameObject> PlayerIcons = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> KeyDictioary = new Dictionary<string, GameObject>();
    private long TimerCounterInt = 0;

    public void AddUserTemplate(string userNamesList) {
        foreach (var playerTemplate in PlayerTemplates) {
            Destroy(playerTemplate);
        }

        foreach (var Username in userNamesList.Split(',')) {
            GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateContainer.transform);
            NewPlayerTemplate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
            NewPlayerTemplate.SetActive(true);

            PlayerTemplates.Add(NewPlayerTemplate);
        }
    }

    public string ReturnMessageField() {
        string InputField = InputFieldObject.GetComponent<TMP_InputField>().text;
        InputFieldObject.GetComponent<TMP_InputField>().text = "";

        return InputField;
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

    public void HandlePlayerTurn(string Username, long TimeStarted, string RandomCharacters) {
        foreach (var data in KeyDictioary) Destroy(data.Value);
        KeyDictioary.Clear();

        List<Vector2> generatedPoints = GeneratePointsAround(new Vector2(0, 0), 4.2464f, PlayerIcons.Count);
        int keyIndexOf = PlayerIcons.Keys.ToList().IndexOf(Username);

        TurnPlayGameObject.GetComponent<TMP_Text>().text = $"{Username}, type an English word containing:";
        TimerCounterInt = TimeStarted;

        LeanTween.cancel(ArrowObject);
        LeanTween.rotateZ(ArrowObject, GetLookAtRotation(new Vector2(0, 0), generatedPoints[keyIndexOf]), .2f);

        foreach (var Character in RandomCharacters) {
            GameObject newKeyPressedGameObject = Instantiate(KeyTemplateObject, KeyParentObject.transform);
            newKeyPressedGameObject.transform.SetSiblingIndex(2);
            newKeyPressedGameObject.name = Character.ToString();
            newKeyPressedGameObject.transform.Find("KeyText").GetComponent<TMP_Text>().text = Character.ToString();
            newKeyPressedGameObject.SetActive(true);

            KeyDictioary.Add(Character.ToString(), newKeyPressedGameObject);
        }
    }

    void Update() {
       float timerText = Math.Clamp(10 - (ReturnUnixTimeInSeconds() - TimerCounterInt), 0f, 10f);
       TimerTextObject.GetComponent<TMP_Text>().text = timerText.ToString();
    }
}
