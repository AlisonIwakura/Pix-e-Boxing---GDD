# Pix-e-Boxing---GDD
GGD for Game,  Pix e Boxing


# 🥊 PIX E BOXING – Game Design Document (GDD)

**Autor:** Alison Tiago Lima da Paixão  
**Cargo:** Desenvolvedor Full Stack & Analista Computacional  
**Versão:** 1.2 – Multiplayer & Economia Real  
**Engine:** Unity WebGL  
**Backend:** Node.js + Socket.io  
**Database:** MySQL  

---

# 📌 1. Visão Geral do Projeto

**Pix e Boxing** é um jogo multiplayer desenvolvido para **WebGL**, integrando mecânicas de combate estratégico com um sistema de economia em tempo real.

A proposta do projeto é demonstrar a viabilidade de uma aplicação **Full Stack Game Architecture**, onde:

- O cliente (Unity WebGL) é responsável pela interface e feedback visual.
- O servidor (Node.js) atua como **servidor autoritativo**, garantindo segurança e integridade.
- O banco de dados (MySQL) armazena informações persistentes como personagens, transações e partidas.

O jogo roda diretamente no navegador, eliminando a necessidade de instalação.

---

# 🎯 2. Objetivo do Projeto

Este projeto tem como objetivo demonstrar a integração entre:

- Desenvolvimento de jogos com **Unity**
- Comunicação multiplayer via **WebSockets**
- Infraestrutura backend **Node.js**
- Sistemas econômicos baseados em probabilidades (Gacha)
- Persistência de dados em **MySQL**

Além disso, o projeto serve como demonstração de habilidades em:

- Arquitetura de software
- Desenvolvimento Full Stack
- Sistemas multiplayer
- Economia virtual em jogos

---

# 🧠 3. Arquitetura do Sistema

O sistema utiliza o modelo **Authoritative Server**, onde todas as decisões críticas são processadas no servidor.

Isso previne:

- manipulação de resultados
- exploits econômicos
- client-side injection
- modificações maliciosas do cliente

---

# 🏗 Arquitetura Geral




            ┌──────────────────────┐
            │      CLIENT WEB      │
            │     Unity WebGL      │
            │                      │
            │  UI / Gameplay      │
            │  Prefabs / Lobby    │
            └──────────┬───────────┘
                       │
                 WebSocket
                 Socket.IO
                       │
            ┌──────────▼───────────┐
            │       BACKEND        │
            │       Node.js        │
            │                      │
            │  Matchmaking Engine  │
            │  Combat Validation   │
            │  Economy Control     │
            │  Lobby Management    │
            └──────────┬───────────┘
                       │
                       │ SQL Queries
                       │
            ┌──────────▼───────────┐
            │      DATABASE        │
            │        MySQL         │
            │                      │
            │ Users                │
            │ Characters           │
            │ Transactions         │
            │ Matches              │
            └──────────────────────┘


            
---

# 🖥 2.1 Frontend — Unity WebGL

O frontend foi desenvolvido utilizando **Unity Engine**, com exportação para **WebGL**, permitindo execução direta em navegadores modernos.

Responsabilidades do cliente:

- Renderização da interface de usuário (UI)
- Instanciamento de personagens através de **Prefabs**
- Feedback visual de combate
- Exibição do lobby multiplayer
- Comunicação em tempo real com o backend

Tecnologias utilizadas:

- Unity Engine
- C#
- WebGL Build
- Canvas UI
- Prefab System

---

# ⚙ 2.2 Backend — Node.js + Socket.IO

O backend foi desenvolvido utilizando **Node.js**, com comunicação em tempo real através de **WebSockets (Socket.IO)**.

O servidor atua como **autoridade central do jogo**, sendo responsável por:

- matchmaking entre jogadores
- validação de saldo
- cálculo de combate
- controle da economia
- sincronização multiplayer

Principais funções do backend:

Matchmaking PvP
Validação de apostas
Cálculo de dano
Controle de economia
Sincronização de estados
Gerenciamento de salas



Esse modelo impede que o cliente manipule resultados de combate ou transações.

---

# 🗄 2.3 Camada de Dados — MySQL

A persistência de dados é realizada através de **MySQL**, utilizando um esquema **normalizado** para garantir integridade e consistência.

Principais tabelas do sistema:

users
wallets
characters
character_attributes
transactions
pvp_matches
gacha_rolls



Dados armazenados incluem:

- saldo de jogadores
- atributos de personagens
- histórico de combates
- registros de transações

---

# 💰 3. ECOSSISTEMA ECONÔMICO (TOKENOMICS)

Um dos diferenciais do projeto é o **modelo econômico híbrido**, que combina:

- mecânica **Gacha (sorteio de personagens)**
- sistema **PvP Stake (apostas entre jogadores)**
- progressão **PvE com recompensas**

O objetivo é criar um sistema onde exista equilíbrio entre:

- geração de ativos
- consumo de recursos
- competitividade entre jogadores

