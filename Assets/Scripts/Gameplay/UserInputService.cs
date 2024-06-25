using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UserInputService : NetworkBehaviour {
    void Awake() {
        if (!IsOwner) return;
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKey(kcode)) {
                Debug.Log("KeyCode down: " + kcode);
            }
        }
    }
}
