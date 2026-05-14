using UnityEngine;
using Unity.Netcode;

public class RayManager : NetworkBehaviour
{
    [Header("Settings")]
    public float maxDistance = 15f;
    public LineRenderer rayLine;
    public LayerMask enemyLayer;
    public float rayWidth = 0.2f;

    [Header("Stats")]
    public float minMoveSpeed = 2f;
    public float maxMoveSpeed = 8f;
    public float minDamage = 5f;
    public float maxDamage = 25f;

    private PlayerMovement player1;
    private PlayerMovement player2;
    private bool isRayActive = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
        CheckForPlayers();
    }

    private void HandleClientConnected(ulong id)
    {
        CheckForPlayers();
    }

    private void CheckForPlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        // Oyuncu arama durumu logu
        Debug.Log($"[RayManager] Oyuncu kontrolü yapılıyor... Bulunan oyuncu sayısı: {players.Length}");

        if (players.Length >= 2)
        {
            player1 = players[0].GetComponent<PlayerMovement>();
            player2 = players[1].GetComponent<PlayerMovement>();

            isRayActive = true;
            rayLine.enabled = true;

            // BAĞLANTI BAŞARILI LOGU
            Debug.Log("<color=green>[RayManager] İKİ OYUNCU SAPTANDI! Işın sistemi aktif edildi.</color>");
            Debug.Log($"[RayManager] Oyuncu 1: {player1.name}, Oyuncu 2: {player2.name}");
        }
        else
        {
            Debug.Log("[RayManager] Henüz yeterli oyuncu yok, ışın bekleniyor...");
        }
    }

    void Update()
    {
        if (!isRayActive || player1 == null || player2 == null) return;

        float dist = Vector2.Distance(player1.transform.position, player2.transform.position);
        dist = Mathf.Clamp(dist, 0, maxDistance);

        rayLine.SetPosition(0, player1.transform.position);
        rayLine.SetPosition(1, player2.transform.position);

        float ratio = dist / maxDistance;
        float currentSpeed = Mathf.Lerp(maxMoveSpeed, minMoveSpeed, ratio);
        float currentDamage = Mathf.Lerp(minDamage, maxDamage, ratio);

        Color rayColor = Color.Lerp(Color.cyan, Color.red, ratio);
        rayLine.startColor = rayColor;
        rayLine.endColor = rayColor;

        player1.moveSpeed = currentSpeed;
        player2.moveSpeed = currentSpeed;

        if (IsServer)
        {
            HandleCollision(currentDamage);
        }
    }

    private void HandleCollision(float damage)
    {
        Vector2 start = player1.transform.position;
        Vector2 end = player2.transform.position;
        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, rayWidth, dir, dist, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<HealthManager>(out var health))
            {
                health.TakeDamageServerRpc(damage * Time.deltaTime);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}