using UnityEngine;
using UnityEngine.InputSystem;

// Pegar arma do chão.
// Com o jogador no punho, apertar E pega a arma mais perto que estiver a até pickupRange do centro.
// A arma some do chão e o ataque passa a usar os valores dela (alcance, arco e intervalo).
// Com uma arma na mão, o E não faz nada.
[RequireComponent(typeof(PlayerAttack))]
public class PlayerPickup : MonoBehaviour
{
    // Distância máxima para pegar uma arma, em unidades (1 unidade = 1 tile),
    // medida do centro do jogador.
    [SerializeField] private float pickupRange = 1f;

    // Ataque do jogador, que recebe os valores da arma pega
    private PlayerAttack playerAttack;

    // Máscara que contém só a camada das armas no chão
    private int weaponMask;

    private void Awake()
    {
        // O RequireComponent garante que o PlayerAttack existe no mesmo objeto
        playerAttack = GetComponent<PlayerAttack>();

        // Monta a máscara uma vez só, usando o nome da camada que está no Contracts.cs
        weaponMask = LayerMask.GetMask(Names.WeaponLayer);
    }

    private void Update()
    {
        // Leitura de input acontece em Update, que roda uma vez por frame
        Keyboard keyboard = Keyboard.current;

        // Sem teclado conectado: não faz nada
        if (keyboard == null)
        {
            return;
        }

        // wasPressedThisFrame só é verdadeiro no frame em que a tecla desce.
        // Por isso segurar o E não tenta pegar de novo a cada frame.
        if (!keyboard.eKey.wasPressedThisFrame)
        {
            return;
        }

        // Já está com uma arma na mão: o E não pega outra
        if (playerAttack.HasWeapon)
        {
            return;
        }

        TryPickup();
    }

    // Procura a arma mais perto dentro do alcance e, se achar, pega
    private void TryPickup()
    {
        // Centro do jogador = posição do objeto raiz
        Vector2 playerCenter = transform.position;

        // Pega todos os colisores da camada Weapon que tocam um círculo em volta do jogador.
        // Aqui os triggers NÃO são ignorados: o colisor da arma no chão é trigger.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerCenter, pickupRange, weaponMask);

        // A arma mais perto achada até agora (null = nenhuma ainda) e a distância dela
        WeaponPickup closestPickup = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D weaponCollider in colliders)
        {
            // Procura o WeaponPickup no objeto do colisor ou nos pais dele; sem isso, não é arma
            WeaponPickup pickup = weaponCollider.GetComponentInParent<WeaponPickup>();
            if (pickup == null)
            {
                continue;
            }

            // Distância do centro do jogador até o centro do colisor da arma
            float distance = Vector2.Distance(playerCenter, weaponCollider.bounds.center);

            // Mais perto que a melhor até agora: vira a nova melhor
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPickup = pickup;
            }
        }

        // Nenhuma arma no alcance: não faz nada
        if (closestPickup == null)
        {
            return;
        }

        // Passa os valores da arma para o ataque do jogador
        playerAttack.EquipWeapon(
            closestPickup.WeaponRange,
            closestPickup.WeaponAngle,
            closestPickup.WeaponCooldown);

        Debug.Log("Pegou " + closestPickup.WeaponName);

        // Tira a arma do chão (o objeto inteiro, com o contorno junto)
        Destroy(closestPickup.gameObject);
    }

    // Desenho de ajuda na janela Scene, só quando o jogador está selecionado.
    // Mostra o círculo do alcance de pegar arma. Não aparece no jogo.
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
