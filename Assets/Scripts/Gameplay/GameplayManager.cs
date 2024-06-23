using System.Collections;
using System.Collections.Generic;
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

    private List<GameObject> PlayerTemplates = new List<GameObject>();
    private Dictionary<string, GameObject> PlayerIcons = new Dictionary<string, GameObject>();

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
}
