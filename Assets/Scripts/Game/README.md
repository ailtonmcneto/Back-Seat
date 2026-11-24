# Game — Arthur

Pasta do Arthur: câmera, GameManager, sala limpa, saída bloqueada, respawn, carro, áudio.

## Câmera: acompanhamento, mouse, limites e Pixel Perfect

`CameraFollowTarget.cs` acompanha um objeto indicado no Inspector e desloca o ponto
observado na direção do mouse, em até **3 unidades** (3 tiles). O Cinemachine segue
esse ponto e controla a `Main Camera`, sem suavização nesta etapa.
O limite da sala usa o componente **Cinemachine Confiner 2D**, já disponível no
Cinemachine 3.1.7 instalado. A montagem está na seção **Limites da sala** abaixo;
ela precisa ser feita e salva pelo Arthur no editor.

O objeto `CameraTarget` separa o ponto observado do personagem: só esse ponto
recebe o deslocamento pelo mouse, sem mover o personagem.
Não é preciso acessar a classe do jogador nem mudar `Contracts.cs`.

O mouse no centro da imagem deixa o alvo centralizado. Quanto mais longe do
centro, maior o deslocamento, até o limite de 3 unidades, inclusive na diagonal.
A referência é o centro da imagem, não a posição do personagem na tela: assim,
o movimento da própria câmera não aumenta o deslocamento a cada quadro.
Perto das bordas, o confinador tem prioridade: o alvo pode sair do centro da
imagem para que a câmera mantenha toda a área visível dentro da sala.

### Como explicar o script

- `target` guarda o `Transform` do objeto a acompanhar. `[SerializeField]` permite
  preencher esse campo no Inspector mesmo sendo privado.
- `gameCamera` é o componente `Camera` da `Main Camera`, que informa o tamanho
  da visão e sua área final na tela. Não é a `FollowCamera` do Cinemachine.
- `lookAheadDistance` define o deslocamento máximo, com padrão de `3` unidades.
  O valor `0` permite testar o acompanhamento básico.
- `Update` lê `Mouse.current.position` usando o Input System e guarda o resultado.
- `LateUpdate` usa `gameCamera.pixelRect`, a área final da imagem na tela,
  já descontadas as barras pretas do Pixel Perfect. Mouse nas barras não desloca a visão.
- O dobro de `orthographicSize` é a altura visível em unidades. Dividir pela
  altura da imagem em pixels fornece quantas unidades cada pixel da tela representa.
- Multiplicar a distância do mouse ao centro da imagem por esse valor fornece
  o deslocamento em unidades do mundo. `Vector2.ClampMagnitude` limita a 3 unidades.
  Esse cálculo pressupõe câmera ortográfica e sem rotação.
- A conversão usa o retângulo da imagem, sem depender da matriz de visão
  arredondada pelo Pixel Perfect. Assim, esse arredondamento não realimenta o
  cálculo do ponto que a câmera segue quando o mouse fica parado.
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
Em 16:9, isso dá `24` unidades de largura, conforme o GDD. A seção **Pixel Perfect**
configura essa mesma visão com resolução interna de 384 × 216.

## Limites da sala

### Como funciona

O script continua calculando onde a câmera quer olhar. O **Cinemachine Confiner 2D**
ajusta a posição final para que as quatro bordas da imagem caibam dentro do
contorno da sala, considerando o tamanho ortográfico e a proporção da Game.
Esse comportamento já existe no pacote: não é necessário criar outro script C#.

Para a sala retangular do MVP, use um **Box Collider 2D**, aceito pelo Cinemachine
3.1.7. O collider fornece um retângulo de 36 × 23 unidades. **Is Trigger** evita
que ele bloqueie o jogador; a camada **Ignore Raycast**, já existente, mantém
esse limite fora da camada `Walls` usada para a visão dos inimigos.

### Passos no editor

1. Abra `Test_Arthur` fora do Play e conclua a configuração anterior:
   `CameraTestTarget` em **(0, 0, 0)**, `CameraTarget > Game Camera = Main Camera`
   e **Look Ahead Distance = 3**.
