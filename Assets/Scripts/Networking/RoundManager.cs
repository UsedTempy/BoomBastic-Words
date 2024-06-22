using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private UI_Handler uiHandler;

    [ServerRpc] 
    public void AddUserTemplateServerRpc() {
        Debug.Log("User Joined Server");
        UpdateUserClientRpc();
    }

    [ClientRpc] 
    private void UpdateUserClientRpc() {
        Debug.Log("Add User Template");
    }
}
