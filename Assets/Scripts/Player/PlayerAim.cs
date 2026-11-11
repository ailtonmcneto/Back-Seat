using UnityEngine;
using UnityEngine.InputSystem;

// Gira o desenho do jogador para apontar para o cursor do mouse.
// Quem gira é um filho (aimPivot), nunca o objeto raiz nem o Rigidbody2D.
public class PlayerAim : MonoBehaviour
{
    // Filho do jogador que gira (o objeto "Visual", onde fica o desenho)
    [SerializeField] private Transform aimPivot;

    // Com o cursor mais perto do jogador que isso (em unidades), a mira não muda
    [SerializeField] private float minAimDistance = 0.1f;

    // Direção da mira, sempre com tamanho 1. Outros scripts podem ler, mas não alterar.
    // Começa apontando para a direita, que é a frente do desenho.
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        // Sem o aimPivot não há o que girar: avisa uma vez e desliga este componente,
        // assim o Update nem chega a rodar
        if (aimPivot == null)
        {
            Debug.LogError("PlayerAim: o campo aimPivot está vazio. Arraste o filho 'Visual' do jogador para ele no Inspector.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // Leitura de input acontece em Update, que roda uma vez por frame
        Mouse mouse = Mouse.current;
        Camera mainCamera = Camera.main;

        // Sem mouse conectado ou sem câmera com a tag MainCamera: não faz nada neste frame
        if (mouse == null || mainCamera == null)
        {
            return;
        }

        // O mouse informa a posição em pixels da tela.
        // A câmera converte esse ponto para uma posição no mundo do jogo.
        Vector2 mouseScreenPosition = mouse.position.ReadValue();
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // Seta que sai do jogador (objeto raiz) e chega no cursor
        Vector2 toCursor = mouseWorldPosition - (Vector2)transform.position;

        // Cursor em cima do jogador: a direção ficaria instável, então mantém a última
        if (toCursor.magnitude < minAimDistance)
        {
            return;
        }

        // Guarda só a direção (tamanho 1), sem a distância
        AimDirection = toCursor.normalized;

        // Atan2 devolve o ângulo da direção em radianos; convertemos para graus.
        // 0 graus = direita, 90 graus = cima.
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;

        // Gira o pivô só no eixo Z, na hora (sem suavização).
        // Com isso a direita do desenho (+X local) aponta para o cursor.
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