2. Crie um objeto vazio na raiz da Hierarchy chamado **CameraBounds**.
   No Transform, coloque **Position = (0, 0, 0)**, **Rotation = (0, 0, 0)**
   e **Scale = (1, 1, 1)**. Defina **Layer = Ignore Raycast** e deixe **Tag = Untagged**.
   Mantenha esse objeto independente do alvo e das câmeras, para o limite ficar parado.
3. Adicione **Box Collider 2D** a `CameraBounds` e configure:

   | Campo | Valor |
   |---|---|
   | Is Trigger | Marcado |
   | Offset | X = `0`, Y = `0` |
   | Size | X = `36`, Y = `23` |
   | Edge Radius | `0` |

   Não é necessário adicionar Rigidbody 2D. O retângulo vai de X = -18 a 18
   e de Y = -11.5 a 11.5. Na aba Scene, selecione o objeto e ative Gizmos para vê-lo.
4. Selecione **FollowCamera**, que contém o componente **Cinemachine Camera**.
   Use **Add Component > Cinemachine Confiner 2D**. Configure:

   | Campo | Valor |
   |---|---|
   | Bounding Shape 2D | Arraste `CameraBounds` (seu Box Collider 2D) |
   | Damping | `0` |
   | Slowing Distance | `0` |
   | Oversize Window | Desativado |

5. Confira **FollowCamera > Lens > Orthographic Size = 6.75**,
   **Cinemachine Follow > Position Damping = (0, 0, 0)**,
   **Follow Offset = (0, 0, -10)** e **Rotation Control = None**.
   Mantenha `Main Camera` ortográfica, ambas as câmeras sem rotação, os métodos
   do Brain em **Late Update** e a Game em **16:9**.
6. No **Cinemachine Confiner 2D**, clique em **Invalidate Bounding Shape Cache**
   depois de definir o tamanho do collider. Salve a cena com **Ctrl+S**.
   Repita esse botão se mudar o tamanho ou a forma do limite. Para este teste,
   configure tamanho da câmera e collider fora do Play e mantenha-os fixos durante o teste.

Esses valores pressupõem a sala centrada em (0, 0). Na integração, Ailton deve
posicionar `CameraBounds` no centro real da sala. O retângulo precisa representar
a região desenhada que a câmera pode mostrar; ele não cria a arte nem as paredes.
Uma sala de outra fase ou troca de sala será tratada na tarefa de fluxo do jogo.

### Teste manual dos limites

Primeiro teste sem deslocamento pelo mouse: coloque **Look Ahead Distance = 0**,
entre em Play e altere a posição de `CameraTestTarget` pelo Inspector.
Observe a posição de **Main Camera**, que é a câmera que desenha a imagem.

| Position do alvo | Position esperada da Main Camera |
|---|---|
| `(0, 0, 0)` | `(0, 0, -10)` |
| `(17, 0, 0)` | Aproximadamente `(6, 0, -10)` |
| `(-17, 0, 0)` | Aproximadamente `(-6, 0, -10)` |
| `(0, 10, 0)` | Aproximadamente `(0, 4.75, -10)` |
| `(0, -10, 0)` | Aproximadamente `(0, -4.75, -10)` |
| `(17, 10, 0)` | Aproximadamente `(6, 4.75, -10)` |
| `(-17, -10, 0)` | Aproximadamente `(-6, -4.75, -10)` |

A conta é: metade da sala `(18, 11.5)` menos metade da imagem `(12, 6.75)`.
Portanto, o centro da câmera fica entre **-6 e 6 no X**, **-4.75 e 4.75 no Y**.
Os valores podem ter uma diferença pequena por arredondamento.

Depois, ainda em Play, coloque **Look Ahead Distance = 3** e:

1. Com o alvo em **(17, 10, 0)**, dê foco à Game e aponte o mouse para o canto
   superior direito. A câmera deve continuar limitada à sala e estável.
2. Repita no canto oposto com o alvo em **(-17, -10, 0)** e o cursor no canto
   inferior esquerdo da Game. O deslocamento pelo mouse também respeita o limite.
