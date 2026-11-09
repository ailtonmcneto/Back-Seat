using UnityEngine;
using UnityEngine.InputSystem;

// Move o jogador em 8 direções com WASD, em velocidade constante.
// Soltou a tecla, para na hora (sem aceleração nem inércia).
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // Velocidade em unidades por segundo (1 unidade = 1 tile, GDD: ~6 tiles/s)
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;

    // Direção pedida pelo jogador neste frame (já normalizada)
    private Vector2 moveInput;

    private void Awake()
    {
        // Guarda a referência ao Rigidbody2D uma vez só, em vez de buscar todo frame
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Leitura de input acontece em Update, que roda uma vez por frame
        Keyboard keyboard = Keyboard.current;

        // Sem teclado conectado: fica parado e não dá erro
        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.wKey.isPressed) y += 1f;
        if (keyboard.sKey.isPressed) y -= 1f;

        // Normaliza para a diagonal não ficar mais rápida que as retas
        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        // Movimento acontece em FixedUpdate, no mesmo ritmo da física.
        // Definimos a velocidade direto: sem tecla, moveInput é zero e o jogador para.
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
