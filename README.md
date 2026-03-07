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

# 🖥 4. Stack Tecnológica

## Frontend

Tecnologias utilizadas:

- Unity Engine
- WebGL Build
- C#
- Canvas UI
- Prefabs dinâmicos

Responsabilidades do cliente:

- renderização da interface
- animações de combate
- feedback visual
- gerenciamento de estado local
- comunicação com backend via WebSockets

---

## Backend

Tecnologias:

- Node.js
- Express
- Socket.IO

Responsabilidades do servidor:

- matchmaking PvP
- validação de ações do jogador
- cálculo de dano
- controle de economia
- sincronização de estados multiplayer
- proteção contra manipulação do cliente

---

## Banco de Dados

Tecnologia utilizada:

- MySQL

Estrutura normalizada para garantir consistência dos dados.

Principais tabelas:


