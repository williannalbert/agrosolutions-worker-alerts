
# 🚨 AgroSolutions - Alerts Worker (Processamento e Alertas)

O **AgroSolutions Alerts Worker** é um serviço de processamento em background (Worker Service) essencial para a plataforma AgroSolutions. Ele atua como o motor de inteligência em tempo real, responsável por consumir os dados brutos de telemetria, avaliar condições climáticas extremas e disparar notificações para os produtores rurais.

## 🚀 Tecnologias Utilizadas

* **Framework:** .NET (Worker Service)
* **Mensageria:** AWS SQS (Simple Queue Service)
* **ORM:** Entity Framework Core (Code-First Migrations)
* **Padrões de Design:** Clean Architecture, Specification Pattern
* **Integrações:** HTTP Client (Comunicação com a History API)
* **Autenticação (S2S):** Integração via Keycloak
* **Containerização:** Docker e Docker Compose
* **Orquestração:** Kubernetes (Amazon EKS)
* **CI/CD:** GitHub Actions

## 🏗️ Arquitetura (Clean Architecture)

O projeto está dividido em quatro camadas principais para garantir um código limpo, testável e escalável:

* `AgroSolutions.Alerts.Worker`: O ponto de entrada (Hosted Service). Fica escutando a fila do AWS SQS de forma contínua.
* `AgroSolutions.Alerts.Application`: Contém a orquestração do serviço (`TelemetryProcessingService`), interfaces de integração (`IHistoryIntegrationService`) e os DTOs.
* `AgroSolutions.Alerts.Domain`: O núcleo da inteligência. Contém as entidades (`Alert`), os Enums de severidade e, mais importante, o **Specification Pattern** que define as regras de negócio para alertas.
* `AgroSolutions.Alerts.Infrastructure`: Implementação técnica. Consumidores do SQS, envio de e-mails (`AwsSqsEmailService`), manipulação de banco de dados (`AgroContext`) e Auth handlers.

## ✨ Funcionalidades e Fluxo de Dados

1. **Ingestão (Consumer):** O Worker lê ininterruptamente mensagens contendo telemetrias (Solo, Clima, Silo) oriundas de uma fila AWS SQS.
2. **Análise de Risco (Specifications):** Os dados passam por um funil de regras de domínio:
   * **DroughtRisk (Risco de Seca):** Avalia combinação de baixa umidade e alta temperatura.
   * **HeavyRain (Chuva Forte):** Avalia altos índices pluviométricos.
   * **PestRisk (Risco de Pragas):** Avalia condições propícias à proliferação de pragas.
3. **Persistência de Alertas:** Caso uma regra seja violada, um registro de alerta é salvo no banco de dados.
4. **Notificação:** Disparo de eventos de notificação via e-mail utilizando a infraestrutura da AWS.
5. **Integração:** Os dados brutos são encaminhados para a `History API` para compor os gráficos e o histórico de longo prazo.

## ⚙️ Como Executar Localmente

### Pré-requisitos
Para rodar este serviço localmente, você precisará de:
* Um banco de dados relacional (conforme configurado no `AgroContext`).
* Credenciais AWS configuradas (Access Key / Secret Key) para acesso ao SQS.

### Ambiente com Docker Compose
O repositório conta com um `docker-compose.yml` para facilitar o levantamento das dependências locais.

    docker-compose up -d
Execução via .NET CLI

    cd AgroSolutions.Alerts.Worker 
    dotnet run
## 🚀 CI/CD e Deploy (Kubernetes na AWS)

O deploy é gerido de forma contínua através do **GitHub Actions**.

**Regra de Gatilho:** A Action (`deploy.yml`) é acionada somente quando há um Push ou um **Pull Request aprovado** para a branch principal.

**O que a esteira faz:**

1.  Checkout e setup do ambiente.
    
2.  Build e Push da imagem Docker para o Amazon ECR.
    
3.  Aplicação do manifesto `k8s/deployment.yaml` no cluster **Amazon EKS**.
    

Como se trata de um Worker Service (background process), este contêiner roda ininterruptamente dentro dos Pods do Kubernetes, sem necessidade de expor portas de entrada (Ingress/LoadBalancer).
