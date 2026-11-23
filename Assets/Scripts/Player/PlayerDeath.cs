using UnityEngine;

// Morte do jogador: um golpe mata.
// Ao receber ReceiveHit, o jogador para de andar, mirar e atacar, vira um corpo parado na cena
// (troca o desenho de vivo pelo de morto) e avisa o resto do jogo com GameEvents.RaisePlayerDied.
// Uma segunda pancada, mesmo no mesmo quadro, não faz nada.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDeath : MonoBehaviour, IHittable
{
    // Opcional. O desenho do jogador vivo (o filho Visual). Some quando o jogador morre.
    // Se ficar vazio, essa troca é pulada sem erro.
    [SerializeField] private GameObject aliveVisual;

    // Opcional. O desenho do jogador morto. Precisa ser filho DIRETO do Player (irmão do Visual,
    // nunca dentro dele) e começar desativado. Aparece quando o jogador morre.
    // Se ficar vazio, essa troca é pulada sem erro.
    [SerializeField] private GameObject deadVisual;

    // Verdadeiro depois do primeiro golpe. Impede que um segundo golpe repita a morte.
    private bool isDead = false;

    // Corpo físico do jogador (o RequireComponent garante que existe)
    private Rigidbody2D rb;

    // Os três scripts que fazem o jogador agir. Podem não existir (por exemplo, numa cena de teste).
    private PlayerMovement playerMovement;
    private PlayerAim playerAim;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        // Guarda as referências uma vez só, no começo, para não buscar na hora da morte
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAim = GetComponent<PlayerAim>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    // Chamado por quem ataca (inimigo ou peça de teste), que só conhece a interface IHittable.
    // O parâmetro "from" (posição de quem atacou) não é usado nesta tarefa.
    public void ReceiveHit(Vector2 from)
    {
        // a. Já está morto: ignora o golpe (vale também para dois golpes no mesmo quadro)
        if (isDead)
        {
            return;
        }

        // b. Marca como morto logo no começo, antes de qualquer outra coisa
        isDead = true;

        // c. Desliga os scripts de agir: o Unity para de chamar Update e FixedUpdate deles.
        // Se algum não existir no objeto, pula sem erro.
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (playerAim != null)
        {
            playerAim.enabled = false;
        }
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        // d. Para o corpo na hora e tira ele da física:
        // não é empurrado, não é acertado de novo e não bloqueia a passagem
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // e. Troca o desenho: esconde o vivo e mostra o morto (cada um só se estiver preenchido)
        if (aliveVisual != null)
        {
            aliveVisual.SetActive(false);
        }
        if (deadVisual != null)
        {
            deadVisual.SetActive(true);
        }

        // f. Por último, avisa o jogo uma única vez (quem escuta, como a tela de morte, reage)
        GameEvents.RaisePlayerDied();
    }
}
