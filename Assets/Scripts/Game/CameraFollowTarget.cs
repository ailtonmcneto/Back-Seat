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
        if (gameCamera != null && hasMouseInput && gameCamera.pixelRect.Contains(mouseScreenPosition))
        {
            // A câmera é ortográfica e sem rotação; o alvo fica no plano XY.
            float distanceToTarget = target.position.z - gameCamera.transform.position.z;
            Vector3 mouseWorldPosition = gameCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distanceToTarget));

            // Mede a partir do centro da visão para o movimento da câmera não se acumular.
            Vector2 offsetFromViewCenter = mouseWorldPosition - gameCamera.transform.position;
            lookAheadOffset = Vector2.ClampMagnitude(offsetFromViewCenter, lookAheadDistance);
        }

        // Desloca só X e Y: preserva o Z do alvo e não copia sua rotação.
        transform.position = target.position + (Vector3)lookAheadOffset;
    }
}
