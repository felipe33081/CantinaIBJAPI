# Cantina IBJ — Aplicativo Desktop Offline

App Windows autônomo para gestão das vendas da cantina nos retiros/acampamentos.
Roda **100% offline**, com **banco de dados embutido** (SQLite), sem precisar
instalar .NET, sem servidor de banco e sem internet.

## Como é por dentro

Um único `CantinaIBJ.exe` que:
- sobe a **API .NET** (a mesma de sempre) em `localhost` dentro do próprio processo;
- serve o **frontend React** já compilado (mesma origem);
- guarda os dados em um arquivo **SQLite**;
- exibe tudo em uma **janela própria** (WebView2) — parece um programa nativo.

## Gerar o aplicativo (na sua máquina de desenvolvimento)

Pré-requisitos: .NET 8 SDK e Node.js instalados.

```powershell
powershell -ExecutionPolicy Bypass -File build-desktop.ps1
```

Isso compila o React, empacota tudo e gera a pasta **`CantinaIBJ-Publish`**
(ao lado dos repositórios) contendo o `CantinaIBJ.exe`.

- Se você mudou só o backend/desktop e não o React, use `-SkipFrontend` para ir mais rápido.

## Gerar o instalador (recomendado para distribuir)

Pré-requisito adicional: Inno Setup 6 (`winget install JRSoftware.InnoSetup`).

```powershell
powershell -ExecutionPolicy Bypass -File build-installer.ps1
```

Gera **`installer\Output\CantinaIBJ-Setup-1.0.0.exe`** (~72 MB). Esse é o arquivo
único que você entrega. Ele instala o app em *Arquivos de Programas*, cria o atalho
no Menu Iniciar (e na Área de Trabalho, se marcado) e registra o desinstalador.

## Instalar no computador da cantina

**Opção A — Instalador (recomendado):** copie o `CantinaIBJ-Setup-1.0.0.exe` para o
computador e execute. Avance pelo assistente; ao final, marque "Abrir agora".
Depois é só usar o atalho **Cantina IBJ**.

**Opção B — Pasta portátil (sem instalar):**
1. Copie a pasta `CantinaIBJ-Publish` inteira para o computador (pen drive serve).
2. Dê **duplo-clique em `CantinaIBJ.exe`**.
3. (Opcional) Botão direito no `.exe` → *Enviar para* → *Área de trabalho (criar atalho)*.

> O Windows 11 já vem com o **Microsoft Edge WebView2 Runtime**. Em um Windows mais
> antigo sem ele, instale o "Evergreen WebView2 Runtime" (download único da Microsoft),
> uma vez só, com internet — depois o app roda offline normalmente.

## Acesso (PIN)

- PIN inicial de fábrica: **`1234`**.
- Troque no primeiro uso pela tela do app (endpoint `POST /v1/auth/change-pin`).
- O PIN fica guardado **com hash** no banco local — nunca em texto puro, nunca na nuvem.

## Onde ficam os dados

- Banco: `%LOCALAPPDATA%\CantinaIBJ\cantinaibj.db`
- Cache da janela: `%LOCALAPPDATA%\CantinaIBJ\WebView2`

Esse caminho é **preservado** ao atualizar o `.exe` — trocar o programa **não apaga**
as vendas. Para começar do zero (ex.: novo retiro), basta apagar o `cantinaibj.db`;
ele é recriado, já com o PIN padrão, na próxima abertura.

## Backup

Copie o arquivo `cantinaibj.db` para um pen drive. É o único arquivo com dados.

## Observações

- WhatsApp e e-mail são **opcionais** e à prova de offline: se não houver internet
  ou configuração, o app apenas ignora o envio — **nunca** trava uma venda.
- A impressão em impressora térmica (ESC/POS) continua funcionando offline.
