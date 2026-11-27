using UnityEngine;
using UnityEngine.InputSystem;

// PEÇA FALSA SÓ PARA CENAS DE TESTE. NÃO VAI PARA A MAIN.
// Faz o papel do inimigo atacando o jogador enquanto o inimigo de verdade não existe:
// tecla H dá um golpe no jogador; tecla J dá dois golpes seguidos no mesmo quadro.
// Também escuta o aviso PlayerDied e conta no Console quantas vezes ele chegou.
public class TestPlayerKiller : MonoBehaviour
{
    // O objeto do jogador (arraste o Player aqui no Inspector)
    [SerializeField] private GameObject player;

    // A parte do jogador que pode ser acertada. Só conhece a interface, não a classe do jogador.
    private IHittable playerHittable;

    // Quantas vezes o aviso PlayerDied chegou desde que o Play começou
    private int playerDiedCount = 0;

    private void Awake()
    {
        // Sem jogador no campo: avisa e desliga este componente
        if (player == null)
        {
            Debug.LogError("TestPlayerKiller: o campo Player está vazio. Arraste o Player para ele.", this);
            enabled = false;
            return;
        }

        // Procura no jogador algum componente que implemente IHittable
        playerHittable = player.GetComponent<IHittable>();

        // Jogador sem IHittable: avisa e desliga este componente
        if (playerHittable == null)
        {
            Debug.LogError("TestPlayerKiller: o objeto " + player.name + " não tem nenhum componente com IHittable (falta o PlayerDeath?).", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // Começa a escutar o aviso de morte do jogador
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        // Para de escutar, para o aviso não chegar a um objeto desligado ou destruído
        GameEvents.PlayerDied -= OnPlayerDied;
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

        // O golpe "vem" da posição deste objeto de teste
        Vector2 from = transform.position;

        // H: um golpe
        if (keyboard.hKey.wasPressedThisFrame)
        {
            playerHittable.ReceiveHit(from);
        }

        // J: dois golpes seguidos no mesmo quadro (o segundo não deve fazer nada)
        if (keyboard.jKey.wasPressedThisFrame)
        {
            playerHittable.ReceiveHit(from);
            playerHittable.ReceiveHit(from);
        }
    }

    // Chamado cada vez que alguém chama GameEvents.RaisePlayerDied()
    private void OnPlayerDied()
    {
        playerDiedCount++;
        Debug.Log("PlayerDied recebido " + playerDiedCount + " vez(es)", this);
    }
}
