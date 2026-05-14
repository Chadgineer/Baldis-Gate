using UnityEngine;
using Unity.Netcode;

public class HealthManager : NetworkBehaviour
{
    // NetworkVariable kullanarak can değerini tüm clientlarda senkronize tutuyoruz
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f);

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] // Sahibi olmasak da (ışın değerince) hasar verebilmek için Everyone
    public void TakeDamageServerRpc(float damage)
    {
        // Hasar hesaplaması sadece Server'da yapılır
        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!IsServer) return;

        if (CompareTag("Player"))
        {
            // Oyuncu ölme mantığı: Örneğin başlangıç noktasına ışınla
            transform.position = Vector3.zero;
            currentHealth.Value = 100f; // Canı yenile
            Debug.Log("Oyuncu öldü ve respawn oldu.");
        }
        else
        {
            // Düşman ölme mantığı: Objeyi ağdan kaldır ve yok et
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
    }
}