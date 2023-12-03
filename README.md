# Hackaton8

## Description

The system is intended to be a chat bot which main purpose is to provide support for people with mental health issues and to prevent suicide.

The system is organized as client application developed with blazor wasm, admin application also developed with blazor wasm and api application developed with dotnet 8 minimal api.

SignalR is used to provide realtime notifications about the system.

The system is supervised by trained professionals which at any moment can stop response generation from AI bot and manually take over.

The system has built in editor for system prompt settings as well as document used to augment response from AI.
Those documents are automatically synced with Azure AI Search after successfull saving in persistance layer. We hope that with providing this functionality trained professionals can augment behavior of AI bot.

SignalR provides realtime updates, meaning that every event is automatically reflected on Admin screens. Starting new chat from a user, user messages and AI responses as well as potential risk detection by AI bot are in realtime shown to trained professional which can then react upon it.

User is tracked by cookie and every time user start conversation his history from previous conversations is loaded both on user and admin screen. History of conversation is also sent as prompt to AI bot so it has more context about the user.

Admin application and API portion of admin application is protected by Azure B2C authentication and authorization.

System is also monitored with AppInsight.

System is configured to be deployed to Azure after push to main branch.


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



## Demo

https://github.com/dejandjenic/Hackaton8/blob/main/demo.mp4
