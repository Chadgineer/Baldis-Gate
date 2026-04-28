using UnityEngine;
using Unity.Netcode;

public class NetworkManagerPersistent : MonoBehaviour
{
    void Awake()
    {
        // Bu obje sahne deðiþse de yok olmaz
        DontDestroyOnLoad(gameObject);

        // Eðer zaten bir NetworkManager varsa (hata olmasýn diye), bunu yok et
        if (FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
    }
}