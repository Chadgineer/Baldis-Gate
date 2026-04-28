using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LobbyUI : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public GameObject waitingCanvas; // "Oyuncu bekleniyor" objesi
    public TextMeshProUGUI roomCodeText; // "Oda kodu" yazan TMP

    void Start()
    {
        // Baþlangýçta aktif et
        waitingCanvas.SetActive(true);

        // Host ise kodu yazdýr
        if (NetworkManager.Singleton.IsHost)
        {
            roomCodeText.text = "Oda Kodu: " + "..."; 
        }
        else
        {
            roomCodeText.gameObject.SetActive(false); 
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            waitingCanvas.SetActive(false);
            roomCodeText.gameObject.SetActive(false);

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}