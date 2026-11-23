using UnityEngine;

// PEÇA FALSA SÓ PARA CENAS DE TESTE. NÃO USAR NA MAIN.
// Faz o papel de "algo que pode ser acertado" enquanto o inimigo de verdade não existe:
// ao receber um golpe, avisa no Console e some.
public class TestDummy : MonoBehaviour, IHittable
{
    // Chamado por quem ataca. "from" é a posição de quem deu o golpe.
    public void ReceiveHit(Vector2 from)
    {
        // Escreve no Console quem foi acertado e de onde veio o golpe
        Debug.Log(name + " recebeu um golpe vindo de " + from, this);

        // Um golpe mata: remove este objeto da cena
        Destroy(gameObject);
    }
}
