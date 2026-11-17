# Game — Arthur

Pasta do Arthur: câmera, GameManager, sala limpa, saída bloqueada, respawn, carro, áudio.

## Câmera acompanhando um alvo e olhando na direção do mouse

`CameraFollowTarget.cs` acompanha um objeto indicado no Inspector e desloca o ponto
observado na direção do mouse, em até **3 unidades** (3 tiles). O Cinemachine segue
esse ponto e controla a `Main Camera`, sem suavização nesta etapa.
O limite da sala fica para outra tarefa.

O objeto `CameraTarget` separa o ponto observado do personagem: só esse ponto
recebe o deslocamento pelo mouse, sem mover o personagem.
Não é preciso acessar a classe do jogador nem mudar `Contracts.cs`.

O mouse no centro da imagem deixa o alvo centralizado. Quanto mais longe do
centro, maior o deslocamento, até o limite de 3 unidades, inclusive na diagonal.
A referência é o centro da imagem, não a posição do personagem na tela: assim,
o movimento da própria câmera não aumenta o deslocamento a cada quadro.

### Como explicar o script

- `target` guarda o `Transform` do objeto a acompanhar. `[SerializeField]` permite
  preencher esse campo no Inspector mesmo sendo privado.
- `gameCamera` é o componente `Camera` da `Main Camera`, usado para converter
  pixels da tela em unidades do jogo. Não é a `FollowCamera` do Cinemachine.
- `lookAheadDistance` define o deslocamento máximo, com padrão de `3` unidades.
  O valor `0` permite testar o acompanhamento básico.
- `Update` lê `Mouse.current.position` usando o Input System e guarda o resultado.
- `LateUpdate` converte o ponto do mouse para o plano do alvo com
  `ScreenToWorldPoint`. A profundidade usada é a distância entre os valores Z
  do alvo e da câmera. Esse cálculo pressupõe câmera ortográfica, sem rotação.
- Subtrair a posição da câmera fornece o deslocamento em relação ao centro
  da visão. `Vector2.ClampMagnitude` limita seu comprimento a 3 unidades.
- Por fim, o script soma esse deslocamento XY à posição do alvo, preservando
  seu Z. A rotação do alvo não é copiada.
- `DefaultExecutionOrder(-100)` faz esta cópia acontecer antes do Cinemachine
  Brain, cuja ordem padrão é `100`. O Brain deve estar em `Late Update`.
- A verificação de `target == null` mantém o ponto parado se o alvo não estiver
  preenchido ou tiver sido destruído.
- Sem câmera atribuída, mouse conectado ou foco no jogo, o acompanhamento
  continua centralizado. O mesmo acontece se o cursor sair da área da câmera.

## Passos no editor

Use **Unity 6000.5.9f1**. Faça a montagem em `Assets/Scenes/Test_Arthur.unity`.

Se a câmera já está montada, ajuste apenas o seguinte antes de testar:

| Objeto | Componente / campo | Valor |
|---|---|---|
| `CameraTestTarget` | Transform > Position | `(0, 0, 0)`, especialmente Z = `0` |
| `CameraTarget` | Camera Follow Target > Target | `CameraTestTarget` |
| `CameraTarget` | Camera Follow Target > Game Camera | Arraste `Main Camera` |
| `CameraTarget` | Camera Follow Target > Look Ahead Distance | `3` |
| `Main Camera` | Transform > Rotation | `(0, 0, 0)` |
| `Main Camera` | Cinemachine Brain > Update Method / Blend Update Method | Ambos `Late Update` |
| `FollowCamera` | Cinemachine Follow > Follow Offset | `(0, 0, -10)` |
| `FollowCamera` | Cinemachine Follow > Position Damping | `(0, 0, 0)` |

Mantenha a projeção ortográfica com tamanho `6.75` e a Game em `16:9`.
Salve a cena fora do Play. Não precisa instalar outro pacote.

Para montar a cena do início:

1. Se o Cinemachine ainda não estiver instalado, abra
   **Window > Package Management > Package Manager**, selecione
   **Unity Registry**, procure **Cinemachine** e instale a versão estável **3.1.x**
   compatível oferecida pelo editor. O pacote já foi aprovado pela equipe;
   a instalação é manual, conforme o `AGENTS.md`.
2. Abra `Test_Arthur`. Na aba **Game**, selecione a proporção **16:9**.
3. Crie um **2D Object > Sprites > Square** chamado `CameraTestTarget`.
   Deixe **Position = (0, 0, 0)** e **Scale = (1, 1, 1)**. No **Sprite Renderer**,
   use **Color = branco** e **Material = Sprite-Unlit-Default**, para enxergar o
   quadrado sem depender de iluminação. Não precisa de script de movimento.
4. Crie outro quadrado chamado `CameraTestMarker`, com **Position = (4, 0, 0)**,
   **Scale = (1, 1, 1)**, **Color = cinza** e o mesmo material. Ele fica parado
   para servir como referência visual.
