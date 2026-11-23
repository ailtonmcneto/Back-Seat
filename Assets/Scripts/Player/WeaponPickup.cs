using UnityEngine;

// Arma no chão, esperando o jogador pegar.
// Guarda os valores da arma (nome, alcance, arco e intervalo) para o PlayerPickup ler,
// e faz o contorno piscar para o jogador ver que ali tem algo para pegar.
// Este script não pega nada sozinho: quem pega é o PlayerPickup, no jogador.
public class WeaponPickup : MonoBehaviour
{
    // Nome da arma, usado na mensagem do Console ao pegar.
    [SerializeField] private string weaponName = "Bastão";

    // Alcance do golpe com esta arma, em unidades (1 unidade = 1 tile). Valor provisório.
    [SerializeField] private float weaponRange = 1.5f;

    // Abertura total do arco do golpe com esta arma, em graus. Valor provisório.
    [SerializeField] private float weaponAngle = 90f;

    // Tempo mínimo em segundos entre um golpe e o próximo com esta arma. Valor provisório.
    [SerializeField] private float weaponCooldown = 0.25f;

    // Opcional. O contorno da arma (um filho com o desenho do contorno). Pisca enquanto a arma está no chão.
    // Se ficar vazio, a arma não pisca e nada dá erro.
    [SerializeField] private GameObject outline;

    // Tempo em segundos que o contorno fica ligado e depois desligado, em cada metade da piscada.
    [SerializeField] private float blinkInterval = 0.5f;

    // Propriedades só de leitura: outros scripts conseguem ler os valores, mas não mudar.
    public string WeaponName => weaponName;
    public float WeaponRange => weaponRange;
    public float WeaponAngle => weaponAngle;
    public float WeaponCooldown => weaponCooldown;

    private void Update()
    {
        // Sem contorno preenchido: não há o que piscar
        if (outline == null)
        {
            return;
        }

        // Conta quantos intervalos inteiros já passaram desde o início do jogo.
        // Número par: contorno ligado. Número ímpar: contorno desligado.
        // Assim ele alterna sozinho a cada blinkInterval segundos, sem precisar de contador.
        int intervalsPassed = Mathf.FloorToInt(Time.time / blinkInterval);
        bool showOutline = intervalsPassed % 2 == 0;
        outline.SetActive(showOutline);
    }
}
