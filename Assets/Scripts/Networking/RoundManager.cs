using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();
    [SerializeField] private GameplayManager GameplayManager;
    [SerializeField] private List<string> SearchList = new List<string>();

    private long clockTime = 0;

    private float turnTimeReset = 10f;
    private float turnTime = 10f;

    private int currentPlayerIndex = 0;
    private string playersTurn;

    private List<string> KeysPressedList = new List<string>();

    private long ReturnUnixTimeInSeconds() {
        DateTime currentTime = DateTime.UtcNow;
        return ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddUserToListServerRPC(string Username) {
        if (UserList.Contains(Username)) return;
        UserList.Add(Username);

        string playersEmpty = "";
        for (int i = 0; i < UserList.Count; i++) {
            string userName = UserList[i];
            if (i == UserList.Count - 1) {
                playersEmpty += userName;
            }
            else playersEmpty += userName + ",";
        }

        AddUserTemplateClientRpc(playersEmpty);
        UpdatePlayerTemplatesClientRpc(playersEmpty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRPC(string Message, string Username) {
        CreateMessagePromptClientRpc(Message, Username);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddKeysPressedForUserServerRPC(string Username, KeyCode keyPressed) {
        Debug.Log(Username);
        Debug.Log(playersTurn);
        if (Username == playersTurn) return;
        Debug.Log("123123");
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerLivesServerRPC(string Username, int Lives) {
        UpdatePlayerLivesClientRpc(Username, Lives);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetUserTurnServerRPC(string newSelectedUser, long TimeSinceTurnStated, string RandomCharacters) {
        HandleUserTurnClientRpc(newSelectedUser, TimeSinceTurnStated, RandomCharacters);
    }


    [ClientRpc]
    private void AddUserTemplateClientRpc(string userList) {
        GameplayManager.AddUserTemplate(userList);
    }

    [ClientRpc]
    private void CreateMessagePromptClientRpc(string Message, string Username) {
        GameplayManager.CreateChatMessage(Username, Message);
    }

    [ClientRpc]
    private void UpdatePlayerTemplatesClientRpc(string userList) {
        GameplayManager.UpdatePlayerTemplates(userList);
    }

    [ClientRpc]
    private void UpdatePlayerLivesClientRpc(string Username, int Lives) {
        GameplayManager.UpdateLives(Username, Lives);
    }

    [ClientRpc]
    private void HandleUserTurnClientRpc(string newSelectedUser, long TimeSinceTurnStated, string RandomCharacters) {
        GameplayManager.HandlePlayerTurn(newSelectedUser, TimeSinceTurnStated, RandomCharacters);
    }




    // -- Index >> LOOP
    void Update() {
        if (!IsOwnedByServer) return;
        if ((ReturnUnixTimeInSeconds() - clockTime) >= turnTime) {
            clockTime = ReturnUnixTimeInSeconds();
            turnTime = turnTimeReset;

            try {
                if (UserList[currentPlayerIndex] != null) {
                    playersTurn = UserList[currentPlayerIndex];
                    string RandomCharacters = SearchList[Random.Range(0, SearchList.Count)];
                    SetUserTurnServerRPC(UserList[currentPlayerIndex], ReturnUnixTimeInSeconds(), RandomCharacters);
                }
            } catch {
                Debug.Log($"ERROR: {currentPlayerIndex}");
            }

            currentPlayerIndex++;
            if (currentPlayerIndex >= UserList.Count) currentPlayerIndex = 0;
        }
    }
}