3. Volte o alvo para **(0, 0, 0)** e mova o mouse na Game: o deslocamento de até
   3 unidades deve funcionar novamente. O ponto `CameraTarget` pode ficar fora
   da região permitida para o centro da câmera; o confinador corrige a posição final.
4. Saia do Play, deixe **Look Ahead Distance = 3** fora do Play e salve.
   Confirme que o Console está sem erros.

Sem Pixel Perfect, o teste pressupõe a Game em 16:9: mudar essa proporção altera
a largura da imagem e o intervalo permitido no X. Com **Windowbox** configurado
abaixo, a imagem mantém 16:9 e usa barras para ocupar o espaço restante da tela.
O enquadramento precisa caber dentro da sala; habilitar Oversize Window não faz
uma imagem maior caber em uma sala menor.

## Pixel Perfect

### Como funciona

O componente **Pixel Perfect Camera** já faz parte do URP instalado.
Com **384 × 216** pixels de referência e **16 pixels por unidade**, ele desenha
uma imagem de **24 × 13.5 unidades**. **Upscale Render Texture** faz o desenho
numa textura pequena; **Windowbox** amplia essa imagem por um número inteiro e
centraliza o resultado com barras pretas quando sobra espaço.

A extensão **Cinemachine Pixel Perfect** coordena o tamanho ortográfico da
`FollowCamera` com esse componente. O Cinemachine ajusta a lente antes do
confinamento, para que o limite considere o tamanho da imagem correto.
O script `CameraFollowTarget` usa a área final da imagem para interpretar o mouse.

### Passos no editor

1. Abra `Test_Arthur` fora do Play. Selecione **Main Camera** e use
   **Add Component > Pixel Perfect Camera** (o componente do URP).
   Não é necessário instalar outro pacote.
2. No **Pixel Perfect Camera**, configure:

   | Campo | Valor |
   |---|---|
   | Assets Pixels Per Unit | `16` |
   | Reference Resolution | X = `384`, Y = `216` |
   | Crop Frame | `Windowbox` |
   | Grid Snapping | `Upscale Render Texture` |

   Nesta versão, **Filter Mode** só aparece ao escolher **Stretch Fill**.
   Mantenha **Windowbox**: é essa opção que preserva a ampliação inteira com barras.
   O Renderer 2D usa filtragem Point na textura interna desse caminho.
3. Na **Main Camera**, mantenha **Projection = Orthographic**,
   **Rotation = (0, 0, 0)**, **Render Type = Base**, **Post Processing desmarcado**
   e **Anti-aliasing = None**. Use o renderer 2D padrão do projeto e mantenha a
   lista de câmeras da pilha vazia. Os métodos do Brain continuam em **Late Update**.
4. Selecione **FollowCamera**. No componente **Cinemachine Camera**, abra
   **Add Extension** e escolha **CinemachinePixelPerfect**. A extensão não tem
   campos para preencher. Mantenha **Lens > Orthographic Size = 6.75**.
5. Confira **CameraTarget > Camera Follow Target > Game Camera = Main Camera**
   e **Look Ahead Distance = 3**. Mantenha o alvo em Z = `0`.
6. No **Cinemachine Confiner 2D**, mantenha **Bounding Shape 2D = CameraBounds**,
   **Damping = 0**, **Slowing Distance = 0**, **Oversize Window desativado**.
   Clique em **Invalidate Bounding Shape Cache** após a configuração e salve a cena.

Os nomes acima correspondem ao Inspector do URP 17.6.0 e Cinemachine 3.1.7
instalados. O projeto já está com Render Scale = 1 e MSAA desativado; esta tarefa
não muda os assets de renderização nem ProjectSettings.

### Teste em resoluções diferentes

Na aba **Game**, abra a lista de proporções/resoluções e use **+** para adicionar
entradas do tipo **Fixed Resolution**, com os tamanhos abaixo. Entre em Play
e selecione cada resolução. Dê foco à Game para o mouse funcionar.

