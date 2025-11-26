using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

// ====== MODELOS DO JSON ======
public class GameEntry
{
    public string Name { get; set; } = "";
    public List<string> Aliases { get; set; } = new();
    public List<ExecutableEntry> Executables { get; set; } = new();

    public override string ToString() => Name;
}

public class ExecutableEntry
{
    public bool Is_Launcher { get; set; }
    public string Name { get; set; } = "";
    public string Os { get; set; } = "";
}

// ====== FORM PRINCIPAL (ESCOLHER JOGO) ======
public class MainForm : Form
{
    private TextBox txtSearch;
    private ListBox lstGames;
    private ListBox lstExecs;
    private Button btnStart;
    private Label lblStatus;

    private List<GameEntry> allGames = new();
    private readonly string exePath;
    private readonly string exeFolder;

    public MainForm(string exePath)
    {
        this.exePath = exePath;
        this.exeFolder = Path.GetDirectoryName(exePath)!;

        Text = "Fake Game Launcher";
        Width = 700;
        Height = 400;
        StartPosition = FormStartPosition.CenterScreen;

        // Controles
        txtSearch = new TextBox { Left = 10, Top = 10, Width = 400, PlaceholderText = "Procurar jogo..." };
        lstGames = new ListBox { Left = 10, Top = 40, Width = 300, Height = 260 };
        lstExecs = new ListBox { Left = 320, Top = 40, Width = 350, Height = 260 };
        btnStart = new Button { Left = 10, Top = 310, Width = 150, Height = 30, Text = "Iniciar fake" };
        lblStatus = new Label { Left = 170, Top = 315, Width = 500, Height = 20, ForeColor = Color.DarkBlue };

        Controls.Add(txtSearch);
        Controls.Add(lstGames);
        Controls.Add(lstExecs);
        Controls.Add(btnStart);
        Controls.Add(lblStatus);

        txtSearch.TextChanged += (s, e) => ApplyFilter();
        lstGames.SelectedIndexChanged += (s, e) => LoadExecutables();
        btnStart.Click += (s, e) => StartFake();

        Load += (s, e) => LoadJson();
    }

    private void LoadJson()
    {
        try
        {
            string jsonPath = Path.Combine(exeFolder, "detectable.json");
            if (!File.Exists(jsonPath))
            {
                MessageBox.Show($"detectable.json não encontrado em:{jsonPath}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            allGames = JsonSerializer.Deserialize<List<GameEntry>>(json, options) ?? new();

            // só jogos que tenham executáveis win32
            allGames = allGames
                .Where(g => g.Executables != null && g.Executables.Any(ex => ex.Os == "win32"))
                .OrderBy(g => g.Name)
                .ToList();

            lstGames.Items.Clear();
            foreach (var g in allGames)
                lstGames.Items.Add(g);

            lblStatus.Text = $"Carregado {allGames.Count} jogos do detectable.json";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao ler detectable.json: " + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        if (allGames.Count == 0) return;

        string q = txtSearch.Text.Trim().ToLowerInvariant();

        lstGames.Items.Clear();
        var filtered = allGames.Where(g =>
            g.Name.ToLowerInvariant().Contains(q) ||
            (g.Aliases != null && g.Aliases.Any(a => a.ToLowerInvariant().Contains(q)))
        );

        foreach (var g in filtered)
            lstGames.Items.Add(g);
    }

    private void LoadExecutables()
    {
        lstExecs.Items.Clear();
        if (lstGames.SelectedItem is not GameEntry game) return;

        var winExecs = game.Executables
            .Where(ex => ex.Os == "win32")
            .ToList();

        foreach (var ex in winExecs)
        {
            // mostra o "name" original do JSON
            lstExecs.Items.Add(ex.Name);
        }
    }

    private void StartFake()
    {
        if (lstGames.SelectedItem is not GameEntry game)
        {
            MessageBox.Show("Selecione um jogo primeiro.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (lstExecs.SelectedItem is not string execNameRaw)
        {
            MessageBox.Show("Selecione um executável (linha da direita).", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // O JSON às vezes tem caminhos tipo "_beta_/wow-64.exe"
        // Vamos pegar só o nome do arquivo final
        string filename = execNameRaw
            .Replace('/', Path.DirectorySeparatorChar); // por segurança
        filename = Path.GetFileName(filename);

        if (!filename.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            filename += ".exe";

        string destPath = Path.Combine(exeFolder, filename);

        try
        {
            if (!File.Exists(destPath))
            {
                File.Copy(exePath, destPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = destPath,
                Arguments = "--child",
                UseShellExecute = false
            });

            lblStatus.Text = $"Iniciado fake: {filename} (jogo: {game.Name})";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao iniciar fake: " + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

// ====== FORM DO PROCESSO FAKE (FILHO) ======
public class FakeForm : Form
{
    public FakeForm(string exePath)
    {
        string exeName = Path.GetFileName(exePath);

        Text = exeName;
        Width = 400;
        Height = 200;
        StartPosition = FormStartPosition.CenterScreen;

        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(FontFamily.GenericSansSerif, 10),
            Text = $"Simulando: {exeName}\r\r" + "Deixe esta janela aberta para o Discord achar que o jogo está rodando."
        };

        Controls.Add(lbl);
    }
}

// ====== PONTO DE ENTRADA ======
static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        string fullPath = Process.GetCurrentProcess().MainModule!.FileName;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Se vier com --child, é o processo fake (nome já é o do .exe copiado)
        if (args.Length > 0 && args[0] == "--child")
        {
            Application.Run(new FakeForm(fullPath));
        }
        else
        {
            // Launcher com GUI
            Application.Run(new MainForm(fullPath));
        }
    }
}