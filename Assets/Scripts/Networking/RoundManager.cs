using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private List<string> UserList = new List<string>();
    [SerializeField] private GameplayManager GameplayManager;
    [SerializeField] private List<string> SearchList = new List<string>();
    [SerializeField] private List<string> DeadUsers = new List<string>();
    [SerializeField] private Dictionary<string, int> UserWins = new Dictionary<string, int>();
 
    private long clockTime = 0;
    [SerializeField] private bool gameStarted = false;
    private float timerGameStart = 10f;

    private float turnTimeReset = 10f;
    private float turnTime = 10f;

    private int currentPlayerIndex = 0;
    private string playersTurn;
    private bool hasGivenValidAnswer = false;
    private string givenRandomLetters;
    private bool canAcceptAnswer = true;
    bool overriteAPIResult = true;

    [SerializeField] private List<string> KeysPressedList = new List<string>();

    private long ReturnUnixTimeInSeconds() {
        DateTime currentTime = DateTime.UtcNow;
        return ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    }
    //IEnumerator StartGame(int secs) {
    //    yield return new WaitForSeconds(secs);
    //    gameStarted = true;
    //}

    public IEnumerator CallApi() {
        string word = string.Join("", KeysPressedList);
        
        if (overriteAPIResult) {
            canAcceptAnswer = true;
            hasGivenValidAnswer = true;
            turnTime = 0f;

            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:3000/wordExits/{word}");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError("API call failed: " + request.error);
            canAcceptAnswer = true;
            yield break; // Exit the coroutine on error
        }

        string jsonResponse = request.downloadHandler.text;
        bool apiResult = false; // Default value if parsing fails

        try {
            // Parse JSON response to extract the boolean value (adjust based on your API's response format)
            apiResult = jsonResponse == "true";
        }
        catch (System.Exception e) {
            Debug.LogError("Error parsing JSON response: " + e.Message);
        }

        // Use the apiResult value for your logic
        Debug.Log("API result: " + apiResult);
        canAcceptAnswer = true;

        if (apiResult == true) {
            hasGivenValidAnswer = true;
            turnTime = 0f;
        }
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

        UserWins.Add(Username, 0);

        AddUserTemplateClientRpc(playersEmpty);
        UpdatePlayerTemplatesClientRpc(playersEmpty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRPC(string Message, string Username) {
        CreateMessagePromptClientRpc(Message, Username);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UserDiedServerRPC(string Username) {
        DeadUsers.Add(Username);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddKeysPressedForUserServerRPC(string Username, KeyCode keyPressed) {
        if (Username != playersTurn) return;
        if (keyPressed == KeyCode.Return) { // Confirm your answer (Basically check if both letters are included in the word and the word exists)
            if (canAcceptAnswer != true) return;
            canAcceptAnswer = false;

            StartCoroutine(CallApi());

            KeysPressedList.Clear();
            RemoveAllPressedKeysClientRpc();
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

    [ClientRpc]
    public void ReviveAllUsersClientRpc() {
        GameplayManager.ReviveAllUsers();
    }


    [ClientRpc]
    public void IntermissionTimeAndStateClientRpc(string winner) {
        GameplayManager.HandleIntermission(winner);
    }

    [ClientRpc]
    public void AddWinToPlayerClientRpc(string Username, int AmountOfWins) {
        GameplayManager.AddWinToUser(Username, AmountOfWins);
    }

    // -- Index >> LOOP

    //void Start() {
    //    if (IsServer) StartCoroutine(StartGame(10));
    //}

    void Update() {
        timerGameStart = Math.Clamp(timerGameStart - Time.deltaTime, 0, 10);
        if (timerGameStart <= 0 && gameStarted == false) {
            gameStarted = true;
        }

        if (!gameStarted) return;
        if (!IsOwner) return;

        if ((ReturnUnixTimeInSeconds() - clockTime) >= turnTime) {
            clockTime = ReturnUnixTimeInSeconds();
            turnTime = turnTimeReset;

            try {
                KeysPressedList.Clear();
                RemoveAllPressedKeysClientRpc();

                if (UserList.Contains(playersTurn) && !hasGivenValidAnswer) {
                    // Deal Damage
                    Debug.Log($"Damage: {playersTurn}");
                    DamagePlayerClientRpc(playersTurn);
                }

                hasGivenValidAnswer = false;

                if (UserList[currentPlayerIndex] != null) {
                    playersTurn = UserList[currentPlayerIndex];
                    if (DeadUsers.Contains(playersTurn)) return;

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

        if ((UserList.Count - DeadUsers.Count) == 1) {
            gameStarted = false;
            timerGameStart = 10f;
            playersTurn = null;
            ReviveAllUsersClientRpc();

            string winner = "TWENTYONECHARACTERS";
            foreach (var playerName in UserList)
            {
                if (DeadUsers.Contains(playerName)) continue;
                winner = playerName;
            }
            DeadUsers.Clear();

            if (winner != null && UserWins[winner] != null) {
                UserWins[winner]++;
                AddWinToPlayerClientRpc(winner, UserWins[winner]);
            }

            IntermissionTimeAndStateClientRpc(winner);
        }
    }
}
