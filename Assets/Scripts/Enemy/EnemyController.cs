using UnityEngine;

// Controla o inimigo: patrulha entre pontos fixos, detecta o jogador, alerta a sala,
// persegue em linha reta, ataca com o bastão e morre com um golpe.
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

    // Velocidade da patrulha em unidades por segundo (1 unidade = 1 tile). Valor provisório, mais lento que o jogador (6).
    [SerializeField] private float moveSpeed = 3f;

    // Velocidade em alerta, indo até o jogador. Valor provisório, ainda mais lento que o jogador.
    [SerializeField] private float chaseSpeed = 4.5f;

    // A esta distância (em unidades) do ponto, consideramos que o inimigo chegou nele
    [SerializeField] private float arrivalDistance = 0.05f;

    // Filho do inimigo onde fica o desenho. Gira para olhar na direção em que anda.
    // Pode ficar vazio: nesse caso o inimigo anda sem girar o desenho.
    [SerializeField] private Transform visual;

    // Segundos vendo o jogador sem parar até identificá-lo. GDD: ~0,8 s.
    [SerializeField] private float detectionTime = 0.8f;

    // Segundos sem ver o jogador, em alerta, até esquecer e voltar a patrulhar. GDD: ~5 s.
    [SerializeField] private float forgetTime = 5f;

    // Alcance do golpe do inimigo, em unidades. Igual ao bastão do jogador (1,5 tile, provisório).
    [SerializeField] private float attackRange = 1.5f;

    // Segundos que o inimigo "prepara" o golpe, vendo o jogador dentro do alcance, antes de acertar.
    // Dá ao jogador uma chance de reagir. Valor provisório.
    [SerializeField] private float attackWindup = 0.3f;

    // Vendo o jogador, o inimigo para de andar a esta distância dele (para não grudar no jogador)
    [SerializeField] private float stopDistance = 1f;

    // Raio do corpo do inimigo, usado para checar parede antes de cada passo.
    // Um pouco menor que metade do colisor (0,625 / 2 = 0,3125).
    [SerializeField] private float bodyRadius = 0.3f;

    // Opcional. O desenho do inimigo morto (o corpo no chão). Filho direto do Enemy,
    // começa desativado e aparece no lugar do Visual quando o inimigo morre.
    // Se ficar vazio, o desenho vivo continua aparecendo parado.
    [SerializeField] private GameObject deadVisual;

    // Opcional. A arma que o inimigo larga ao morrer (o prefab WeaponPickup do Ailton).
    // Se ficar vazio, ele não larga nada.
    [SerializeField] private GameObject droppedWeapon;

    // Direção para onde o inimigo está olhando (tamanho 1). A visão usa isso.
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    // Quanto o contador de detecção já encheu, de 0 (nada) a 1 (identificou).
    // A interrogação usa isso para a cor.
    public float DetectionProgress => detectionTimer / detectionTime;

    // Estados que a interrogação precisa ler (só leitura para os outros scripts)
    public bool IsAlerted => state == EnemyState.Alert;
    public bool IsDead => state == EnemyState.Dead;

    // Último lugar onde o jogador foi visto (por este inimigo ou por quem alertou a sala)
    public Vector2 LastKnownPlayerPosition { get; private set; }

    private Rigidbody2D rb;
    private EnemyVision vision;
    private EnemyState state = EnemyState.Patrol;

    // Máscara que contém só a camada das paredes (para checar parede antes de andar)
    private int wallsMask;

    // Resposta da visão neste frame (calculada no Update, usada também no FixedUpdate)
    private bool seesPlayer;

    // Contadores em segundos
    private float detectionTimer;
    private float forgetTimer;
    private float attackTimer;

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
        wallsMask = LayerMask.GetMask(Names.WallsLayer);

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
        seesPlayer = false;

        if (state == EnemyState.Alert)
        {
            state = EnemyState.Patrol;
        }
        detectionTimer = 0f;
        forgetTimer = 0f;
        attackTimer = 0f;
    }

    private void Update()
    {
        // Morto não vê nada; com o jogador morto, não há o que ver
        if (state == EnemyState.Dead || playerIsDead)
        {
            return;
        }

        // Os dois contadores usam a mesma verificação de linha de visão (GDD)
        seesPlayer = vision.CanSeePlayer(FacingDirection);

        if (state == EnemyState.Patrol)
        {
            UpdateDetection();
        }
        else if (state == EnemyState.Alert)
        {
            UpdateAlert();
        }
    }

    // Patrulhando: o contador de detecção enche enquanto vê o jogador
    // e esvazia no mesmo ritmo quando deixa de ver
    private void UpdateDetection()
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

        // Encheu: identificou o jogador e avisa a sala inteira
        if (detectionTimer >= detectionTime)
        {
            Debug.Log(name + " identificou o jogador e alertou a sala", this);
            AlertRoom(vision.PlayerPosition);
        }
    }

    // Avisa todos os inimigos da mesma sala (inclusive este) do ponto onde o jogador foi visto.
    // "Mesma sala" = filhos do mesmo objeto pai, como no GDD (um objeto vazio por sala).
    private void AlertRoom(Vector2 playerPosition)
    {
        // Sem pai (inimigo solto na cena): só este inimigo entra em alerta
        if (transform.parent == null)
        {
            ReceiveAlert(playerPosition);
            return;
        }

        foreach (EnemyController roommate in transform.parent.GetComponentsInChildren<EnemyController>())
        {
            roommate.ReceiveAlert(playerPosition);
        }
    }

    // Chamado por quem alertou a sala: entra em alerta indo até o ponto informado
    public void ReceiveAlert(Vector2 playerPosition)
    {
        // Morto não acorda, e com o jogador morto não há alerta
        if (state == EnemyState.Dead || playerIsDead)
        {
            return;
        }

        state = EnemyState.Alert;
        LastKnownPlayerPosition = playerPosition;
        detectionTimer = detectionTime;
        forgetTimer = 0f;
    }

    // Em alerta: vendo o jogador, guarda onde ele está, olha para ele e ataca se estiver perto;
    // sem ver, o contador de esquecimento enche até voltar a patrulhar
    private void UpdateAlert()
    {
        if (seesPlayer)
        {
            LastKnownPlayerPosition = vision.PlayerPosition;
            forgetTimer = 0f;
            Face(LastKnownPlayerPosition - rb.position);
            UpdateAttack();
            return;
        }

        // Perdeu o jogador de vista: o golpe preparado é cancelado
        attackTimer = 0f;
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

    // Prepara o golpe enquanto o jogador está dentro do alcance; ao completar, acerta
    private void UpdateAttack()
    {
        float distance = Vector2.Distance(rb.position, LastKnownPlayerPosition);

        // Fora do alcance: o golpe preparado é cancelado
        if (distance > attackRange)
        {
            attackTimer = 0f;
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer < attackWindup)
        {
            return;
        }

        attackTimer = 0f;

        // Acerta o jogador pelo contrato IHittable (o PlayerDeath do Ailton faz o resto).
        // Se o jogador morrer, o aviso PlayerDied chega aqui e o inimigo para.
        if (vision.PlayerHittable != null)
        {
            vision.PlayerHittable.ReceiveHit(rb.position);
        }
    }

    private void FixedUpdate()
    {
        // Movimento acontece em FixedUpdate, no mesmo ritmo da física
        if (state == EnemyState.Patrol)
        {
            Patrol();
        }
        else if (state == EnemyState.Alert)
        {
            Chase();
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

        Vector2 target = patrolPositions[currentPointIndex];

        // Chegou no ponto: escolhe o próximo. O "%" faz voltar ao 0 depois do último.
        if (Vector2.Distance(rb.position, target) <= arrivalDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPositions.Length;
            return;
        }

        MoveTowards(target, moveSpeed);

        // Olha para onde está andando
        Face(target - rb.position);
    }

    // Em alerta: anda em linha reta até o último ponto conhecido do jogador
    private void Chase()
    {
        Vector2 toTarget = LastKnownPlayerPosition - rb.position;

        // Vendo o jogador, para um pouco antes dele; sem ver, vai até o ponto exato
        float stopAt = seesPlayer ? stopDistance : arrivalDistance;
        if (toTarget.magnitude <= stopAt)
        {
            return;
        }

        MoveTowards(LastKnownPlayerPosition, chaseSpeed);

        // Sem ver o jogador, olha para onde anda (vendo, o UpdateAlert já o faz olhar para o jogador)
        if (!seesPlayer)
        {
            Face(toTarget);
        }
    }

    // Dá um passo em direção ao alvo, sem passar dele, e para se houver parede no caminho.
    // O corpo é Kinematic e não colide sozinho com a parede, por isso a checagem antes do passo.
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, target, speed * Time.fixedDeltaTime);
        Vector2 step = nextPosition - currentPosition;

        // CircleCast "arrasta" um círculo do tamanho do corpo ao longo do passo.
        // Se bater em algo da camada Walls, o inimigo fica parado (GDD: para se bater na parede).
        if (Physics2D.CircleCast(currentPosition, bodyRadius, step.normalized, step.magnitude, wallsMask))
        {
            return;
        }

        // MovePosition é o jeito certo de mover um Rigidbody2D Kinematic
        rb.MovePosition(nextPosition);
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

        // Troca o desenho vivo pelo corpo no chão, virado para o mesmo lado
        if (deadVisual != null)
        {
            if (visual != null)
            {
                deadVisual.transform.rotation = visual.rotation;
                visual.gameObject.SetActive(false);
            }
            deadVisual.SetActive(true);
        }

        // Larga a arma no chão, no lugar onde morreu. Fica dentro do mesmo objeto da sala,
        // assim some junto quando a sala é recarregada.
        if (droppedWeapon != null)
        {
            Instantiate(droppedWeapon, transform.position, Quaternion.identity, transform.parent);
        }

        // Avisa as outras partes do jogo (o Arthur conta os inimigos vivos da sala)
        GameEvents.RaiseEnemyDied(gameObject);
    }

    // Desenho de ajuda na janela Scene: linhas ligando os pontos da patrulha
    // e o círculo vermelho do alcance do golpe. Não aparece no jogo.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
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
