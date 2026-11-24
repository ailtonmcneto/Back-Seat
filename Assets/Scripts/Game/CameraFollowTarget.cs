using UnityEngine;
using UnityEngine.InputSystem;

// Atualiza o ponto de acompanhamento antes do Cinemachine Brain em Late Update.
[DefaultExecutionOrder(-100)]
public class CameraFollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera gameCamera;
    [SerializeField, Min(0f)] private float lookAheadDistance = 3f;

    private Vector2 mouseScreenPosition;
    private bool hasMouseInput;

    private void Update()
    {
        // O input é lido em Update; a posição da câmera será calculada depois.
        Mouse mouse = Mouse.current;
        hasMouseInput = mouse != null && Application.isFocused;

        if (hasMouseInput)
        {
            mouseScreenPosition = mouse.position.ReadValue();
        }
    }

    private void LateUpdate()
    {
        // Sem alvo atribuído, mantém a posição atual e evita erro no Console.
        if (target == null)
        {
            return;
        }

        Vector2 lookAheadOffset = Vector2.zero;

        // Sem câmera, mouse ou foco, continua acompanhando o alvo sem deslocamento.
        if (gameCamera != null && hasMouseInput)
        {
            // O Pixel Perfect informa a área final da imagem, sem as barras pretas.
            Rect cameraRect = gameCamera.pixelRect;
            if (cameraRect.Contains(mouseScreenPosition))
            {
                // A altura visível da câmera ortográfica é o dobro do seu tamanho.
                float unitsPerScreenPixel = 2f * gameCamera.orthographicSize / cameraRect.height;

                // Converte a distância ao centro sem usar a matriz arredondada pelo Pixel Perfect.
                // A câmera permanece sem rotação; X e Y da tela correspondem aos do mundo.
                Vector2 offsetFromViewCenter = (mouseScreenPosition - cameraRect.center) * unitsPerScreenPixel;
                lookAheadOffset = Vector2.ClampMagnitude(offsetFromViewCenter, lookAheadDistance);
            }
        }

        // Desloca só X e Y: preserva o Z do alvo e não copia sua rotação.
        transform.position = target.position + (Vector3)lookAheadOffset;
    }
}
