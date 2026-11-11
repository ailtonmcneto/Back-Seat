# Contexto — BACKSEAT (Projeto Extensionista)

> Anexe este arquivo no início de uma nova conversa para retomar o projeto de onde paramos.
> Última atualização: 10/11/2026 — **Unity + C#. Setup feito e movimento do jogador pronto (PR aberto). Foco total no MVP (seção 3).**
>
> **Como ler este arquivo:** toda seção separa **MVP** (o que será feito até 04/12) de **Pós-MVP** (desenho do GDD que só entra se sobrar tempo depois do MVP pronto). Na dúvida, vale o MVP.

---

## 1. Situação do projeto

- **Equipe ativa:** **Ailton Neto**, **Luis Felipe Veras (Lipe)**, **Arthur Tavares**. Alunos do 2º período de Ciência da Computação, faculdade privada, João Pessoa (PB).
- **Fernando Meira:** sem tarefas de desenvolvimento desde 26/10. **Apresenta o jogo ao professor em 07/12: pitch, história e demonstração da jogabilidade.**
- **Entrega:** jogo do **Projeto Extensionista**. **Apresentação em 07/12/2026. Versão final gravada em 04/12.**
- **GDD:** entregue e avaliado pelo professor; aguardando nota. O GDD avaliado cita LibGDX; a troca de engine precisa ser avisada ao professor.
- **Experiência:** nenhuma com jogos, Unity ou C#.
- **Tempo:** 3h/semana por pessoa (~30h de equipe até 04/12). O estudo de Java (Portão 0) continua fora deste projeto.
- **Estado em 10/11:**
  - **Setup do projeto concluído (Ailton, 09/11):** Unity **6000.5.9f1**, 2D URP, `.gitignore` do Unity, tag `Enemy`, camadas `Walls` (6) e `Weapon` (7), cenas `Main`, `Test_Ailton`, `Test_Arthur`, `Test_Lipe`, pastas `Scripts/{Contracts,Player,Enemy,Game,UI}` com `README.md`, `Contracts.cs`, `AGENTS.md`, `CLAUDE.md`, modelo de PR e `docs/contexto-jogo.md` na `main`.
  - **Movimento do jogador pronto e testado:** `PlayerMovement.cs` na branch `ailton/movimento`, PR aberto para a `main` com o Arthur como revisor. Merge previsto na reunião de quarta (11/11).
  - Arthur e Lipe ainda precisam clonar e abrir o projeto (gatilho de 11/11).
  - Cinemachine e TextMeshPro ainda não instalados (o Arthur instala na câmera e nas telas).

---

## 2. Stack e ferramentas

- **Unity + C#**, projeto 2D com URP. **Mesma versão LTS nos três computadores** (registrada em `ProjectSettings/ProjectVersion.txt`).
- **Versão: Unity 6000.5.9f1** (os três precisam da mesma).
- **Pacotes:** URP e Input System instalados; Cinemachine e TextMeshPro aceitos, ainda não instalados. Nada além disso sem acordo da equipe.
- **Input: só o Input System novo** (`Keyboard.current`, `Mouse.current`). O jeito antigo (`Input.GetKey`) dá erro neste projeto.
- **Código com Claude Code e Codex**, guiados pelo `AGENTS.md` do repositório (o `CLAUDE.md` só aponta para ele).
- **Git + GitHub**, com `.gitignore` padrão do Unity.

| Necessidade do jogo | Como fazer em Unity |
|---|---|
| Mapas das salas | **Tilemap**, com Tilemap Collider 2D + Composite Collider 2D na camada `Walls` |
| Colisão | Rigidbody2D + Collider2D |
| Linha de visão | `Physics2D.Linecast` contra a camada `Walls` |
| Câmera com look-ahead e trava nas bordas | Cinemachine + confinador 2D + script curto de look-ahead |
| Resolução pixel perfect | Pixel Perfect Camera, referência 384×216, 16 pixels por unidade |
| Patrulha | Lista de pontos (Transform) no Inspector do inimigo |
| Respawn | Reiniciar a sala e reposicionar o jogador no ponto de spawn |
| Vinheta e granulação | Pós-processamento do URP (Volume) |
| Telas e HUD | Canvas + TextMeshPro |
| Escala | 1 tile = 1 unidade do Unity; sprites de 16 px |

