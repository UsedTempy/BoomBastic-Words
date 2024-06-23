using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClass : MonoBehaviour {
    private string Username;
    private int Lives = 2;
    private GameObject playerObject;

    public PlayerClass(string _username) {
        this.Username = _username;
    }

    public void SetPlayerObject(GameObject _playerGameObject) {
        this.playerObject = _playerGameObject;
    }

    public void TakeDamage() {
        if (this.Lives <= 0) return;
        this.Lives--;
    }

    public void ResetLives() {
        this.Lives = 2;
    }
}
