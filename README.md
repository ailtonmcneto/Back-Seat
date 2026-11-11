# BACKSEAT

Jogo de ação top-down em preto e branco, ambientado numa cidade noir dos anos 30 (Lei Seca). Um capitão de polícia sobrevive a uma emboscada que matou a família e vai atrás dos responsáveis, sala por sala.

> **Um golpe mata, dos dois lados.** Sem barra de vida, sem armadura. Vencer depende de reflexo, movimento e leitura da sala.

Projeto da disciplina de Projeto Extensionista do curso de Ciência da Computação. Feito por Ailton Neto, Luis Felipe Veras (Lipe) e Arthur Tavares, com apresentação do pitch e da história por Fernando Meira.

**Status:** em desenvolvimento. Versão final prevista para dezembro de 2026.

---

## Sobre o jogo

- **Gênero:** ação top-down com furtividade leve. Inspirado em *Hotline Miami* e *Mark of the Ninja*.
- **Estilo visual:** "noir de 16 pixels": preto, branco e cinzas. O vermelho aparece só em sangue e na interrogação dos inimigos.
- **Inimigos:** patrulham as salas. Se virem o jogador por tempo suficiente, alertam a sala inteira e vão até o último ponto onde ele foi visto.
- **Armas:** o jogador começa com o punho e pega o bastão que os inimigos largam.
- **Estrutura:** duas fases no escopo principal (um bar clandestino e uma mansão), com volta a pé ao carro ao fim de cada fase.

### Pilares de design

1. **Um golpe mata, dos dois lados.** Sem barra de vida, armadura ou escudo.
2. **Ritmo rápido.** A morte reinicia na sala atual, sem cena longa.
3. **Leitura da sala.** Pensar antes de agir, se esconder e atacar pelas costas.

---

## Controles

| Ação | Tecla |
|---|---|
| Mover | `W` `A` `S` `D` |
| Mirar | Mouse |
| Atacar | Botão esquerdo |
| Pegar arma | `E` |
| Reiniciar (tela de morte) | `R` |
| Pausa | `Esc` |

---

## Como rodar o projeto

**Pré-requisitos**

- Unity **6000.5.9f1** (a versão exata; outras podem gerar conflitos de cena e prefab)
- Git

**Passos**

1. Clone o repositório:
   ```bash
   git clone https://github.com/ailtonmcneto/Back-Seat.git
   ```
2. Abra a pasta no Unity Hub, usando a versão 6000.5.9f1.
3. Abra a cena `Assets/Scenes/Main.unity` e aperte **Play**.

Para testar uma parte isolada, use as cenas `Test_<nome>` em `Assets/Scenes/`.

---

## Tecnologias

- **Unity 6** (6000.5.9f1), projeto 2D com URP
- **C#**
- **Input System** (o projeto usa só o Input System novo; `Input.GetKey` e similares não funcionam aqui)
- **Tilemap** e Physics2D para mapas, colisão e linha de visão
- **Cinemachine** e **TextMeshPro** para câmera e telas (em instalação)

---

## Estrutura do repositório

```
Assets/
  Scenes/        Cena principal (Main) e cenas de teste por integrante
  Scripts/
    Contracts/   Interfaces e eventos compartilhados pelos três
    Player/      Movimento, mira e ataque do jogador
    Enemy/       Patrulha, detecção e alerta dos inimigos
    Game/        Fluxo da fase: sala limpa, saída, respawn, carro
    UI/          Telas, avisos e HUD
  Physics/       Materiais de física 2D
  Settings/      Configurações de URP e Input System
docs/
  contexto-jogo.md   Documento de design completo (GDD em uso pela equipe)
AGENTS.md            Regras de código e de trabalho do projeto
```

---

## Escopo

**MVP (até 04/12/2026)**

- Fase 1 (bar clandestino) jogável de ponta a ponta
- Fase 2 (mansão, com o chefão) como escopo desejável
- Movimento em 8 direções, mira no mouse, punho e bastão
- Inimigos com patrulha, linha de visão, interrogação, alerta e esquecimento
- Morte, respawn na sala, saída bloqueada até limpar a sala e volta ao carro
- Menu, telas de texto, música e efeitos sonoros (desejáveis)

**Fora do escopo do MVP**

- Fases 3 e 4, pistola, Thompson e largar arma
- Pathfinding (A\*) no alerta
- Shader próprio, estatísticas, tela de controles, save e opções

---

## Equipe

| Integrante | Responsabilidades principais |
|---|---|
| **Ailton Neto** | Setup do projeto, jogador (movimento, mira, ataque, morte), cena principal e salas |
| **Luis Felipe Veras (Lipe)** | Inimigos (patrulha, detecção, alerta, morte), interrogação, música e sons |
| **Arthur Tavares** | Câmera, fluxo da fase, telas, HUD, pausa e build |
| **Fernando Meira** | Pitch, história e demonstração da jogabilidade |

Cada pasta tem um dono, e as mudanças em arquivos compartilhados (como `Contracts.cs`) só entram com acordo dos três. O fluxo de trabalho e as regras estão em `AGENTS.md`.

---

## Contribuindo

1. Crie uma branch por tarefa, no formato `<nome>/<tarefa>` (ex.: `ailton/mira`).
2. Faça commits pequenos, com mensagem em português no imperativo.
3. Abra um PR usando o modelo em `.github/pull_request_template.md`.
4. O dono do PR faz o merge depois da revisão cruzada.

Antes de abrir o PR, o projeto precisa abrir sem erro no Console do Unity e a tarefa precisa funcionar na sua cena de teste.

---

## Créditos

- Arte: formas simples desenhadas pela equipe (a arte final pode usar pacotes CC0).
- Música e efeitos sonoros: serão listados em `CREDITS.md`, com autor e licença de cada arquivo (apenas CC0 ou CC-BY).

## Licença

Consulte o arquivo [LICENSE](LICENSE).