---

## 3. Escopo

| Nível | O quê | Prazo |
|---|---|---|
| **MVP obrigatório (Marco A)** | **Fase 1** (sala 1.1) jogável de ponta a ponta | 22/11 |
| **MVP desejável** | **Fase 2** (salas 2.1 e 2.2); música e sons; HUD; pausa; vinheta e granulação; telas de texto; pausa de impacto e tremor | 29/11 |
| **Pós-MVP** | Fases 3 e 4; pistola; Thompson; largar arma; A\*; shader de cor reservada; estatísticas; tela de controles; som posicional | Só se o MVP estiver pronto e sobrar tempo |

**O MVP contém:** movimento em 8 direções; mira no mouse; punho e bastão; um golpe mata; inimigos com patrulha, linha de visão, interrogação, alerta e esquecimento; inimigo morto larga o bastão e o jogador pega com E; morte, "[R] para reiniciar" e respawn na sala; saída bloqueada até limpar a sala; volta ao carro; tela de fim; menu; arte de formas já em cinza, com vermelho só em sangue e interrogação.

O corpo a corpo, a interrogação e o esquecimento nunca são cortados.

---

## 4. Pitch

> Jogo de ação **top-down** em **preto e branco, ambientação noir**. O jogador limpa salas cheias de inimigos com uma regra brutal: **um único golpe mata — o inimigo e também você.** Vencer depende de reflexo, movimento e leitura do ambiente, não de resistência.

**Nome: BACKSEAT.** Remete ao banco de trás vazio do carro da emboscada, motivo que abre e fecha a história.

**Gênero:** ação top-down com furtividade leve. "Hotline Miami encontra Mark of the Ninja."

---

## 5. Pilares de design

1. **Um golpe mata, dos dois lados.** Recusa barra de vida, armadura, escudo.
2. **Ritmo frenético.** Morreu, reinicia na hora. Recusa cena no meio da fase, animação de morte longa, checkpoint distante.
3. **Leitura da sala.** Pensar antes de agir, esconder-se e atacar pelas costas. Recusa perseguição infinita, ataque de fora da tela, vidro ou meia parede.

**Direção de estilo:** "noir de 16 pixels: preto, branco e sangue."

Filtro de decisão: ideia nova que não fortalece um pilar, corta.

---

## 6. Core loop

> Entrar na sala → ler onde estão os inimigos → posicionar-se → atacar (um golpe mata) → pegar a arma que caiu → limpar a sala → seguir para a próxima sala → ao fim da fase, voltar a pé ao carro. Morreu, reinicia na sala atual.

---

## 7. Estrutura de fases