5. Crie um objeto vazio chamado `CameraTarget`, com **Position = (0, 0, 0)**,
   **Rotation = (0, 0, 0)** e **Scale = (1, 1, 1)**. Adicione o componente
   **Camera Follow Target** e arraste `CameraTestTarget` para o campo **Target**.
   Arraste `Main Camera` para **Game Camera** e deixe **Look Ahead Distance = 3**.
6. Selecione `Main Camera`. No **Transform**, use **Position = (0, 0, -10)** e
   **Rotation = (0, 0, 0)**. No componente **Camera**, use
   **Projection = Orthographic**, **Size = 6.75** e, em **Environment**,
   **Background Type = Solid Color** com fundo preto. Adicione
   **Cinemachine Brain** e configure **Update Method = Late Update** e
   **Blend Update Method = Late Update**.
7. Crie outro objeto vazio chamado `FollowCamera`, com
   **Position = (0, 0, -10)**, **Rotation = (0, 0, 0)** e **Scale = (1, 1, 1)**.
   Adicione **Cinemachine Camera** e configure:

   | Campo | Valor |
   |---|---|
   | Tracking Target | Arraste `CameraTarget` |
   | Lens > Orthographic Size | `6.75` |
   | Position Control | `Follow` |
   | Rotation Control | `None` |
   | Cinemachine Follow > Binding Mode | `World Space` |
   | Cinemachine Follow > Follow Offset | `(0, 0, -10)` |
   | Cinemachine Follow > Position Damping | `(0, 0, 0)` |

   Se o tamanho ortográfico não aparecer, confira primeiro a projeção da
   `Main Camera`. Deixe os demais campos nos valores padrão.
8. Mantenha esses objetos separados na raiz da Hierarchy: `CameraTarget` e as
   câmeras não devem ser filhos do alvo. Salve a cena antes de entrar em Play.

O tamanho ortográfico é metade da altura visível: `6.75 × 2 = 13.5` unidades.
Em 16:9, isso dá `24` unidades de largura, conforme o GDD. Pixel Perfect será
configurado em outra tarefa.

## Teste manual

1. Entre em **Play**, clique na aba **Game** para dar foco e coloque o mouse
   no centro da imagem. O quadrado branco deve ficar aproximadamente centralizado.
2. Mova o cursor para perto da borda direita, na metade da altura. A câmera deve
   olhar até 3 unidades à direita; o quadrado aparece à esquerda do centro.
   Repita à esquerda, acima e abaixo. O marcador cinza ajuda a perceber o movimento.
3. Pare o mouse perto de um canto por alguns segundos. A câmera deve permanecer
   estável, sem continuar se afastando. O limite total na diagonal também é de
   3 unidades, não 3 em cada eixo. Volte ao centro para recentralizar.
4. Durante o Play, coloque **Look Ahead Distance = 0**. Altere a posição de
   `CameraTestTarget` para **(3, 2, 0)**: a `Main Camera` deve chegar a
   **(3, 2, -10)**. Coloque **Rotation Z = 45** no alvo: a câmera mantém rotação zero.
5. Restaure **Look Ahead Distance = 3**, volte à Game e mova o mouse para a mesma
   região usada antes. O deslocamento relativo ao alvo deve ser o mesmo,
   mesmo depois de mudar a posição do alvo. O Z da câmera continua em `-10`.
6. Limpe **Game Camera** no script e volte à Game: o alvo fica centralizado,
   sem erro no Console. Reatribua `Main Camera` para retomar o deslocamento.
   Sem mouse ou com o cursor fora da Game, também deve acompanhar sem deslocamento.
7. Limpe **Target** durante o Play: a câmera fica na última posição, sem erros.
   Arraste `CameraTestTarget` de volta para o campo para retomar.
8. Saia do Play. As alterações feitas durante o teste são descartadas.
   Confirme que o Console está sem erros antes de abrir o PR.

A compilação isolada do script não substitui esse teste na cena. A instalação
do pacote gera alterações em `Packages/`; o Unity também gera o `.meta` do script.
Essas mudanças devem ser conferidas pelo Arthur junto com a cena antes do PR.
A integração na `Main` é feita pelo Ailton, seguindo a mesma configuração e
substituindo `CameraTestTarget` pelo Transform do jogador no campo `Target`
e atribuindo a câmera da cena ao campo `Game Camera`.

## Referências

- [Cinemachine Camera 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCamera.html)
- [Cinemachine Follow 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineFollow.html)
- [Cinemachine Brain 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrain.html)
- [DefaultExecutionOrder](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/DefaultExecutionOrder.html)
- [Camera.ScreenToWorldPoint](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ScreenToWorldPoint.html)
- [Vector2.ClampMagnitude](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2.ClampMagnitude.html)
