using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Ataque corpo a corpo do jogador com o punho.
// O botão esquerdo do mouse dá um golpe instantâneo em arco, à frente, na direção da mira.
// Quem estiver no arco e implementar IHittable recebe ReceiveHit. Parede bloqueia o golpe.
[RequireComponent(typeof(PlayerAim))]
public class PlayerAttack : MonoBehaviour
{
    // Alcance do golpe em unidades (1 unidade = 1 tile), medido do centro do jogador.
    // O valor inicial é o do punho (provisório). Ao pegar uma arma, EquipWeapon troca este valor.
    [SerializeField] private float attackRange = 1f;

    // Abertura total do arco em graus: metade para cada lado da mira.
    // O valor inicial é o do punho (provisório). Ao pegar uma arma, EquipWeapon troca este valor.
    [SerializeField] private float attackAngle = 90f;

    // Tempo mínimo em segundos entre um golpe e o próximo.
    // O valor inicial é o do punho (provisório). Ao pegar uma arma, EquipWeapon troca este valor.
    [SerializeField] private float attackCooldown = 0.5f;

    // Verdadeiro quando o jogador pegou uma arma do chão (falso = punho).
    // Qualquer script pode ler; só este script pode mudar (private set).
    public bool HasWeapon { get; private set; } = false;

    // Componente da mira, de onde vem a direção do golpe
    private PlayerAim playerAim;

    // Máscara que contém só a camada das paredes (usada para saber se há parede no caminho)
    private int wallsMask;

    // Horário (no relógio do jogo, Time.time) a partir do qual o próximo golpe é permitido
    private float nextAttackTime;

    private void Awake()
    {
        // O RequireComponent garante que o PlayerAim existe no mesmo objeto
        playerAim = GetComponent<PlayerAim>();

        // Monta a máscara uma vez só, usando o nome da camada que está no Contracts.cs
        wallsMask = LayerMask.GetMask(Names.WallsLayer);
    }

    private void Update()
    {
        // Leitura de input acontece em Update, que roda uma vez por frame
        Mouse mouse = Mouse.current;

        // Sem mouse conectado: não faz nada
        if (mouse == null)
        {
            return;
        }

        // wasPressedThisFrame só é verdadeiro no frame em que o botão desce.
        // Por isso segurar o botão não repete o golpe.
        if (!mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // Clique antes de o intervalo acabar é ignorado (não fica guardado para depois)
        if (Time.time < nextAttackTime)
        {
            return;
        }

        // Clique aceito: o intervalo começa a contar agora, acertando algo ou não
        nextAttackTime = Time.time + attackCooldown;

        Attack();
    }

    // Chamado quando o jogador pega uma arma do chão (pelo PlayerPickup).
    // A partir daqui, o golpe usa o alcance, o arco e o intervalo da arma no lugar dos do punho.
    public void EquipWeapon(float newRange, float newAngle, float newCooldown)
    {
        attackRange = newRange;
        attackAngle = newAngle;
        attackCooldown = newCooldown;
        HasWeapon = true;
    }

    // Dá um golpe: acontece inteiro neste frame, sem duração
    private void Attack()
    {
        // Centro do jogador = posição do objeto raiz
        Vector2 playerCenter = transform.position;
        Vector2 aimDirection = playerAim.AimDirection;

        // Lista de quem já foi acertado neste golpe.
        // Serve para um alvo com mais de um colisor não receber o golpe duas vezes.
        List<IHittable> alreadyHit = new List<IHittable>();

        // Pega todos os colisores que tocam um círculo em volta do jogador
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerCenter, attackRange);

        foreach (Collider2D hitCollider in colliders)
        {
            // a. Triggers não são corpos de verdade (ex.: área de pegar arma): ignora
            if (hitCollider.isTrigger)
            {
                continue;
            }

            // b. Colisor do próprio jogador (raiz ou filho dele): ignora
            if (hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            // c. Procura IHittable no objeto do colisor ou nos pais dele; sem isso, não é alvo
            IHittable hittable = hitCollider.GetComponentInParent<IHittable>();
            if (hittable == null)
            {
                continue;
            }

            // d. Esse alvo já recebeu este golpe por outro colisor: ignora
            if (alreadyHit.Contains(hittable))
            {
                continue;
            }

            // e. Fora do arco: o ângulo entre a mira e a seta até o alvo passa da metade da abertura
            Vector2 targetCenter = hitCollider.bounds.center;
            Vector2 toTarget = targetCenter - playerCenter;
            if (Vector2.Angle(aimDirection, toTarget) > attackAngle / 2f)
            {
                continue;
            }

            // f. Parede no caminho: uma linha do jogador até o alvo bate em algo da camada Walls
            if (Physics2D.Linecast(playerCenter, targetCenter, wallsMask))
            {
                continue;
            }

            // g. Passou por tudo: acerta, informando de onde veio o golpe, e anota na lista
            hittable.ReceiveHit(playerCenter);
            alreadyHit.Add(hittable);
        }
    }

    // Desenho de ajuda na janela Scene, só quando o jogador está selecionado.
    // Mostra o círculo do alcance e as duas bordas do arco. Não aparece no jogo.
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;

        // Fora do Play Mode o Awake não rodou, então busca o PlayerAim aqui.
        // Se não existir, desenha apontando para a direita.
        Vector2 aimDirection = Vector2.right;
        PlayerAim aim = GetComponent<PlayerAim>();
        if (aim != null)
        {
            aimDirection = aim.AimDirection;
        }

        // Círculo do alcance
        Gizmos.DrawWireSphere(center, attackRange);

        // Gira a direção da mira metade da abertura para cada lado (giro no eixo Z)
        float halfAngle = attackAngle / 2f;
        Vector3 leftEdge = Quaternion.Euler(0f, 0f, halfAngle) * aimDirection;
        Vector3 rightEdge = Quaternion.Euler(0f, 0f, -halfAngle) * aimDirection;

        // Uma linha do centro até a ponta de cada borda do arco
        Gizmos.DrawLine(center, center + leftEdge * attackRange);
        Gizmos.DrawLine(center, center + rightEdge * attackRange);
    }
}
