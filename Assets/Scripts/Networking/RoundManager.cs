using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();

    void Start() {
        GetComponent<NetworkManager>().ConnectionApprovalCallback = ConnectionApprovalCallback;
    }

    void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        response.Approved = true;
        response.CreatePlayerObject = true;

        AddUserToListServerRpc("Test");
    }


    [ServerRpc] 
    public void AddUserToListServerRpc(string Username) {
        if (UserList.Contains(Username)) return;
        UserList.Add(Username);

        Debug.Log(string.Format("Player with the username: %s joined!", Username));
    }

    [ClientRpc] 
    private void UpdateUserClientRpc() {
        
    }
}
