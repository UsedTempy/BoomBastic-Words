using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour {
    [SerializeField] private GameObject PlayerTemplateContainer;
    [SerializeField] private GameObject PlayerTemplate;
    private List<GameObject> PlayerTemplates = new List<GameObject>();

    public void AddUserTemplate(List<string> ActivePlayers) {
        foreach (GameObject PlayerTemplate in PlayerTemplates) {
            Destroy(PlayerTemplate);
        }

        foreach (string Username in ActivePlayers) {
            GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateContainer.transform);
            NewPlayerTemplate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
            NewPlayerTemplate.SetActive(true);
        }
    }
}
