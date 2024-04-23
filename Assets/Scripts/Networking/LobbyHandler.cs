using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyHandler : MonoBehaviour {
    private Lobby HostLobby;
    private float HeartbeatTimer;

    [Header("UserInterface")] 
    [SerializeField] private UI_Handler uiHandler;

    private async void Start() {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateLobby(string LobbyName, int MaxPlayers) {
        try {
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

            Debug.Log("Lobbies Found: " + queryResponse.Results.Count);
            foreach (Lobby ActiveLobby in queryResponse.Results) {
                Debug.Log(ActiveLobby.Name + " " + ActiveLobby.MaxPlayers);
            }
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinLobby(Lobby lobby) {
        await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
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
