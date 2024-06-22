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
        Username = $"User_{Mathf.Floor(Random.Range(0f, 9999f))}";

        RoundManager.AddUserToListServerRPC(Username);
    }
}
