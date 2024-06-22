using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour {
    [SerializeField] private GameObject PlayerTemplateContainer;
    [SerializeField] private GameObject PlayerTemplate;

    public void AddUserTemplate(string Username) {
        GameObject NewPlayerTemplate = Instantiate(PlayerTemplate, PlayerTemplateContainer.transform);
        NewPlayerTemplate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = Username;
        NewPlayerTemplate.SetActive(true);
    }
}
