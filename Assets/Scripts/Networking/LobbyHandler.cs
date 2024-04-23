using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = System.Random;

public class LobbyHandler : MonoBehaviour {
    private Lobby HostLobby;
    private Lobby JoinedLobby;

    private float HeartbeatTimer;
    private float LobbyHeartbeatTimer;

    private string playerName = "Tempy" + new Random().Next(1, 999);

    [Header("UserInterface")] 
    [SerializeField] private UI_Handler uiHandler;

    private async void Start() {
        await UnityServices.InitializeAsync();

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

            Lobby ActiveLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers);
            Debug.Log("Created Lobby! " + ActiveLobby.Name + " " + ActiveLobby.MaxPlayers);

            HostLobby = ActiveLobby;
            uiHandler.SetLoadingState(false);
            uiHandler.SetUserInterfaceState("GameContent", true);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }   
    }

    private async void ListLobbies() {
        try {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Count = 9,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lists: " + queryResponse.Results.Count);

            foreach (Lobby ActiveLobby in queryResponse.Results) {
                Debug.Log(ActiveLobby.Name + " " + ActiveLobby.MaxPlayers);
            }
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinLobby(Lobby lobby) {
        JoinLobbyByIdOptions JoinLobbyOptions = new JoinLobbyByIdOptions {
            Player = GetPlayer(),
        };

        Lobby NewJoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, JoinLobbyOptions);
        JoinedLobby = NewJoinedLobby;
    }

    public async void LeaveLobby() {
        try {
            if (HostLobby != null) {
                MigrateLobbyHost();
                LobbyService.Instance.RemovePlayerAsync(HostLobby.Id, AuthenticationService.Instance.PlayerId);
            } else {
                LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
           
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

    private async void MigrateLobbyHost() {
        try {
            HostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                HostId = JoinedLobby.Players[1].Id
            });
            JoinedLobby = HostLobby;
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
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { "Wins", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "1") }
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

    private void Update() {
        HandleLobbyHeartbeat();
    }
}
