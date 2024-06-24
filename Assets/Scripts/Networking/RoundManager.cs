using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();
    [SerializeField] private GameplayManager GameplayManager;

    private long clockTime = 0;
    private float turnTime = 5f;
    private int currentPlayerIndex = 0;

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
    public void UpdatePlayerLivesServerRPC(string Username, int Lives) {
        UpdatePlayerLivesClientRpc(Username, Lives);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetUserTurnServerRPC(string newSelectedUser) {
        HandleUserTurnClientRpc(newSelectedUser);
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
    private void HandleUserTurnClientRpc(string newSelectedUser) {
        GameplayManager.HandlePlayerTurn(newSelectedUser);
    }




    // -- Index >> LOOP
    void Update() {
        if (!IsServer) return;
        if ((ReturnUnixTimeInSeconds() - clockTime) >= turnTime) {
            clockTime = ReturnUnixTimeInSeconds();
            turnTime = 5f;

            try {
                if (UserList[currentPlayerIndex] != null) {
                    SetUserTurnServerRPC(UserList[currentPlayerIndex]);
                }
            } catch {
                Debug.Log($"ERROR: {currentPlayerIndex}");
            }

            currentPlayerIndex++;
            if (currentPlayerIndex >= UserList.Count) currentPlayerIndex = 0;
        }
    }
}
