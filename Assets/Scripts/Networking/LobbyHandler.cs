using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class LobbyHandler : NetworkBehaviour {
    public Lobby HostLobby;
    public Lobby JoinedLobby;

    private float HeartbeatTimer;
    private float LobbyHeartbeatTimer;
    private float HostLobbyHeartbeatTimer;
    
    public const string KEY_START_GAME = "StartGame";

    private string playerName = "User_" + new Random().Next(1, 9999);
    public List<string> activePlayers = new List<string>();

    [Header("UserInterface")] 
    [SerializeField] private UI_Handler uiHandler;
    [SerializeField] private RoundManager roundManager;

    private async void Start() {
        InitializationOptions initOptions = new InitializationOptions();
        initOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initOptions);

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void RefreshLobbyList() {
        try {
            QueryLobbiesOptions Options = new QueryLobbiesOptions();
            Options.Count = 9;

            Options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            Options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse LobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies Found: " + LobbyListQueryResponse.Results.Count);

            uiHandler.DisplayServerLobbies(LobbyListQueryResponse);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void CreateLobby(string LobbyName, int MaxPlayers) {
        try {
            CreateLobbyOptions LobbyOptionData = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby ActiveLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, LobbyOptionData);
            Debug.Log("Created Lobby! " + ActiveLobby.Name + " " + ActiveLobby.MaxPlayers);

            HostLobby = ActiveLobby;
            uiHandler.SetLoadingState(false);
            uiHandler.SetUserInterfaceState("GameContent", true);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }   
    }

    public async void JoinLobby(Lobby lobby) {
        Debug.Log(lobby.Id);
        Player player = GetPlayer();

        JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        roundManager.AddUserTemplateServerRpc();
        uiHandler.SetUserInterfaceState("GameContent", true);
    }

    public async void LeaveLobby() {
        try {
            if (HostLobby != null) {
                LobbyService.Instance.RemovePlayerAsync(HostLobby.Id, AuthenticationService.Instance.PlayerId);
                HostLobby = null;
            }
            
            if (JoinedLobby != null) {
                LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
                JoinedLobby = null;
            }

            RefreshLobbyList();
            uiHandler.SetUserInterfaceState("ServerList", true);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void KickPlayer() {
        try {
            LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby(){
        try {
            await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private Player GetPlayer() {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }






    private async void HandleLobbyPollUpdates() {
        if (JoinedLobby != null) {
            LobbyHeartbeatTimer -= Time.deltaTime;
            if (LobbyHeartbeatTimer <= 0f) {
                float LobbyHeartbeatTimerMax = 1.1f;
                LobbyHeartbeatTimer = LobbyHeartbeatTimerMax;

                Lobby queryLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                JoinedLobby = queryLobby;

                uiHandler.UpdateLobbyPlayerTemplates();

                if (JoinedLobby.Data["StartGame"].Value != null && JoinedLobby.Data["StartGame"].Value != "0") {
                    Debug.Log(JoinedLobby.Data["StartGame"].Value);
                    //start game
                    if (!IsLobbyHost()) {
                        MultiplayerRelay.Instance.JoinRelay(JoinedLobby.Data[KEY_START_GAME].Value);
                    }
                    JoinedLobby = null;

                    //OnGameStarted?.Invoke(this, new LobbyEventArgs { lobby = JoinedLobby });
                }
            }
        }
    }

    public bool IsLobbyHost() {
        return HostLobby != null && HostLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    private async void HandleHostLobbyPollUpdates() {
        if (HostLobby != null) {
            HostLobbyHeartbeatTimer -= Time.deltaTime;
            if (HostLobbyHeartbeatTimer <= 0f) {
                float HostLobbyHeartbeatTimerMax = 1.1f;
                HostLobbyHeartbeatTimer = HostLobbyHeartbeatTimerMax;

                Lobby queryLobby = await LobbyService.Instance.GetLobbyAsync(HostLobby.Id);
                HostLobby = queryLobby;

                uiHandler.UpdateLobbyPlayerTemplates();
            }
        }
    }

    private async void HandleLobbyHeartbeat() {
        if (HostLobby != null) {
            HeartbeatTimer -= Time.deltaTime;
            if (HeartbeatTimer <= 0f) {
                float HeartbeatTimerMax = 20f;
                HeartbeatTimer = HeartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }

    public async void StartGame() {
        if (IsLobbyHost()) {
            try {
                GoToGameScene();

                Debug.Log("StartGame");

                string relayCode = await MultiplayerRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });

                HostLobby = lobby;
            }
            catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    private void GoToGameScene() {
        SceneManager.LoadScene("MainGameplay");
    }

    private void Update() {
        HandleLobbyHeartbeat();
        HandleLobbyPollUpdates();
        HandleHostLobbyPollUpdates();
    }
}
