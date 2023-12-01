# Hackaton8

## Description



## Deployment
UI: https://polite-sand-09fd4c010.4.azurestaticapps.net/

Admin UI: https://purple-bay-0bec04d10.4.azurestaticapps.net/

API: https://hackatonapi.azurewebsites.net/


## Architecture

As shown on diagram bellow system heavily integrated with Azure and services provided by Azure cloud.

1. Azure B2C - provides authentication and authorization for API authorized calls and it's login flow starts from ADMIN.UI
2. AppInsight - collects logs, metrics and telemetry from both ADMIN.UI and API and provides valuble insight in system behavior
3. CosmosDB - used within persistance layer to store information about the system, chat history, user information, settings ...
4. Azure Vault - store sensitive information in Azure Vault secrets, endpoints, keys ...
5. AI Search - used to store, index and search embedings
6. OpenAI - main bot model
7. SignalR - used to provided realtime notifications
8. AppServices - ysed to host deployment of API
9. Static Pages - used to host EDGE.UI and ADMIN.UI



![Archtecture](./architecture.png)


## CI/CD

Once commit is made to github repository , github actions is configured to build solution and deploy API to AppServices and published Blazor WASM application to Static Pages for EDGE.ui and ADMIN.UI


![CI/CD](./deployment.png)


