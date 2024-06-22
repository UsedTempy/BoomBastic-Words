using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] private string Username;
    [SerializeField] private RoundManager RoundManager;

    // Start is called before the first frame update
    void Start() {
        RoundManager = FindObjectOfType<RoundManager>();
        Username = "Test";

        RoundManager.AddUserToListServerRPC(Username);
    }

    // Update is called once per frame
    void Update() {
        
    }
}
