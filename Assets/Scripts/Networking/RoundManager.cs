using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();
    [SerializeField] private GameplayManager GameplayManager;


    [ServerRpc(RequireOwnership = false)]
    public void AddUserToListServerRPC(string Username) {
        if (UserList.Contains(Username)) return;
        UserList.Add(Username);

        AddUserTemplateClientRpc(Username);
    }

    [ClientRpc]
    private void AddUserTemplateClientRpc(string Username) {
        GameplayManager.AddUserTemplate(Username);
    }
}
