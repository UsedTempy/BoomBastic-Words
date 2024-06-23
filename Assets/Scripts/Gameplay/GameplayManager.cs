using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour {
    [Header("PlayerList")]
    [SerializeField] private GameObject PlayerTemplateContainer;
    [SerializeField] private GameObject PlayerTemplate;

    [Header("Chatbox")]
    [SerializeField] private GameObject InputFieldObject;
    [SerializeField] private GameObject SendMessageObject;
    [SerializeField] private GameObject ChatboxMessageObject;
    [SerializeField] private GameObject ChatboxMessageParent;


    private List<GameObject> PlayerTemplates = new List<GameObject>();

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
}
