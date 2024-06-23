using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour {
    [SerializeField] private GameObject PlayerTemplateContainer;
    [SerializeField] private GameObject PlayerTemplate;
    [SerializeField] private GameObject InputFieldObject;
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
        return InputFieldObject.GetComponent<TMP_InputField>().text;
    }
}
