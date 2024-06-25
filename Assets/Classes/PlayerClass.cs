using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClass : MonoBehaviour {
    private string Username;
    private int Lives = 2;
    private RoundManager RoundManager;

    public PlayerClass(string _username, RoundManager _roundManager) {
        this.Username = _username;
        this.RoundManager = _roundManager;
    }

    public void TakeDamage() {
        if (this.Lives <= 0) return;
        this.Lives--;

        //RoundManager.UpdatePlayerLivesServerRPC(this.Username, this.Lives);
        if (this.Lives == 0) RoundManager.UserDiedServerRPC(this.Username);
    }

    public void ResetLives() {
        this.Lives = 2;
        //RoundManager.UpdatePlayerLivesServerRPC(this.Username, this.Lives);
    }
}
