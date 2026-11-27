using UnityEngine;

// "Olhos" do inimigo. Só responde uma pergunta: estou vendo o jogador agora?
// Quem decide o que fazer com a resposta (detectar, esquecer) é o EnemyController.
public class EnemyVision : MonoBehaviour
{
    // Alcance máximo da visão em unidades (1 unidade = 1 tile). GDD: ~6,5 tiles.
    [SerializeField] private float viewDistance = 6.5f;

    // Abertura total do cone de visão em graus, metade para cada lado da frente.
    // Fora do cone (pelas costas) o inimigo não vê. Valor provisório.
    [SerializeField] private float viewAngle = 120f;

    // Posição do jogador da última vez que foi procurado (o alerta usa isso)
    public Vector2 PlayerPosition { get; private set; }

    // A parte do jogador que pode ser acertada (o PlayerDeath do Ailton).
    // Só conhecemos a interface do Contracts.cs, nunca a classe do jogador.
    public IHittable PlayerHittable { get; private set; }

    // Jogador encontrado pela tag
    private Transform player;

    // Máscara que contém só a camada das paredes
    private int wallsMask;

    // Resposta da última verificação, só para o desenho de ajuda
    private bool lastResult;

    private void Awake()
    {
        // Monta a máscara uma vez só, usando o nome do Contracts.cs
        wallsMask = LayerMask.GetMask(Names.WallsLayer);
    }

    // Devolve true se o jogador está dentro do alcance, dentro do cone
    // e sem parede no caminho. "facing" é a direção para onde o inimigo olha.
    public bool CanSeePlayer(Vector2 facing)
    {
        lastResult = CheckSight(facing);
        return lastResult;
    }

    private bool CheckSight(Vector2 facing)
    {
        // Procura o jogador pela tag só enquanto ainda não achou
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag(Names.PlayerTag);
            if (playerObject == null)
            {
                return false;
            }
            player = playerObject.transform;
            PlayerHittable = playerObject.GetComponent<IHittable>();
        }

        Vector2 eyes = transform.position;
        PlayerPosition = player.position;
        Vector2 toPlayer = PlayerPosition - eyes;

        // 1. Longe demais
        if (toPlayer.magnitude > viewDistance)
        {
            return false;
        }

        // 2. Fora do cone: o ângulo entre a frente e o jogador passa da metade da abertura
        if (Vector2.Angle(facing, toPlayer) > viewAngle / 2f)
        {
            return false;
        }

        // 3. Parede no meio: a linha do inimigo até o jogador bate em algo da camada Walls
        if (Physics2D.Linecast(eyes, PlayerPosition, wallsMask))
        {
            return false;
        }

        return true;
    }

    // Desenho de ajuda na janela Scene: círculo do alcance e, no Play Mode,
    // uma linha até o jogador (vermelha = vendo, cinza = não vendo). Não aparece no jogo.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = lastResult ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
