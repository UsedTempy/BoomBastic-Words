using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MainNetworkingLoop : NetworkBehaviour {
    //private long clockTime = 0;
    //private float turnTime = 5f;

    //private RoundManager roundManager;

    //void Awake() {
    //    roundManager = FindObjectOfType<RoundManager>();
    //}

    //private long ReturnUnixTimeInSeconds() {
    //    DateTime currentTime = DateTime.UtcNow;
    //    return ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    //}

    //void Update() {
    //    if (!IsServer) return;
    //    if ((ReturnUnixTimeInSeconds() - clockTime) >= turnTime) {
    //        clockTime = ReturnUnixTimeInSeconds();

    //        roundManager.SetUserTurnServerRPC("User");
    //        Debug.Log("SERVER");
    //    }
    //}
}


