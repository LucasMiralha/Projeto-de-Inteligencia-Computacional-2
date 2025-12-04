# Replicação de Experimento: Reinforcement Learning para Desvio de Obstáculos com Unity ML-Agents

Este repositório contém a implementação e replicação do experimento científico proposto na disciplina de **Inteligência Computacional 1** do curso de Engenharia da Computação do **CESUPA**.

> **Professora:** Polyana Santos Fonseca Nascimento

---

## Artigo Base
O trabalho consiste na replicação dos métodos e resultados apresentados no seguinte artigo (publicado nos últimos 7 anos):

* **Título:** *Reinforcement learning for obstacle avoidance application in unity ml-agents*
* **Autores:** Reza Mahmoudi e Armantas Ostreika
* **Ano:** 2023
* **Publicação:** CEUR Workshop Proceedings (Vol-3575)
* **Resumo:** O estudo investiga o uso de Aprendizado por Reforço (RL) usando o toolkit Unity ML-Agents para treinar agentes (karts) em uma pista de corrida simulada. O foco é comparar técnicas de imitação para desvio de obstáculos.

## Objetivos do Experimento
O objetivo principal é treinar um agente autônomo (Kart) para navegar em uma pista e desviar de obstáculos aleatórios, maximizando a recompensa cumulativa e minimizando a perda (loss). O experimento compara duas abordagens de aprendizado por imitação utilizando o algoritmo **PPO (Proximal Policy Optimization)**:

1.  **Behavior Cloning (Clonagem de Comportamento):** O agente aprende supervisionado por demonstrações de um especialista.
2.  **GAIL (Generative Adversarial Imitation Learning):** Utiliza redes adversárias (GANs) para aprender a política a partir das demonstrações.

## Tecnologias e Ferramentas
* **Engine:** Unity (Versão 6000.2.6f2)
* **Linguagem:** C#
* **Algoritmo:** PPO (Proximal Policy Optimization)

## Metodologia
* Rede Neural feita do zero em C#, com 7 entradas, 2 camadas de 10 neurônios e 2 saídas (Aceleração e Rotação).
* Modularidade das características da simulação e do cenário.
* Randomização do cenário para garantir aprendizado e não memorização.

## Como Executar o Projeto

### Pré-requisitos
1.  Instalar o [Unity Hub e Editor](https://unity.com/).

### Passo a Passo
1.  **Clonar o Repositório:**
    ```bash
    git clone [https://github.com/LucasMiralha/Projeto-de-Inteligencia-Computacional-2.git](https://github.com/LucasMiralha/Projeto-de-Inteligencia-Computacional-2.git)
    cd Projeto-de-Inteligencia-Computacional-2
    ```

2.  **Abrir o Projeto no Unity:**
    * Abra o Unity Hub, clique em "Open" e selecione a pasta do projeto clonado.

3.  **Iniciar a simulação:**
    * Na tela de editor do Unity, clique no objeto SimulationManager e defina as variáveis da simulação, como Mutation Rate e Strength, Time Scale, população, etc.
    * Aperte o play do editor Unity e deixe a simulação rodar.
    * Durante a simulação, aperte "T" para tomar controle de um cabrito para executar o Cloning Behaviour.
    * Controles: W - aceleração para frente; S - Freio; A/D - Rotação esquerda/direita. (Durante a sessão de Cloning Behaviour a escala de tempo sempre será 1)

4.  **Visualizar Resultados:**
    * Acesse os arquivos do projeto, na pasta "Assets".
    * O arquivo CSV com os valores dos resultados da simulação possuirá os valores de Mutation Rate e Strength no nome.
---
*Projeto desenvolvido para a disciplina de Inteligência Computacional 1 - CESUPA 2025.*
