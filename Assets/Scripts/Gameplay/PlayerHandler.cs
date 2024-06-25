using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerHandler : NetworkBehaviour {
    [SerializeField] private string Username;
    [SerializeField] private RoundManager RoundManager;
    [SerializeField] private GameplayManager GameplayManager;
    private Dictionary<string, PlayerClass> playerClasses = new Dictionary<string, PlayerClass>();

    // Start is called before the first frame update
    void Start() {
        if (!IsOwner) return;
        RoundManager = FindObjectOfType<RoundManager>();
        GameplayManager = FindObjectOfType<GameplayManager>();
        Username = $"User_{Mathf.Floor(Random.Range(1000, 9999))}";

        PlayerClass newPlayerClass = new PlayerClass(Username, RoundManager);
        playerClasses.Add(Username, newPlayerClass);

        RoundManager.AddUserToListServerRPC(Username);
        newPlayerClass.ResetLives();

        GameObject SendButton = GameObject.FindGameObjectWithTag("SendButton");
        if (SendButton) {
            SendButton.GetComponent<Button>().onClick.AddListener(SendMessage);
        }
    }

    public string GetUserName() {
        if (!IsOwner) return "TWENTYONECHARACTERS";
        return Username;
    }

    public void SendMessage() {
        if (!IsOwner) return;
        string Field = GameplayManager.ReturnMessageField();
        RoundManager.SendMessageServerRPC(Field, GetUserName());
    }

    void Update() {
        if (!IsOwner) return;
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(kcode)) {
                if (kcode >= KeyCode.Mouse0 && kcode <= KeyCode.Mouse6) continue;
                if (kcode == KeyCode.LeftAlt || kcode == KeyCode.RightAlt || kcode == KeyCode.LeftControl || kcode == KeyCode.RightControl) continue;
                if (kcode == KeyCode.LeftApple || kcode == KeyCode.RightApple || kcode == KeyCode.LeftWindows || kcode == KeyCode.RightWindows) continue;
                if (kcode == KeyCode.UpArrow || kcode == KeyCode.LeftArrow || kcode == KeyCode.RightArrow || kcode == KeyCode.DownArrow) continue;
                if (kcode == KeyCode.LeftShift || kcode == KeyCode.RightShift) continue;

                bool isValid = char.IsLetter((char)kcode) || kcode == KeyCode.Return || kcode == KeyCode.Backspace; // Check for letters or Enter key

                if (isValid) {
                    RoundManager.AddKeysPressedForUserServerRPC(GetUserName(), kcode);
                }
            }
        }
    }
}
