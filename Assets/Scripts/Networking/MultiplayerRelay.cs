using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerRelay : MonoBehaviour {
    //begin chatgbt
    private static MultiplayerRelay _instance;

    public static MultiplayerRelay Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<MultiplayerRelay>();
                if (_instance == null) {
                    GameObject obj = new GameObject();
                    obj.name = typeof(MultiplayerRelay).Name;
                    _instance = obj.AddComponent<MultiplayerRelay>();
                }
            }
            return _instance;
        }
    }
    //einde chatgbt

    public async Task<string> CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);


            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
            return null;
        }

    }
    public async void JoinRelay(string joinCode) {
        try {
            Debug.Log("Joining relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }
}