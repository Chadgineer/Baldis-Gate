using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    [Header("UI & Settings")]
    public TMP_InputField codeInputField;
    public string sceneToLoad = "GameScene"; // Inspector'dan oyun sahnenein adını yaz

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void HostGame()
    {
        // 1. Relay Odası Aç (Max 2 kişi)
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("Oda Kodu: " + joinCode);
        if (codeInputField != null) codeInputField.text = joinCode;

        // 2. Transport'u Relay'e bağla
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        // 3. Host ol ve Sahneye geç
        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }

    public async void JoinGame()
    {
        string joinCode = codeInputField.text;
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        // 2. Transport'u Relay'e bağla
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        // 3. Client olarak katıl
        NetworkManager.Singleton.StartClient();
    }
}