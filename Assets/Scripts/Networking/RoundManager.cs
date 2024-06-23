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

        string playersEmpty = "";
        for (int i = 0; i < UserList.Count; i++) {
            string userName = UserList[i];
            if (i-1 == Username.Length) {
                playersEmpty += userName + ",";
            } else playersEmpty += userName + ",";
        }

        AddUserTemplateClientRpc(playersEmpty);
    }

    [ClientRpc]
    private void AddUserTemplateClientRpc(string userList) {
        GameplayManager.AddUserTemplate(userList);
    }
}
