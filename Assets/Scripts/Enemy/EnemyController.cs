using UnityEngine;

// Controla o inimigo: patrulha entre pontos fixos, detecta o jogador e morre com um golpe.
// Implementa IHittable (do Contracts.cs), então o ataque do jogador consegue acertá-lo
// sem conhecer esta classe.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyController : MonoBehaviour, IHittable
{
    // Estados possíveis do inimigo (GDD: patrulha → alerta → morto)
    private enum EnemyState
    {
        Patrol,
        Alert,
        Dead
    }

    // Pontos da patrulha, na ordem em que o inimigo passa por eles (volta ao primeiro no fim).
    // São objetos vazios colocados na sala.
    [SerializeField] private Transform[] patrolPoints;

    // Velocidade em unidades por segundo (1 unidade = 1 tile). Valor provisório, mais lento que o jogador (6).
    [SerializeField] private float moveSpeed = 3f;

    // A esta distância (em unidades) do ponto, consideramos que o inimigo chegou nele
    [SerializeField] private float arrivalDistance = 0.05f;

    // Filho do inimigo onde fica o desenho. Gira para olhar na direção em que anda.
    // Pode ficar vazio: nesse caso o inimigo anda sem girar o desenho.
    [SerializeField] private Transform visual;

    // Segundos vendo o jogador sem parar até identificá-lo. GDD: ~0,8 s.
    [SerializeField] private float detectionTime = 0.8f;

    // Segundos sem ver o jogador, em alerta, até esquecer e voltar a patrulhar. GDD: ~5 s.
    [SerializeField] private float forgetTime = 5f;

    // Direção para onde o inimigo está olhando (tamanho 1). A visão usa isso.
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    // Quanto o contador de detecção já encheu, de 0 (nada) a 1 (identificou).
    // A interrogação vai usar isso numa próxima tarefa.
    public float DetectionProgress => detectionTimer / detectionTime;

    // Último lugar onde o inimigo viu o jogador (o alerta vai usar isso na próxima tarefa)
    public Vector2 LastKnownPlayerPosition { get; private set; }

    private Rigidbody2D rb;
    private EnemyVision vision;
    private EnemyState state = EnemyState.Patrol;

    // Contadores em segundos
    private float detectionTimer;
    private float forgetTimer;

    // Verdadeiro depois do aviso PlayerDied. O corpo do jogador continua na cena com a tag
    // Player, então sem isso o inimigo continuaria vendo e indo até ele.
    private bool playerIsDead;

    // Posições dos pontos guardadas no início. Assim, mesmo que alguém coloque os pontos
    // como filhos do inimigo, eles não andam junto com ele.
    private Vector2[] patrolPositions;

    // Índice do ponto para onde o inimigo está indo agora
    private int currentPointIndex;

    private void Awake()
    {
        // Guarda as referências uma vez só
        rb = GetComponent<Rigidbody2D>();
        vision = GetComponent<EnemyVision>();

        // Sem os olhos o inimigo não funciona: avisa uma vez e desliga este componente,
        // assim o Update nem chega a rodar (em vez de dar erro a cada frame)
        if (vision == null)
        {
            Debug.LogError("EnemyController: falta o componente EnemyVision neste inimigo. Adicione com Add Component.", this);
            enabled = false;
            return;
        }

        // Copia a posição de cada ponto para a lista de posições
        if (patrolPoints == null)
        {
            patrolPositions = new Vector2[0];
            return;
        }

        patrolPositions = new Vector2[patrolPoints.Length];
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                Debug.LogError("EnemyController: o ponto de patrulha " + i + " está vazio no Inspector.", this);
                patrolPositions = new Vector2[0];
                return;
            }

            patrolPositions[i] = patrolPoints[i].position;
        }
    }

    private void OnEnable()
    {
        // Começa a escutar o aviso de morte do jogador (regra do AGENTS.md: inscreve no OnEnable)
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        // Para de escutar, para o aviso não chegar a um inimigo desligado ou destruído
        GameEvents.PlayerDied -= OnPlayerDied;
    }

    // Chamado quando o jogador morre: o inimigo esquece tudo e volta a patrulhar.
    // Ele não detecta mais ninguém até a sala ser recarregada pelo respawn.
    private void OnPlayerDied()
    {
        playerIsDead = true;

        if (state == EnemyState.Alert)
        {
            state = EnemyState.Patrol;
        }
        detectionTimer = 0f;
        forgetTimer = 0f;
    }

    private void Update()
    {
        // Morto não vê nada; com o jogador morto, não há o que ver
        if (state == EnemyState.Dead || playerIsDead)
        {
            return;
        }

        // Os dois contadores usam a mesma verificação de linha de visão (GDD)
        bool seesPlayer = vision.CanSeePlayer(FacingDirection);

        if (state == EnemyState.Patrol)
        {
            UpdateDetection(seesPlayer);
        }
        else if (state == EnemyState.Alert)
        {
            UpdateAlert(seesPlayer);
        }
    }

    // Patrulhando: o contador de detecção enche enquanto vê o jogador
    // e esvazia no mesmo ritmo quando deixa de ver
    private void UpdateDetection(bool seesPlayer)
    {
        if (seesPlayer)
        {
            detectionTimer += Time.deltaTime;
        }
        else
        {
            detectionTimer -= Time.deltaTime;
        }

        // Mantém o contador entre 0 e o tempo de detecção
        detectionTimer = Mathf.Clamp(detectionTimer, 0f, detectionTime);

        // Encheu: identificou o jogador
        if (detectionTimer >= detectionTime)
        {
            state = EnemyState.Alert;
            forgetTimer = 0f;
            LastKnownPlayerPosition = vision.PlayerPosition;
            Debug.Log(name + " identificou o jogador", this);
        }
    }

    // Em alerta: vendo o jogador, guarda onde ele está e olha para ele;
    // sem ver, o contador de esquecimento enche até voltar a patrulhar
    private void UpdateAlert(bool seesPlayer)
    {
        if (seesPlayer)
        {
            LastKnownPlayerPosition = vision.PlayerPosition;
            forgetTimer = 0f;
            Face(LastKnownPlayerPosition - rb.position);
            return;
        }

        forgetTimer += Time.deltaTime;

        // Encheu: esqueceu o jogador e volta a patrulhar do zero
        if (forgetTimer >= forgetTime)
        {
            state = EnemyState.Patrol;
            detectionTimer = 0f;
            forgetTimer = 0f;
            Debug.Log(name + " esqueceu o jogador e voltou a patrulhar", this);
        }
    }

    private void FixedUpdate()
    {
        // Movimento acontece em FixedUpdate, no mesmo ritmo da física
        if (state == EnemyState.Patrol)
        {
            Patrol();
        }
    }

    // Anda em linha reta até o ponto atual; ao chegar, passa para o próximo
    private void Patrol()
    {
        // Sem pontos (ou com um só), o inimigo fica parado no lugar
        if (patrolPositions.Length < 2)
        {
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 target = patrolPositions[currentPointIndex];

        // Chegou no ponto: escolhe o próximo. O "%" faz voltar ao 0 depois do último.
        if (Vector2.Distance(currentPosition, target) <= arrivalDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPositions.Length;
            return;
        }

        // Anda um passo em direção ao ponto, sem passar dele.
        // MovePosition é o jeito certo de mover um Rigidbody2D Kinematic.
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, target, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        // Olha para onde está andando
        Face(target - currentPosition);
    }

    // Gira o desenho para a direção informada (0 graus = direita, como no jogador)
    private void Face(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        FacingDirection = direction.normalized;

        if (visual != null)
        {
            float angle = Mathf.Atan2(FacingDirection.y, FacingDirection.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Chamado por quem ataca (o jogador). Um golpe mata.
    // "from" é a posição de quem atacou; ainda não usamos (o sangue é do Ailton).
    public void ReceiveHit(Vector2 from)
    {
        // Já está morto: ignora, para não avisar a morte duas vezes
        if (state == EnemyState.Dead)
        {
            return;
        }

        Die();
    }

    private void Die()
    {
        state = EnemyState.Dead;

        // Para de se mexer
        rb.linearVelocity = Vector2.zero;

        // Desliga os colisores: o corpo fica na sala, mas não bloqueia o jogador
        // e não pode ser acertado de novo
        foreach (Collider2D enemyCollider in GetComponentsInChildren<Collider2D>())
        {
            enemyCollider.enabled = false;
        }

        // Avisa as outras partes do jogo (o Arthur conta os inimigos vivos da sala)
        GameEvents.RaiseEnemyDied(gameObject);
    }

    // Desenho de ajuda na janela Scene: linhas ligando os pontos da patrulha. Não aparece no jogo.
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            return;
        }

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform from = patrolPoints[i];
            Transform to = patrolPoints[(i + 1) % patrolPoints.Length];
            if (from != null && to != null)
            {
                Gizmos.DrawLine(from.position, to.position);
            }
        }
    }
}