---

# 🎲 3.1 Algoritmo de Raridade

Os personagens possuem **raridade**, que influencia diretamente seus atributos e desempenho em combate.

A raridade não é apenas estética — ela impacta matematicamente os cálculos realizados pelo servidor.

### Fórmula de Cálculo

AtributoFinal = AtributoBase × MultiplicadorRaridade


### Exemplo

Personagem:

Força Base = 50
Stamina Base = 40
Raridade = Místico
Multiplicador = 2.00x


Isso torna personagens raros **ativos estratégicos de alto valor no ecossistema do jogo**.

---

# 📊 Tabela de Raridades

| Raridade | Multiplicador |
|--------|--------|
Comum | 1.00x |
Raro | 1.25x |
Épico | 1.50x |
Lendário | 1.75x |
Místico | 2.00x |

---

# 📈 3.2 Fluxo de Receita e Retenção

A economia do jogo possui três principais mecanismos de monetização.

---

## Revelação de Personagens (Gacha)


Valor: R$ 3,00


Função:

- geração de novos personagens
- criação de ativos no ecossistema

---

## PvP Stake

Jogadores podem disputar partidas **apostando valores entre si**.

O sistema aplica uma taxa operacional.



Isso gera fluxo de caixa para manutenção da infraestrutura.

---

## Ticket de Aventura

Modo PvE pago com recompensas.




Objetivo:

- incentivar progressão
- incentivar upgrade de personagens
- criar consumo econômico no sistema

---

# 🎮 4. MODOS DE OPERAÇÃO

O jogo possui três modos principais de interação.

---

# 🌐 4.1 Lobby Social & Chat

O lobby funciona como um **ambiente social multiplayer**, onde jogadores podem interagir antes das partidas.

Recursos:

- instanciamento global de jogadores
- sincronização de transformadas
- visualização de personagens
- chat em tempo real

Tecnologia utilizada:

WebSockets (Socket.IO)


Cada jogador é representado por um **Prefab sincronizado no ambiente 3D**.

---

# ⚔ 4.2 Matchmaking PvP (1v1)

O modo PvP permite confrontos diretos entre jogadores.

O sistema utiliza **salas privadas criadas dinamicamente**.

Fluxo de matchmaking:


↓
Servidor busca adversário compatível
↓
Criação de sala PvP
↓
Execução da luta
↓
Servidor valida o resultado
↓
Distribuição de prêmio

Critérios de matchmaking incluem:

- estabilidade de conexão
- faixa de aposta
- disponibilidade de jogadores

---

# 🧭 4.3 Modo Aventura (PvE Progressivo)

O modo PvE utiliza um sistema baseado em **ondas de inimigos (waves)**.

Estrutura do modo:


10 estágios progressivos



Cada estágio aumenta:

- dificuldade
- vida dos inimigos
- dano recebido

O objetivo é chegar ao **boss final**.

Recompensa máxima:


R$ 10,00

Esse modo incentiva:

- upgrade de personagens
- consumo de tickets
- progressão do jogador

---

# 🔒 5. SEGURANÇA E INTEGRIDADE

O sistema foi projetado priorizando **segurança transacional** e **integridade de dados**.

Medidas aplicadas:

### Servidor Autoritativo

O cliente **não pode decidir resultados de combate**.

Todas as decisões são tomadas pelo servidor.

---

### Validação de Transações

Cada operação financeira passa por:


verificação de saldo
registro de log
confirmação no banco


---

### Registro de Logs

Logs armazenados incluem:

combates
transações
revelações de personagens
resultados PvP



---

# 🚀 6. INFRAESTRUTURA

Infraestrutura atual do projeto:


Linux VPS
Nginx
Node.js
MySQL
Unity WebGL




Fluxo de deploy:

Nginx
↓
Node.js Application
↓
MySQL Database


O build do Unity WebGL é servido como **conteúdo estático via Nginx**.

---

# 📊 7. ESCALABILIDADE

A arquitetura permite futuras melhorias como:

- cluster Node.js
- Redis para sessões
- matchmaking distribuído
- microserviços
- balanceamento de carga

---

# 🧾 8. CONCLUSÃO TÉCNICA

O projeto **Pix e Boxing** demonstra a viabilidade de aplicações **WebGL multiplayer complexas**, suportadas por uma infraestrutura **Full Stack moderna**.

Os pilares técnicos da aplicação incluem:

- arquitetura servidor autoritativo
- economia integrada ao gameplay
- comunicação multiplayer em tempo real
- persistência segura de dados

A segurança do saldo dos jogadores e a precisão do algoritmo de raridade foram tratados como prioridades centrais no desenvolvimento do sistema.

---

# ✍ Autor

**Alison Tiago Lima da Paixão**  
Desenvolvedor Full Stack & Analista Computacional

Especialidades:

- Desenvolvimento de Jogos
- Arquitetura Multiplayer
- WebGL Applications
- Backend para Games
- Infraestrutura Full Stack
