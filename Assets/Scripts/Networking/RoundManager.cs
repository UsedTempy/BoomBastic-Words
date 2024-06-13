using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private UI_Handler uiHandler;

    [ServerRpc] public void AddUserTemplateServerRpc() {

    }

    [ClientRpc] private void UpdateUserClientRpc() {

    }
}
