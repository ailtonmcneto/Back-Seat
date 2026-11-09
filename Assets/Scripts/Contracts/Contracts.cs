using System;
using UnityEngine;

// =====================================================================
// CONTRATOS DO BACKSEAT
// Arquivo compartilhado pelos três. Fica em Assets/Scripts/Contracts/.
// Só muda com os três de acordo (na hora juntos da semana).
// Cada um programa contra estes nomes; a ligação real acontece sozinha
// quando as partes se encontram na cena.
// =====================================================================

// Tudo que morre com um golpe implementa isto: o jogador (Ailton) e o inimigo (Lipe).
// Quem ataca não precisa saber o que acertou, só chama ReceiveHit.
public interface IHittable
{
    // Um golpe mata. "from" é a posição de quem atacou (útil para o sangue espirrar na direção certa).
    void ReceiveHit(Vector2 from);
}

// Avisos que uma parte do jogo dá para as outras.
// Quem avisa chama o método Raise...; quem escuta se inscreve no evento.
// IMPORTANTE: quem escuta se inscreve em OnEnable e se desinscreve em OnDisable,
// senão o aviso chega para objetos destruídos depois que a sala recarrega.
public static class GameEvents
{
    // Ailton avisa (jogador morreu). Arthur escuta (tela de morte).
    public static event Action PlayerDied;
    public static void RaisePlayerDied() => PlayerDied?.Invoke();

    // Lipe avisa (inimigo morreu). Arthur escuta (conta os vivos para a sala limpa).
    public static event Action<GameObject> EnemyDied;
    public static void RaiseEnemyDied(GameObject enemy) => EnemyDied?.Invoke(enemy);

    // Arthur avisa (último inimigo morreu). Arthur escuta (abre a saída, mostra o aviso e a seta).
    public static event Action RoomCleared;
    public static void RaiseRoomCleared() => RoomCleared?.Invoke();

    // Arthur avisa (jogador entrou no carro). Arthur escuta (tela de fim de fase).
    public static event Action PlayerReachedCar;
    public static void RaisePlayerReachedCar() => PlayerReachedCar?.Invoke();
}

// Nomes que precisam ser iguais nos três computadores.
// Criem estas camadas e tags no Unity (Project Settings > Tags and Layers) com estes nomes exatos.
public static class Names
{
    public const string PlayerTag = "Player";       // tag do jogador (Lipe procura o jogador por ela)
    public const string EnemyTag = "Enemy";         // tag do inimigo
    public const string WallsLayer = "Walls";       // camada do Tilemap de paredes (linha de visão e colisão)
    public const string WeaponLayer = "Weapon";     // camada da arma no chão (pickup do Ailton)
}
