using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField codeInputField;

    async void Start()
    {
        try
        {
            Debug.Log("[Multiplayer] Unity Servisleri başlatılıyor...");
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("[Multiplayer] Unity Services girişi başarılı. Oyuncu ID: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Multiplayer] Servis başlatma hatası: " + e.Message);
        }
    }

    public async void HostGame()
    {
        Debug.Log("[Multiplayer] Host başlatılıyor...");
        try
        {
            // 2 kişilik yer ayır
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("[Multiplayer] Relay Odası Oluşturuldu! KOD: " + joinCode);
            if (codeInputField != null) codeInputField.text = joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            bool success = NetworkManager.Singleton.StartHost();
            Debug.Log("[Multiplayer] Host başlatma sonucu: " + (success ? "BAŞARILI" : "BAŞARISIZ"));
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Multiplayer] Host hatası: " + e.Message);
        }
    }

    public async void JoinGame()
    {
        string joinCode = codeInputField.text;
        Debug.Log("[Multiplayer] '" + joinCode + "' kodu ile bağlanılmaya çalışılıyor...");

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("[Multiplayer] Kod boş olduğu için işlem iptal edildi.");
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("[Multiplayer] Relay sunucusuna bağlanıldı, veriler aktarılıyor...");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            bool success = NetworkManager.Singleton.StartClient();
            Debug.Log("[Multiplayer] Client (2. Oyuncu) başlatma sonucu: " + (success ? "BAŞARILI" : "BAŞARISIZ"));

            // Bağlantı durumunu takip et
            NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
                Debug.Log("[Multiplayer] BAĞLANTI TAMAMLANDI! Bağlanan Client ID: " + id);
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Multiplayer] Join hatası: " + e.Message);
        }
    }
}