using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[System.Serializable]
public class LobbyClass {
    public Lobby Lobby;
    public GameObject LobbyTemplate;


    public LobbyClass(Lobby lobby, GameObject lobbyTemplate) {
        this.Lobby = lobby;
        this.LobbyTemplate = lobbyTemplate;
    }
}