| Resolução da Game | Escala esperada | Área da imagem | Barras de cada lado |
|---|---|---|---|
| `768 × 432` | `2:1` | `768 × 432` | Nenhuma |
| `1280 × 720` | `3:1` | `1152 × 648` | 64 px à esquerda/direita; 36 px acima/abaixo |
| `1920 × 1080` | `5:1` | `1920 × 1080` | Nenhuma |
| `1024 × 768` | `2:1` | `768 × 432` | 128 px à esquerda/direita; 168 px acima/abaixo |

1. Veja **Current Pixel Ratio** no **Pixel Perfect Camera** durante o Play.
   Deve coincidir com a tabela. Para avaliar a nitidez, use a prévia da Game
   em escala **1x**; uma prévia reduzida pelo editor pode parecer borrada.
2. Em cada resolução, teste o mouse no centro da **imagem**, nos lados e nas
   barras pretas. Nas barras, o deslocamento deve voltar a zero; o confinador
   ainda tem prioridade se o alvo estiver perto da borda da sala.
3. Com o alvo em **(0, 0, 0)**, mantenha o mouse parado perto do centro da imagem
   por alguns segundos: o alvo da câmera deve permanecer estável. O movimento
   visível da câmera acontece em passos de pixels, como esperado nesse modo.
4. Repita o teste dos limites nos quatro lados e cantos, incluindo **1024 × 768**.
   A imagem útil esperada é 16:9 mesmo nessa resolução. Com **Look Ahead Distance = 0**
   e o alvo em **(17, 10, 0)**, confirme que a `Main Camera` para perto de
   **(6, 4.75, -10)** e que nenhuma borda revela a área externa à sala.
5. Saia do Play e confira o Console. Salve as configurações fora do Play.

O quadrado de teste permite verificar enquadramento, barras e movimento.
Para conferir a arte final pixel a pixel, Ailton deve importar os sprites com
**Pixels Per Unit = 16**, **Filter Mode = Point (no filter)** e **Compression = None**.
Essa configuração pertence aos assets de arte e não foi alterada pelo agente.

### Explicação para a apresentação

O jogo desenha uma imagem de 384 × 216. Cada 16 pixels representam uma unidade
da sala. A imagem é ampliada 2, 3, 4 ou mais vezes inteiras, para que cada pixel
ocupe um bloco regular na tela. O espaço que sobra fica preto. O Cinemachine
Pixel Perfect mantém esse tamanho compatível com a câmera que acompanha o jogador.

## Teste manual do acompanhamento e do mouse

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
5. Restaure **Look Ahead Distance = 3**, coloque o alvo em **(1, 1, 0)**,
   volte à Game e mova o mouse para a mesma região usada antes. Nessa região
   central da sala, o deslocamento relativo ao alvo deve ser o mesmo,
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
Para o confinamento, Ailton também deve criar o `CameraBounds` da sala e atribuí-lo
ao **Cinemachine Confiner 2D** da `FollowCamera`, conforme a seção acima.
Para Pixel Perfect, também deve adicionar os dois componentes e aplicar os
valores da seção **Pixel Perfect** na câmera principal e na `FollowCamera` da Main.

### Validação desta etapa

A configuração de confinamento já consta na cena salva. Para Pixel Perfect,
os campos e a ampliação foram conferidos contra o código e o Inspector do URP
17.6.0 e Cinemachine 3.1.7 instalados. O cálculo do mouse foi ajustado para usar
o retângulo final da imagem e não depender da matriz arredondada pelo Pixel Perfect.
A montagem do Pixel Perfect e a validação visual em Play Mode continuam pendentes
até o Arthur executar e salvar os passos no editor.

## Referências

- [Cinemachine Camera 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCamera.html)
- [Cinemachine Follow 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineFollow.html)
- [Cinemachine Brain 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrain.html)
- [Cinemachine Confiner 2D 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineConfiner2D.html)
- [Cinemachine Pixel Perfect 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePixelPerfect.html)
- [DefaultExecutionOrder](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/DefaultExecutionOrder.html)
- [Camera.pixelRect](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-pixelRect.html)
- [Vector2.ClampMagnitude](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2.ClampMagnitude.html)
