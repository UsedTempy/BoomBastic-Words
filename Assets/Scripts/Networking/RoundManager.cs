using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();
    [SerializeField] private GameplayManager GameplayManager;
    [SerializeField] private List<string> SearchList = new List<string>();

    private long clockTime = 0;

    private float turnTimeReset = 10f;
    private float turnTime = 10f;

    private int currentPlayerIndex = 0;
    private string playersTurn;
    private bool hasGivenValidAnswer = false;
    private string givenRandomLetters;
    private bool canAcceptAnswer = true;

    [SerializeField] private List<string> KeysPressedList = new List<string>();

    private long ReturnUnixTimeInSeconds() {
        DateTime currentTime = DateTime.UtcNow;
        return ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    }

    public IEnumerator CallApi() {
        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:3000/wordExits/{givenRandomLetters}");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError("API call failed: " + request.error);
            yield break; // Exit the coroutine on error
        }

        string jsonResponse = request.downloadHandler.text;
        bool apiResult = false; // Default value if parsing fails

        try {
            // Parse JSON response to extract the boolean value (adjust based on your API's response format)
            Dictionary<string, bool> data = JsonUtility.FromJson<Dictionary<string, bool>>(jsonResponse);
            apiResult = data["success"]; // Assuming the key for the boolean result is "success"
        }
        catch (System.Exception e) {
            Debug.LogError("Error parsing JSON response: " + e.Message);
        }

        // Use the apiResult value for your logic
        Debug.Log("API result: " + apiResult);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUserToListServerRPC(string Username) {
        if (UserList.Contains(Username)) return;
        UserList.Add(Username);

        string playersEmpty = "";
        for (int i = 0; i < UserList.Count; i++) {
            string userName = UserList[i];
            if (i == UserList.Count - 1) {
                playersEmpty += userName;
            }
            else playersEmpty += userName + ",";
        }

        AddUserTemplateClientRpc(playersEmpty);
        UpdatePlayerTemplatesClientRpc(playersEmpty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRPC(string Message, string Username) {
        CreateMessagePromptClientRpc(Message, Username);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddKeysPressedForUserServerRPC(string Username, KeyCode keyPressed) {
        if (Username != playersTurn) return;
        if (keyPressed == KeyCode.Return) { // Confirm your answer (Basically check if both letters are included in the word and the word exists)
            if (canAcceptAnswer != true) return;
            canAcceptAnswer = false;

            KeysPressedList.Clear();
            RemoveAllPressedKeysClientRpc();

            StartCoroutine(CallApi());
            
            hasGivenValidAnswer = true;
            turnTime = 0f;
        } else if (keyPressed == KeyCode.Backspace) { // Remove the last character put in
            if (KeysPressedList.Count == 0) return;
            KeysPressedList.RemoveAt(KeysPressedList.Count-1);
            RemoveOnePressedKeyClientRpc();
        } else { // Any key that is pressed (Make sure it is only up to a certain amount)
            if (KeysPressedList.Count >= 15) return;
            KeysPressedList.Add(keyPressed.ToString());
            SetPressedKeyObjectClientRpc(keyPressed.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerLivesServerRPC(string Username, int Lives) {
        UpdatePlayerLivesClientRpc(Username, Lives);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetUserTurnServerRPC(string newSelectedUser, long TimeSinceTurnStated, string RandomCharacters) {
        HandleUserTurnClientRpc(newSelectedUser, TimeSinceTurnStated, RandomCharacters);
    }

    [ClientRpc]
    private void AddUserTemplateClientRpc(string userList) {
        GameplayManager.AddUserTemplate(userList);
    }

    [ClientRpc]
    private void CreateMessagePromptClientRpc(string Message, string Username) {
        GameplayManager.CreateChatMessage(Username, Message);
    }

    [ClientRpc]
    private void UpdatePlayerTemplatesClientRpc(string userList) {
        GameplayManager.UpdatePlayerTemplates(userList);
    }

    [ClientRpc]
    private void UpdatePlayerLivesClientRpc(string Username, int Lives) {
        GameplayManager.UpdateLives(Username, Lives);
    }

    [ClientRpc]
    private void HandleUserTurnClientRpc(string newSelectedUser, long TimeSinceTurnStated, string RandomCharacters) {
        GameplayManager.HandlePlayerTurn(newSelectedUser, TimeSinceTurnStated, RandomCharacters);
    }

    [ClientRpc]
    private void SetPressedKeyObjectClientRpc(string keyPressed) {
        GameplayManager.HandleKeyPressingObject(keyPressed, false);
    }

    [ClientRpc]
    private void RemoveAllPressedKeysClientRpc() {
        GameplayManager.RemoveAllKeysObjects();
    }

    [ClientRpc]
    private void RemoveOnePressedKeyClientRpc() {
        GameplayManager.RemoveOneKeyObject();
    }

    [ClientRpc]
    private void DamagePlayerClientRpc(string Username) {
        GameplayManager.TakePlayerDamage(Username);
    }


    // -- Index >> LOOP
    void Update() {
        if (!IsOwnedByServer) return;
        if ((ReturnUnixTimeInSeconds() - clockTime) >= turnTime) {
            clockTime = ReturnUnixTimeInSeconds();
            turnTime = turnTimeReset;

            try {
                KeysPressedList.Clear();
                RemoveAllPressedKeysClientRpc();

                if (UserList.Contains(playersTurn) && !hasGivenValidAnswer) {
                    // Deal Damage
                    DamagePlayerClientRpc(playersTurn);
                }

                hasGivenValidAnswer = false;

                if (UserList[currentPlayerIndex] != null) {
                    playersTurn = UserList[currentPlayerIndex];

                    string RandomCharacters = SearchList[Random.Range(0, SearchList.Count)];
                    givenRandomLetters = RandomCharacters;

                    SetUserTurnServerRPC(UserList[currentPlayerIndex], ReturnUnixTimeInSeconds(), RandomCharacters);
                }
            } catch {
                Debug.Log($"ERROR: {currentPlayerIndex}");
            }

            currentPlayerIndex++;
            if (currentPlayerIndex >= UserList.Count) currentPlayerIndex = 0;
        }
    }
}
