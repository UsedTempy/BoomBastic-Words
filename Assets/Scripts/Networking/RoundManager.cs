using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();


    [ServerRpc(RequireOwnership = false)]
    public void AddUserToListServerRPC(string Username) {
        if (UserList.Contains(Username)) return;
        UserList.Add(Username);
    }

    [ClientRpc]
    private void UpdateUserClientRpc(string Username) {
        Debug.Log(Username);
    }
}
