using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InfoTemplate : MonoBehaviour {
    public string playerID;
    public GameObject Template;

    public InfoTemplate(string playerid, GameObject template) {
        this.playerID = playerid;
        this.Template = template;
    }
}
