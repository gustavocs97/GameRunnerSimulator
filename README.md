# GameRunnerSimulator
CompletarQuests – Simulando Jogos no Discord

Pequeno app em C# (.NET 10) que simula jogos rodando no seu PC para o Discord, usando o arquivo oficial `detectable.json` como base.

Você escolhe um jogo na lista, o programa copia o próprio executável com o nome do jogo (por ex. `overwatch.exe`) e mantém esse processo rodando, para o Discord detectar como se fosse o jogo real.

---

## Download (binário pronto)

Na aba **Releases** deste repositório, baixe o arquivo:

- `executavelnomeavel-DOTNET10-runtime.zip`  
  *(ou o nome mais novo que estiver lá, ex: `GameRunnerSimulator-win-x64-dotnet10-runtime.zip`)*

Extraia todo o conteúdo do `.zip` para uma pasta qualquer, por exemplo:

```text
C:\Users\SEU_USUARIO\Downloads\GameRunnerSimulator\
```

Dentro da pasta extraída você terá, por exemplo:

- `executavelnomeavel.exe`
- `executavelnomeavel.dll`
- `executavelnomeavel.deps.json`
- `executavelnomeavel.runtimeconfig.json`
- `detectable.json`

> **Importante:** o arquivo `detectable.json` **deve ficar na mesma pasta do `.exe`**.  
> Se você mover o `.exe` para outra pasta, mova o `detectable.json` junto.

---

## Como usar (binário)

1. Extraia o `.zip` em uma pasta.
2. Deixe todos os arquivos juntos (`executavelnomeavel.exe` + `.dll` + `.json` + `detectable.json`).
3. Dê **duplo clique** em `executavelnomeavel.exe`.
4. Na janela que abrir:
   - Use a caixa de busca para procurar o jogo (ex: `Overwatch`);
   - Selecione o jogo na lista da esquerda;
   - Selecione o executável correspondente na lista da direita (ex: `overwatch.exe`);
   - Clique em **Iniciar fake**.
5. O programa vai:
   - Copiar o próprio `.exe` com o nome do executável escolhido (ex: `overwatch.exe`);
   - Iniciar um novo processo com esse nome;
   - Abrir uma janelinha “Simulando: overwatch.exe”.

Enquanto essa janelinha estiver aberta, o Discord deve enxergar esse processo como se fosse o jogo.

---

## Requisitos

Para rodar o binário (versão *runtime*):

- Windows 64 bits;
- .NET Desktop Runtime 10 (x64) instalado.

Se o runtime não estiver instalado, o Windows normalmente abre uma tela pedindo para instalar.

---

## Como compilar a partir do código-fonte

Se você quiser gerar o seu próprio `.exe` a partir dos arquivos `Program.cs` e `.csproj` deste repositório, pode seguir este passo a passo simples.

### 1. Criar pasta base do projeto

No PowerShell ou CMD:

```ps
mkdir GameRunnerSimulator
cd GameRunnerSimulator
dotnet new console
```

Isso vai criar um projeto console padrão dentro da pasta `GameRunnerSimulator`.

### 2. Substituir os arquivos do template

Na pasta do projeto recém-criada:

- Apague o `Program.cs` gerado automaticamente;
- Apague o `.csproj` gerado automaticamente;
- Copie para essa pasta:
  - O `Program.cs` deste repositório (o da GUI);
  - O `.csproj` deste repositório (por exemplo `executavelnomeavel.csproj` ou o nome que você estiver usando).

Ou seja, **substituir os arquivos**:

```text
Program.cs
executavelnomeavel.csproj   (ou GameRunnerSimulator.csproj, se você renomear)
```

pelos que estão aqui no GitHub.

### 3. Compilar / publicar

Ainda dentro da pasta do projeto:

```ps
dotnet publish -c Release
```

Os arquivos compilados/publish vão aparecer em algo como:

```text
bin\Release\net10.0\win-x64\publish\
```

Nessa pasta você terá:

- o `.exe` (executável),
- `.dll`,
- `.deps.json`,
- `.runtimeconfig.json`.

Copie também o `detectable.json` para essa mesma pasta de `publish` (ou para onde você quiser rodar o `.exe`, sempre lado a lado).

Se quiser, você pode zipar o conteúdo de `publish` para criar um novo `.zip` de release.

---

## Como funciona (resumo técnico)

- App em C# (.NET 10, Windows Forms).
- Em tempo de execução:
  - Descobre o próprio caminho (`Process.GetCurrentProcess().MainModule!.FileName`);
  - Lê `detectable.json` da **mesma pasta** do `.exe`;
  - Filtra entradas com executáveis `win32`;
  - Exibe:
    - Lista de jogos,
    - Lista de executáveis por jogo.
- Quando clica em **Iniciar fake**:
  - Pega o `name` do executável (por ex. `"overwatch.exe"` ou `"_beta_/wow-64.exe"`);
  - Normaliza para um nome de arquivo (pega só o nome final, tipo `wow-64.exe`);
  - Copia o próprio `.exe` com esse nome;
  - Sobe um processo novo passando `--child`;
  - O processo filho abre uma janelinha e fica rodando (mantendo o processo ativo com aquele nome).

---

