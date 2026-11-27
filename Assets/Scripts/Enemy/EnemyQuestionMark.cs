using UnityEngine;

// Interrogação acima do inimigo (GDD: enche de branco para vermelho conforme a detecção sobe).
// - Escondida quando o inimigo não desconfia de nada.
// - Aparece e vai ficando vermelha enquanto o contador de detecção enche.
// - Vermelha cheia em alerta. Some quando o inimigo morre.
// Fica num filho do inimigo (fora do Visual, para não girar junto com o desenho).
public class EnemyQuestionMark : MonoBehaviour
{
    // O desenho da interrogação (o SpriteRenderer deste objeto)
    [SerializeField] private SpriteRenderer mark;

    // Cor com a detecção começando a encher
    [SerializeField] private Color startColor = Color.white;

    // Cor com a detecção cheia e em alerta. GDD: vermelho #C8102E (200, 16, 46).
    [SerializeField] private Color fullColor = new Color(200f / 255f, 16f / 255f, 46f / 255f);

    // O inimigo dono desta interrogação (achado sozinho no pai)
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();

        // Sem o desenho ou sem o inimigo não há o que mostrar: avisa uma vez e desliga
        if (mark == null || enemy == null)
        {
            Debug.LogError("EnemyQuestionMark: preencha o campo Mark e deixe este objeto como filho do Enemy.", this);
            enabled = false;
            return;
        }

        mark.enabled = false;
    }

    private void Update()
    {
        // Morto, ou sem nenhuma desconfiança: esconde
        if (enemy.IsDead || (!enemy.IsAlerted && enemy.DetectionProgress <= 0f))
        {
            mark.enabled = false;
            return;
        }

        mark.enabled = true;

        // Em alerta: vermelho cheio. Desconfiando: mistura as duas cores.
        // Color.Lerp com 0 dá a cor inicial, com 1 dá a cor final, e no meio uma mistura.
        if (enemy.IsAlerted)
        {
            mark.color = fullColor;
        }
        else
        {
            mark.color = Color.Lerp(startColor, fullColor, enemy.DetectionProgress);
        }
    }
}
