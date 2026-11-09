# AGENTS.md — BACKSEAT

Regras para qualquer agente de código (Claude Code, Codex) e para os humanos da equipe.
Leia tudo antes de mexer em qualquer arquivo.

## O projeto

- Jogo 2D top-down, preto e branco, ambientação noir. **Um golpe mata, dos dois lados.**
- Unity + C# (2D, URP). A versão do Unity está em `ProjectSettings/ProjectVersion.txt`. Não troque de versão.
- Equipe de iniciantes em Unity e C#. **O código precisa ser simples o bastante para o dono explicar na apresentação.**
- Apresentação em 07/12. Versão final em 04/12. Não há tempo para refatorações grandes.

## Escopo: o que NÃO implementar

Não escreva código para nada disto, mesmo que pareça útil:

- Pistola, Thompson, projéteis, munição
- Largar arma (tecla Q)
- Pathfinding (A\*, NavMesh): o inimigo em alerta anda em linha reta até o último ponto conhecido e para se bater na parede
- Shaders próprios (o preto e branco vem da arte já desenhada em cinza)
- Fases 3 e 4, estatísticas, tela de controles, save, opções
- Som posicional; som que alerta inimigo

Se uma tarefa parecer exigir algo da lista, pare e pergunte.

## Donos

| Pasta / arquivo | Dono |
|---|---|
| `Assets/Scripts/Player/`, prefab `Player`, prefab `WeaponPickup` | Ailton |
| `Assets/Scripts/Enemy/`, prefab `Enemy` | Lipe |
| `Assets/Scripts/Game/`, `Assets/Scripts/UI/`, prefabs de câmera, GameManager e telas | Arthur |
| `Assets/Scripts/Contracts/Contracts.cs` | Os três, só com acordo |
| Cena `Assets/Scenes/Main.unity` | Ailton |
| `Assets/Scenes/Test_<Nome>.unity` | Cada um a sua |
| `ProjectSettings/` (tags, camadas, input, versão) | Ailton |

**O agente só cria ou altera arquivos dentro da pasta de quem pediu a tarefa.** Se a tarefa precisar de mudança na pasta de outra pessoa, no `Contracts.cs` ou em `ProjectSettings/`, não faça: descreva a mudança necessária no fim da resposta.

## Como as partes se conectam

Toda comunicação entre jogador, inimigo e jogo passa pelo `Contracts.cs`:

- **`IHittable.ReceiveHit(Vector2 from)`**: quem ataca procura `IHittable` no que acertou e chama. Nunca referencie diretamente a classe do jogador ou do inimigo de fora da pasta dela.
- **`GameEvents`**: avisos entre as partes (`PlayerDied`, `EnemyDied`, `RoomCleared`, `PlayerReachedCar`). Quem avisa chama `Raise...`. Quem escuta se inscreve em `OnEnable` e se desinscreve em `OnDisable`, sempre.
- **`Names`**: use as constantes para tags e camadas. Nunca escreva `"Player"` ou `"Walls"` soltos no código.

Para testar sem a parte do colega, use uma peça falsa na sua cena de teste, nunca código do colega pela metade.

## Arquivos que o agente não edita

- **`.unity`, `.prefab`, `.asset`, `.meta`**: não edite como texto. O agente escreve scripts; a ligação no editor (arrastar componente, preencher campo do Inspector, criar prefab) é feita pela pessoa.
- No fim de toda tarefa, liste em **"Passos no editor"** exatamente o que a pessoa precisa fazer no Unity: em qual objeto, qual componente, qual campo, qual valor.

## Convenções de C#

- Nomes de classes, métodos e variáveis em **inglês**; comentários em **português**.
- `PascalCase` para classes e métodos, `camelCase` para variáveis. Um `MonoBehaviour` por arquivo, com o mesmo nome do arquivo.
- Campos ajustáveis: `[SerializeField] private float detectionTime = 0.8f;` com o valor do GDD como padrão. Nada de números mágicos no meio do código.
- Física: `Rigidbody2D` e `Collider2D`. Movimento do jogador e do inimigo em `FixedUpdate`, leitura de input em `Update`.
- Linha de visão: `Physics2D.Linecast` com máscara da camada `Walls`.
- Nada de pacote novo, plugin ou asset da Asset Store sem acordo da equipe (exceção já aceita: Cinemachine, TextMeshPro, URP).
- Prefira código curto e direto a padrões elaborados. Sem herança profunda, sem injeção de dependência, sem `async`.

## Valores do GDD

| Parâmetro | Valor |
|---|---|
| Escala | 1 tile = 1 unidade do Unity; sprites de 16 px, 16 pixels por unidade |
| Câmera | ~24 × 13,5 tiles visíveis; look-ahead de ~3 tiles na direção do mouse; trava na borda da sala |
| Sala | 36 × 23 tiles |
| Velocidade do jogador | ~6 tiles/s (provisório) |
| Detecção | ~0,8 s de linha de visão contínua |
| Esquecimento | ~5 s sem linha de visão |
| Alcance de visão do inimigo | ~6,5 tiles |
| Pegar arma | até 1 tile de distância, tecla E |
| Área de acerto do jogador | 10 × 10 px dentro do sprite de 16 × 16 (0,625 × 0,625 unidade) |
| Cores | preto, branco e cinzas; vermelho `#C8102E` só em sangue e interrogação |

Controles: WASD mover, mouse mirar, botão esquerdo atacar, E pegar, R reiniciar (tela de morte), ESC pausa.

## Forma de trabalho com o agente

- **Uma tarefa pequena por sessão** (ex.: "movimento em 8 direções"). Não emende tarefas.
- Antes de escrever, diga em poucas linhas o plano e quais arquivos vai criar ou alterar.
- Depois de escrever, **explique o código para o dono**, como para um iniciante: o que cada parte faz e por quê. O dono precisa conseguir explicar na apresentação.
- Se algo do pedido contradizer este arquivo, pare e pergunte.

## Git

- Branch por tarefa: `<nome>/<tarefa>`, ex.: `ailton/movimento`, `lipe/patrulha`.
- Commits pequenos, mensagem em português no imperativo: "Adiciona mira no mouse".
- **O agente não faz merge na `main`, não faz `push --force` e não apaga branch.**
- Todo PR usa o modelo `.github/pull_request_template.md`. O agente pode rascunhar a descrição; o dono revisa e completa a parte "Explicação".
- Revisão cruzada: Ailton revisa Lipe, Lipe revisa Arthur, Arthur revisa Ailton. O dono do PR faz o merge e resolve os próprios conflitos.
- Depois de 04/12, nada entra na `main`.

## Antes de abrir o PR

- O projeto abre sem erro no Console do Unity.
- A tarefa funciona na sua cena de teste, em Play Mode.
- Nenhum arquivo fora da sua pasta foi alterado (confira com `git status`).