### MVP
| Fase | Lugar | Salas | Arma da fase | Jogador começa com |
|---|---|---|---|---|
| 1 | Bar clandestino (porão do Harlow's) | 1.1 | Bastão | Punho |
| 2 | Mansão | 2.1, 2.2 | Bastão | Punho |

### Pós-MVP
Desenho original do GDD, só se sobrar tempo: 4 fases (bar → docas → cassino → mansão), com pistola a partir da fase 2 e Thompson a partir da fase 3.

### Regras (MVP)
- **Arma zera entre fases** (recomeça no punho) e **persiste entre salas** da mesma fase.
- **Todos os inimigos de uma sala carregam a mesma arma.**
- **Volta ao carro:** ao limpar a última sala, o jogador volta a pé pelas salas limpas até o carro, na entrada. Contraste entre violência e caminhada vazia. Precisa ser curta.
- Salas já limpas mantêm o estado (corpos, armas no chão).

---

## 8. Mecânicas — Jogador

### MVP
- **Movimento:** 8 direções, velocidade constante (~6 tiles/s, provisório), sem aceleração. Sem dash.
- **Mira:** o personagem olha para o mouse. Mover e mirar são independentes.
- **Ataque:** botão esquerdo. Golpe em arco curto à frente, com intervalo mínimo entre ataques.
- **Morte:** um acerto mata. Tela de morte; o jogador aperta R para reiniciar.
- **Respawn:** no início da sala atual, com a arma que tinha ao chegar nela.
- **Pegar arma:** tecla E, a até 1 tile.
- **Punho sempre disponível.**
- **Área de acerto do jogador:** 10×10 px dentro do sprite de 16×16.

### Pós-MVP
- Largar arma (Q).
- Tiro em projétil na direção do mouse; respawn com munição.

---

## 9. Mecânicas — Inimigos

### MVP
**Três estados:** patrulha → alerta → morto.

- **Patrulha:** percorre pontos fixos definidos na sala.
- **Detecção em dois estágios:**
  - **Contador de detecção (~0,8 s de linha de visão contínua):** enche → o inimigo identifica o jogador, ataca e **alerta a sala inteira**. Não enche → nada acontece.
  - **Interrogação** acima do inimigo, enchendo de branco para vermelho conforme a detecção sobe.
  - **Contador de esquecimento (~5 s sem linha de visão):** enche → volta a patrulhar.
  - Os dois contadores usam a mesma verificação de linha de visão.
- **Alerta:** quem identifica guarda o **último ponto conhecido** do jogador, e **todos os inimigos da sala vão até esse ponto em linha reta** (param se baterem na parede).
  - Com linha de visão: atualiza o ponto e ataca com o bastão.
  - Sem linha de visão: vai ao ponto e roda o contador de esquecimento.
  - **Só a visão alerta.** Barulho não alerta.
- **Linha de visão:** bloqueada por qualquer parede. Alcance máximo **~6,5 tiles**.
- **Morto:** larga a arma no chão e o corpo fica na sala.

### Pós-MVP
- Pathfinding A\* no alerta (em vez de linha reta).
- Inimigo que mira e atira (pistola, Thompson).

---

## 10. Armas

### MVP
| Arma | Tipo | Alcance | Cadência |
|---|---|---|---|
| Punho | Corpo a corpo | Muito curto | Lenta |
| Bastão | Corpo a corpo | Curto (maior que o punho) | Rápida |

### Pós-MVP
| Arma | Tipo | Alcance | Cadência | Munição |
|---|---|---|---|---|
| Pistola | Tiro | Longo | Média | 6-8 |
| Thompson | Tiro | Longo | Rajada | 20-30 |

Regras das armas de fogo (pós-MVP): um pente só, sem recarga; bala para no primeiro alvo; renascer com a arma de chegada transforma munição em investimento. Problema conhecido: sala de tiro com o jogador no bastão, resolvido por level design (cobertura e ataque pelas costas), tempo de reação e mira imperfeita do inimigo.

---

## 11. Level design

### Câmera (MVP)
- Acompanha o jogador. Tela visível **~24 × 13,5 tiles**. **Look-ahead de ~3 tiles** na direção do mouse. Trava nas bordas da sala.

### Layout (MVP)
- **Corrente de salas:** carro → sala 1 → sala 2. Vãos são aberturas na parede.
- **Saída bloqueada até a sala ser limpa:** um bloqueio sólido sobre o vão some quando o último inimigo morre.
- **Tamanho padrão: 36 × 23 tiles** para todas as salas.

### Inimigos por sala
| Sala | Arma | Inimigos | Escopo |
|---|---|---|---|
| 1.1 | Bastão | 5 | **MVP obrigatório** |
| 2.1 | Bastão | 5 | **MVP desejável** |
| 2.2 | Bastão | 8 (sala final, com o chefão) | **MVP desejável** |
| Fases 3 e 4 | Pistola / Thompson | — | Pós-MVP |

- Sala 1.1 ensina a regra: patrulhas bem espaçadas, nunca os 5 de uma vez.
- **Chefão (sala 2.2):** um dos 8 inimigos, com aparência diferente e as mesmas regras (um golpe mata).

### Regras obrigatórias de toda sala
1. **Spawn seguro:** nenhum inimigo tem linha de visão para o ponto de respawn.
2. **Três rotas, no mínimo,** do spawn até cada grupo de inimigos.
3. **Dois flancos, no mínimo,** cada um nas costas de um inimigo diferente.
4. **Cobertura por perto** a até ~3 tiles de qualquer ponto (obrigatória nas salas de tiro do pós-MVP; recomendada no MVP).
5. **Tudo sólido bloqueia movimento e visão.**
6. **Alcance máximo do inimigo: ~6,5 tiles.**
7. **Portas próximas:** entrada e saída a até ~18 tiles em linha reta.

### Montagem no Unity (MVP)
- **Tilemap `Walls`** (camada `Walls`): paredes e móveis; bloqueiam movimento e visão.
- **Por sala, um objeto vazio** com: ponto de spawn, bloqueio da saída e os inimigos da sala como filhos (cada um com seus pontos de patrulha).
- **Carro:** objeto com gatilho na entrada da fase.
- **Cena `Main`:** montada só pelo Ailton.

---

## 12. Estilo visual e arte

### MVP
- **Top-down puro.** O desenho inteiro gira na direção do mouse: um desenho por estado, não por direção.
- **Resolução:** interna **384×216** (tiles de 16 px), ampliada por inteiro até a tela (×5 em 1080p; barras pretas quando não couber).
- **Paleta:** preto, branco e dois cinzas. **Vermelho `#C8102E` só em sangue e na interrogação.**
- **Arte de programador:** formas simples já desenhadas em cinza. **Jogador em tom claro, inimigos em tom escuro.** Arma no chão com contorno branco piscando.
- **Atmosfera:** vinheta e granulação pelo pós-processamento do URP (desejável).

### Pós-MVP
- Pacotes de arte gratuitos CC0 (ex.: Kenney) com retoques no Piskel/LibreSprite.
- Shader que converte a arte colorida para cinza preservando só o vermelho reservado.
- Animação: 2 quadros de passos, 1 de ataque. Carro com marcas de bala.

---

## 13. UI e UX

### MVP obrigatório
| Item | Dono |
|---|---|
| Menu inicial: Jogar e Sair (com uma linha de controles) | Arthur |
| Tela de morte: congela, escurece, "VOCÊ MORREU — [R] PARA REINICIAR" após ~0,5 s | Arthur |
| Tela de fim de fase: fundo preto, "[Enter] continuar" | Arthur |
| Aviso "SALA LIMPA — VOLTE AO CARRO" + seta na borda da tela para a saída e depois para o carro | Arthur |
| Mira própria (cursor do sistema escondido) | Ailton |
| Arma no chão: contorno piscando e "[E] pegar" | Ailton |
| Sangue vermelho na morte (efeito único usado por jogador e inimigo) | Ailton |
| Interrogação sobre o inimigo, sempre sobre fundo escuro | Lipe |
| Corpo do inimigo no chão | Lipe |

### MVP desejável
- HUD da arma no canto inferior esquerdo (Arthur).
- Pausa (ESC): Continuar, Reiniciar sala, Sair (Arthur).
- "Reduzir efeitos" na pausa, desligando tremor, vinheta e granulação (Arthur).
- Pausa de impacto (~60 ms) e tremor curto ao matar.
- Telas de texto da narrativa com efeito máquina de escrever; Enter pula.

### Pós-MVP
- Tela de controles, estatísticas (tempo, mortes, abatidos), créditos completos.
- **Vetado de vez:** indicador de fase, sala ou inimigos restantes.

### Controles (MVP)
| Ação | Tecla |
|---|---|
| Mover | WASD |
| Mirar | Mouse |
| Atacar | Botão esquerdo |
| Pegar arma | E |
| Reiniciar (tela de morte) | R |
| Pausa (desejável) | ESC |

---

## 14. Narrativa

### Premissa
- **Capitão de polícia** que investigava a máfia. Uma **emboscada no trânsito** matou a família; ele sobreviveu. Devolve o distintivo e vai atrás dos responsáveis.
- **Lei Seca, anos 30.** Cidade sem nome. Nome do protagonista: em aberto.

### Estrutura (MVP)
| Fase | Lugar | Alvo |
|---|---|---|
| 1 | Bar clandestino (porão do Harlow's) | O atirador da emboscada |
| 2 | Mansão | O chefão |

### Forma em tela
- Telas de texto em fundo preto, primeira pessoa, noir seco. Sem diálogos, retratos, cenas ou escolhas.
- No MVP obrigatório, o texto pode ser estático; o efeito máquina de escrever é desejável.

### Textos (rascunho — equipe revisa)
**Abertura:** "Eu era capitão. Tinha distintivo, sala com janela e uma família no banco de trás. Numa terça de chuva, um sedã parou ao nosso lado no sinal da Rua 9 e abriu fogo. Eles vieram por mim. Levaram todos, menos eu."

**Fase 1 → 2:** "Devolvi o distintivo. O atirador bebia todas as noites no porão do Harlow's. Antes do fim, ele disse quem pagou pela emboscada e onde ele mora: uma mansão no alto da colina. Sábado, oito da noite. Não fui convidado."

**Final:** "Acabou. A chuva lavou a escadaria da mansão como lavou a Rua 9 naquela terça. Voltei para o carro. O banco de trás continua vazio."

---

## 15. Áudio

### MVP (desejável)
- **Música:** duas faixas em loop, jazz noir (contrabaixo e sopro). Menu mais calmo, gameplay mais tenso. Troca com fade de ~1 s.
  - **Para na volta ao carro** (ao aparecer o aviso). Não reinicia na morte. Abaixa na pausa.
- **Efeitos:** punho, bastão e morte (o som de morte toca junto com o golpe que mata). Passos e alerta sem som.
- **Formato:** `.ogg` para música, `.wav` ou `.ogg` para efeitos, em `Assets/Audio/Music/` e `Assets/Audio/SFX/`.
- **Origem:** OpenGameArt, Freesound, Kenney. Só **CC0 ou CC-BY**, com autor e licença de cada arquivo em `CREDITS.md`.

### Pós-MVP
- Sons de pistola, Thompson, pegar e largar arma. Som posicional.

---

## 16. Organização da equipe

### Divisão
| Pessoa | Código | UX/UI e conteúdo |
|---|---|---|
| **Ailton** | **Setup do projeto** (projeto Unity, `.gitignore`, tags e camadas, `Contracts.cs`, `AGENTS.md`, modelo de PR) e `ProjectSettings`; prefab do jogador: movimento, mira, punho, bastão, pegar arma, morte | **Dono da cena `Main`** e das salas (7 regras); mira; arma no chão; sangue; textos; playtest |
| **Lipe** | Prefab do inimigo: patrulha, linha de visão, detecção, alerta, esquecimento, ataque, morte | Interrogação; corpo no chão; música e sons; `CREDITS.md` |
| **Arthur** | Câmera; GameManager (sala limpa, saída bloqueada, checkpoint, respawn, carro); áudio; build Windows | Todas as telas; avisos de fluxo; HUD e pausa |

| **Fernando** | — | **Apresentação em 07/12:** pitch, história e demonstração da jogabilidade |

**Regra:** o dono entende e sabe explicar na apresentação tudo que faz merge na pasta dele. Perguntas técnicas do professor sobre o código ficam com o dono de cada parte.

### Conexão entre os três
1. **`Contracts.cs`** (`Assets/Scripts/Contracts/`): primeiro commit depois do projeto. Só muda com os três de acordo.
   - **`IHittable.ReceiveHit(Vector2 from)`**: implementado pelo jogador (Ailton) e pelo inimigo (Lipe). Quem ataca só chama, sem saber o que acertou.
   - **`GameEvents`**: `PlayerDied` (Ailton avisa, Arthur escuta), `EnemyDied` (Lipe avisa, Arthur escuta), `RoomCleared` e `PlayerReachedCar` (Arthur). Inscrever em `OnEnable`, desinscrever em `OnDisable`.
   - **`Names`**: tags `Player` e `Enemy`; camadas `Walls` e `Weapon`.
2. **`AGENTS.md`**: regras do Claude Code e do Codex. Agentes só mexem na pasta de quem pediu, não editam `.unity`, `.prefab` e `.meta` como texto, e terminam cada tarefa listando os "Passos no editor".
3. **Donos de pasta, prefab e cena:**
   - `Scripts/Player` (Ailton), `Scripts/Enemy` (Lipe), `Scripts/Game` e `Scripts/UI` (Arthur), `Scripts/Contracts` (os três).
   - Cena `Main`: só o Ailton salva. Cenas de teste: `Test_Ailton`, `Test_Lipe`, `Test_Arthur`.
   - `ProjectSettings` (tags, camadas, input, versão): só o Ailton.
   - `Assets/Physics/` (materiais de física, como o `PlayerNoFriction`): Ailton. Proposta a confirmar na reunião e a incluir no `AGENTS.md`.
4. **PRs documentados** com `.github/pull_request_template.md`: o que mudou, contratos usados, como testar, passos no editor depois do merge e **explicação escrita pelo dono**.
5. **Reunião semanal: segunda, 19h30–20h30, em chamada.** Sai das 3h de cada um. **A primeira é na quarta, 11/11.**
   - ~10 min: o que cada um terminou e onde travou
   - ~25 min: merge dos PRs **já revisados antes**; conflito o dono resolve na hora
   - ~15 min: Ailton monta a `Main` com os "Passos no editor" e os três jogam
   - ~10 min: tarefas da semana e mudanças no `Contracts.cs`

**Entre reuniões:** ao fim de cada sessão, mensagem no grupo: "Fiz X. Abri o PR Y. Travei em Z."

**Ciclo de cada tarefa (como o Ailton trabalha com os agentes):**
1. A tarefa é aprofundada no chat do Project: decisões técnicas, critérios de pronto e o prompt.
2. O prompt manda ler o `AGENTS.md` e a seção do `docs/contexto-jogo.md`; tem REQUISITOS, ESCOPO fechado ("somente o que está em REQUISITOS, nada além; mesmo que pareça útil"), NÃO FAÇA, e o formato de retorno: arquivos, código, explicação para iniciante, **decisões que tomou sozinho**, passos no editor, como testar, dúvidas ou riscos, descrição do PR.
3. O agente escreve, faz commit só do script na branch e push. Não abre PR nem faz merge.
4. O dono abre o Unity, espera compilar e faz commit do `.meta` **antes** de qualquer cena usar o script.
5. O dono faz os passos no editor na cena de teste, confere cada objeto contra a lista e testa em Play Mode. No Unity a prova é o teste jogando, não testes automáticos.
6. O retorno do agente vem para o chat; a revisão começa pelas decisões que ele tomou sozinho.
7. O dono salva a cena (Ctrl+S), faz commit e abre o PR pelo site. A explicação do PR é escrita ou revisada pelo dono e conferida no chat.
8. Cuidado de Git: criar a branch a partir da `main` atualizada (`git pull` antes). A próxima tarefa pode sair da branch anterior se ela ainda não tiver merge.

**Peças falsas para trabalhar em paralelo:** Ailton testa com um quadrado que implementa `IHittable` e some; Lipe testa com um quadrado de tag `Player` arrastado na cena; Arthur testa com teclas de depuração (K chama `RaisePlayerDied`, L chama `RaiseEnemyDied`).

**Fluxo de Git:** branch por tarefa (`ailton/movimento`) → PR com o modelo → revisão cruzada (Ailton revisa Lipe, Lipe revisa Arthur, Arthur revisa Ailton) → **o dono faz o merge e resolve os próprios conflitos** → Ailton monta a `Main`. Merge toda semana, nunca tudo no final. Nada entra depois de 04/12.

---

## 17. Cronograma

| Semana | Período | Ailton | Lipe | Arthur |
|---|---|---|---|---|
| **U1** | 09–15/11 | ~~Setup do projeto~~ (feito 09/11); ~~movimento com colisão~~ (feito, PR aberto); mira no mouse (próxima, branch `ailton/mira`) | Inimigo patrulhando; linha de visão contra parede | Câmera e menu (começa após o setup) |
| **U2** | 16–22/11 | Mira própria; punho, bastão, pegar arma, sangue, morte; sala 1.1 na `Main` | Interrogação, alerta, esquecimento, ataque, morte e corpo | Saída bloqueada, respawn, carro, tela de morte, aviso e seta → **Marco A em 22/11** |
| **U3** | 23–29/11 | Playtest 1; salas 2.1 e 2.2; textos | Música e sons; `CREDITS.md`; ajustes pós-playtest | HUD, pausa, vinheta, telas de texto, tela de fim → **congelamento em 29/11** |
| **U4** | 30/11–06/12 | Ensaio da apresentação | Correção de erros | Build Windows testado em outro computador → **versão final 04/12**; 05–06 ensaio → **apresentação 07/12** |

**Fernando:** recebe o build da semana a partir do Marco A (22/11) para treinar a demonstração; monta o pitch e a história com base nas seções 4, 5 e 14; ensaia com os três em 05–06/12.

**Momentos de integração:** reunião de 16/11 (soco mata inimigo e inimigo mata jogador na `Main`) e de 23/11 (morte, sala limpa e carro ligados às telas).

### Gatilhos de corte
| Se até… | não estiver pronto… | então… |
|---|---|---|
| 11/11 | projeto aberto nos 3 computadores | reunião no mesmo dia |
| 15/11 | jogador batendo num inimigo que patrulha | fase 2 sai; entrega só a fase 1 |
| 22/11 | Marco A | fase 2 sai; U3 termina o Marco A |
| 29/11 | qualquer coisa | congelamento: só correção de erro |
| 04/12 | — | versão final gravada |

---

## 18. Qualidade

### Playtest 1 (após o Marco A)
Três pessoas de fora; ninguém explica nada; uma hipótese escrita antes.
1. O jogador entende a interrogação sem explicação? (≥2 de 3 saem de vista quando ela enche)
2. Aperta R depois da primeira morte sem ninguém falar? (todos)
3. Pega o bastão do chão sozinho? (≥2 de 3)

Se a primeira falhar, corrigir a interrogação (tamanho, contraste, velocidade) antes da fase 2.

### Critérios de sucesso
- 2 de 3 limpam a 1.1 em até 5 min sem ajuda.
- 60 FPS no notebook mais fraco da equipe.
- Build roda num computador que nunca abriu o projeto.

### Riscos
| Risco | Plano B |
|---|---|
| Integrante some | Revisão cruzada garante que cada frente tem uma segunda pessoa que conhece o código |
| Conflito de Git em cena ou prefab | Donos fixos; só o Ailton salva a `Main` |
| Código cresce além do que a equipe entende | Explicação obrigatória no PR; congelar funções novas e revisar |
| Inimigo em linha reta trava em parede | Aceito no MVP; level design evita becos entre inimigo e jogador |
| Build não roda fora | Testar na U4; plano B: apresentar no notebook da equipe |

### Classificação e público
- **ClassInd: 16 anos.** Público que busca desafio e fantasia de vingança.
- **Acessibilidade:** informação por forma, não só por cor. Sem modo fácil (pilar 1).

---

## 19. Pendências

**Pauta da 1ª reunião (qua 11/11):**
- Arthur e Lipe com o projeto aberto (Unity 6000.5.9f1, Console sem erro).
- Merge do PR do movimento, depois da revisão do Arthur.
- Pasta `Assets/Physics` com o Ailton como dono (entra no `AGENTS.md`).
- Rigidbody do inimigo: proposta Kinematic, para jogador e inimigo não se empurrarem.
- Explicação do PR: o dono escreve, ou o agente rascunha e o dono confere (decidir para os três).

**Próxima tarefa do Ailton:** mira no mouse (o personagem gira para o cursor), branch `ailton/mira` a partir de `ailton/movimento`. Montar o prompt no chat.

**Notion:** a página do BACKSEAT ainda está com o estado de 09/11 (setup atribuído ao Arthur). Atualização proposta e ainda não confirmada: próximo passo = Arthur e Lipe abrem o projeto até 11/11, merge do movimento e mira; equipe com o setup no Ailton e o Fernando na apresentação.


- Nota do GDD (aguardando).
- Avisar o professor da troca de engine.
- Confirmar se código assistido por IA é aceito.
- Confirmar com Arthur e Lipe o horário de segunda, 19h30.
- Revisar o texto "Fase 1 → 2" e os outros rascunhos.
- Velocidade do jogador (provisório: ~6 tiles/s); nome do protagonista.
