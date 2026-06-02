# C# WinForms Auth System — План реализации

**Goal:** Портировать Python/tkinter приложение аутентификации на C# WinForms за ~1 час.

**Architecture:** Один `MainForm` + три `Panel` (Login, User, Admin). Всё в одном месте, без абстракций.

**Tech Stack:** .NET 10 WinForms, MySqlConnector (NuGet)

---

## Карта файлов

**Изменить:**
- `Demo/Demo.csproj` — добавить MySqlConnector, копирование pics
- `Demo/Program.cs` — запустить `MainForm`
- `Demo/SignIn.cs` → переименовать/заменить на `MainForm.cs`

**Создать:**
- `Demo/MainForm.cs` — главное окно + DB-подключение + переключение панелей
- `Demo/LoginPanel.cs` — форма входа + капча
- `Demo/CaptchaControl.cs` — виджет капчи (4 картинки)
- `Demo/UserPanel.cs` — заглушка пользователя
- `Demo/AdminPanel.cs` — управление пользователями

**Удалить:**
- `Demo/SignIn.cs`
- `Demo/SignIn.Designer.cs`

---

## Замечание: исправленный баг из reference.py

Схема `demo_users`: 0=id, 1=login, 2=password, **3=roles**, 4=FIO, 5=status.
В оригинале `login_into(row[2])` — передаёт пароль вместо роли. В C# используем `row[3]`.

---

## Task 1: NuGet и настройка проекта

**Files:** `Demo/Demo.csproj`

- [ ] **Step 1: Обновить Demo.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net10.0-windows</TargetFramework>
        <Nullable>enable</Nullable>
        <UseWindowsForms>true</UseWindowsForms>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="MySqlConnector" Version="2.*" />
    </ItemGroup>
    <ItemGroup>
        <None Update="pics\*.png">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
</Project>
```

- [ ] **Step 2: Восстановить пакеты**

```bash
dotnet restore
```
Expected: `Restore completed.`

- [ ] **Step 3: Удалить SignIn**

```bash
rm "C:\Users\tiwole\RiderProjects\Demo\Demo\SignIn.cs"
rm "C:\Users\tiwole\RiderProjects\Demo\Demo\SignIn.Designer.cs"
```

---

## Task 2: CaptchaControl

**Files:** `Demo/CaptchaControl.cs`

Логика: 4 PictureBox 150×150 в панели 300×300. Кнопка "Решить капчу" перемешивает. После 7 нажатий — сброс в правильный порядок. Капча решена, когда позиции = `{(0,0),(150,0),(0,150),(150,150)}`.

- [ ] **Step 1: Создать CaptchaControl.cs**

```csharp
namespace Demo;

public class CaptchaControl : Panel
{
    private readonly PictureBox[] _boxes = new PictureBox[4];
    private readonly List<Point> _positions = new() {
        new(0,0), new(150,0), new(0,150), new(150,150)
    };
    private static readonly List<Point> _solved = new() {
        new(0,0), new(150,0), new(0,150), new(150,150)
    };
    private readonly Random _rng = new();
    private int _counter;

    public bool IsSolved => _positions.SequenceEqual(_solved);

    public CaptchaControl(string imagesPath)
    {
        Size = new Size(300, 300);
        BorderStyle = BorderStyle.FixedSingle;

        for (int i = 0; i < 4; i++)
        {
            _boxes[i] = new PictureBox
            {
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(Path.Combine(imagesPath, $"{i + 1}.png")),
                Location = _positions[i]
            };
            Controls.Add(_boxes[i]);
        }
    }

    public void Shuffle()
    {
        if (_counter >= 7)
        {
            for (int i = 0; i < 4; i++) _positions[i] = _solved[i];
            _counter = 0;
        }
        else
        {
            for (int i = _positions.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_positions[i], _positions[j]) = (_positions[j], _positions[i]);
            }
            _counter++;
        }

        for (int i = 0; i < 4; i++) _boxes[i].Location = _positions[i];
    }
}
```

---

## Task 3: LoginPanel

**Files:** `Demo/LoginPanel.cs`

- [ ] **Step 1: Создать LoginPanel.cs**

```csharp
using MySqlConnector;

namespace Demo;

public class LoginPanel : Panel
{
    private readonly TextBox _loginBox = new() { Width = 200 };
    private readonly TextBox _passBox = new() { Width = 200, UseSystemPasswordChar = true };
    private readonly CaptchaControl _captcha;
    private int _failCount;

    public event EventHandler<string>? LoginSuccess;

    public LoginPanel(string imagesPath)
    {
        _captcha = new CaptchaControl(imagesPath);
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 10
        };
        for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (int i = 0; i < 10; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

        layout.Controls.Add(Lbl("Логин:"), 1, 1);         layout.SetColumnSpan(layout.Controls[^1], 2);
        layout.Controls.Add(_loginBox, 1, 2);              layout.SetColumnSpan(_loginBox, 2);
        layout.Controls.Add(Lbl("Пароль:"), 1, 3);        layout.SetColumnSpan(layout.Controls[^1], 2);
        layout.Controls.Add(_passBox, 1, 4);               layout.SetColumnSpan(_passBox, 2);
        layout.Controls.Add(_captcha, 1, 5);               layout.SetColumnSpan(_captcha, 2); layout.SetRowSpan(_captcha, 2);

        var shuffleBtn = new Button { Text = "Решить капчу", Dock = DockStyle.Fill };
        shuffleBtn.Click += (_, _) => _captcha.Shuffle();
        layout.Controls.Add(shuffleBtn, 1, 7);             layout.SetColumnSpan(shuffleBtn, 2);

        var loginBtn = new Button { Text = "Войти", Dock = DockStyle.Fill };
        loginBtn.Click += OnLogin;
        layout.Controls.Add(loginBtn, 1, 8);               layout.SetColumnSpan(loginBtn, 2);

        Controls.Add(layout);
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        var login = _loginBox.Text.Trim();
        var pass = _passBox.Text.Trim();

        if (login.Length == 0 || pass.Length == 0)
        {
            MessageBox.Show("Логин и пароль не могут быть пустыми", "Ошибка");
            return;
        }

        var db = ((MainForm)FindForm()!).Db;
        try
        {
            var byLogin = db.QueryOne("SELECT * FROM demo_users WHERE login=@l", ("@l", login));
            if (byLogin == null)
            {
                MessageBox.Show("Вы ввели неверный логин.\nПожалуйста проверьте еще раз введенные данные", "Ошибка");
                return;
            }

            var byBoth = db.QueryOne("SELECT * FROM demo_users WHERE login=@l AND password=@p", ("@l", login), ("@p", pass));
            if (byBoth == null || !_captcha.IsSolved)
            {
                MessageBox.Show("Вы ввели неверный пароль или не решили капчу.\nПожалуйста проверьте еще раз введенные данные", "Ошибка");
                _failCount++;
                if (_failCount >= 3)
                    db.Execute("UPDATE demo_users SET status='blocked' WHERE login=@l", ("@l", login));
                return;
            }

            if (byBoth[5]?.ToString() == "blocked")
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Ошибка");
                return;
            }

            LoginSuccess?.Invoke(this, byBoth[3]?.ToString() ?? "");
        }
        catch
        {
            MessageBox.Show("Ошибка связи с БД! Попробуйте ещё раз.", "Ошибка");
            db.Reconnect();
        }
    }

    private static Label Lbl(string text) =>
        new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
}
```

---

## Task 4: UserPanel

**Files:** `Demo/UserPanel.cs`

- [ ] **Step 1: Создать UserPanel.cs**

```csharp
namespace Demo;

public class UserPanel : Panel
{
    public event EventHandler? LogoutRequested;

    public UserPanel()
    {
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 10, RowCount = 11 };
        for (int i = 0; i < 10; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        for (int i = 0; i < 11; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 11));

        var lbl = new Label
        {
            Text = "Вы успешно вошли как пользователь. Это окно-заглушка для дальнейшей реализации.",
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
        };
        layout.Controls.Add(lbl, 0, 0); layout.SetColumnSpan(lbl, 10);

        for (int row = 1; row <= 9; row++)
            for (int col = 0; col < 10; col++)
                layout.Controls.Add(new Button { Text = $"{col}-{row}", Dock = DockStyle.Fill }, col, row);

        var exitBtn = new Button { Text = "Выйти", Dock = DockStyle.Fill };
        exitBtn.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        layout.Controls.Add(exitBtn, 5, 10);

        Controls.Add(layout);
    }
}
```

---

## Task 5: AdminPanel

**Files:** `Demo/AdminPanel.cs`

- [ ] **Step 1: Создать AdminPanel.cs**

```csharp
namespace Demo;

public class AdminPanel : Panel
{
    public event EventHandler? LogoutRequested;

    private readonly TextBox _newLogin = new() { Dock = DockStyle.Fill };
    private readonly TextBox _newPass  = new() { Dock = DockStyle.Fill };
    private readonly TextBox _newFio   = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _newRole   = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _newStatus = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly ComboBox _usersList  = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox  _editPass   = new() { Dock = DockStyle.Fill };
    private readonly TextBox  _editFio    = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _editRole   = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _editStatus = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

    public AdminPanel()
    {
        Dock = DockStyle.Fill;

        _newRole.Items.AddRange(new object[] { "user", "admin" });
        _newStatus.Items.AddRange(new object[] { "active", "blocked" });
        _editRole.Items.AddRange(new object[] { "user", "admin" });
        _editStatus.Items.AddRange(new object[] { "active", "blocked" });

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 10, RowCount = 14 };
        for (int i = 0; i < 10; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        for (int i = 0; i < 14; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 14));

        // ---- Левая секция: добавить ----
        Add(layout, Lbl("Создать нового пользователя"), 1, 1);
        Add(layout, Lbl("Логин:"),   1, 2); Add(layout, _newLogin,   1, 3);
        Add(layout, Lbl("Пароль:"),  1, 4); Add(layout, _newPass,    1, 5);
        Add(layout, Lbl("ФИО:"),     1, 6); Add(layout, _newFio,     1, 7);
        Add(layout, _newRole,   1, 8);
        Add(layout, _newStatus, 1, 9);
        var addBtn = new Button { Text = "Добавить пользователя", Dock = DockStyle.Fill };
        addBtn.Click += OnAddUser;
        Add(layout, addBtn, 1, 10);

        // ---- Правая секция: изменить ----
        Add(layout, Lbl("Изменить пользователя"), 6, 0);
        Add(layout, _usersList, 6, 1);
        var refreshBtn = new Button { Text = "Обновить список", Dock = DockStyle.Fill };
        refreshBtn.Click += (_, _) => RefreshUsers();
        Add(layout, refreshBtn, 7, 1);
        var alterBtn = new Button { Text = "Изменить пользователя", Dock = DockStyle.Fill };
        alterBtn.Click += OnAlterUser;
        Add(layout, alterBtn, 6, 2);
        Add(layout, _editPass,   7, 2);
        Add(layout, _editFio,    7, 3);
        Add(layout, _editRole,   7, 4);
        Add(layout, _editStatus, 7, 5);

        // ---- Выход ----
        var exitBtn = new Button { Text = "Выйти", Dock = DockStyle.Fill };
        exitBtn.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        Add(layout, exitBtn, 4, 8);

        Controls.Add(layout);

        _usersList.SelectedIndexChanged += OnUserSelected;
    }

    public void Init() => RefreshUsers();

    private void RefreshUsers()
    {
        var db = ((MainForm)FindForm()!).Db;
        try
        {
            var logins = db.QueryColumn("SELECT login FROM demo_users");
            _usersList.Items.Clear();
            foreach (var l in logins) _usersList.Items.Add(l);
        }
        catch
        {
            MessageBox.Show("Ошибка обновления списка.", "Ошибка");
            db.Reconnect();
        }
    }

    private void OnUserSelected(object? sender, EventArgs e)
    {
        if (_usersList.SelectedItem is not string login) return;
        var db = ((MainForm)FindForm()!).Db;
        try
        {
            var row = db.QueryOne("SELECT * FROM demo_users WHERE login=@l", ("@l", login));
            if (row == null) return;
            _editPass.Text   = row[2]?.ToString() ?? "";
            _editRole.Text   = row[3]?.ToString() ?? "";
            _editFio.Text    = row[4]?.ToString() ?? "";
            _editStatus.Text = row[5]?.ToString() ?? "";
        }
        catch { MessageBox.Show("Ошибка связи с БД.", "Ошибка"); }
    }

    private void OnAddUser(object? sender, EventArgs e)
    {
        var login  = _newLogin.Text.Trim();
        var pass   = _newPass.Text.Trim();
        var fio    = _newFio.Text.Trim();
        var role   = _newRole.Text;
        var status = _newStatus.Text;

        if (login.Length == 0 || pass.Length == 0 || fio.Length == 0 || role.Length == 0 || status.Length == 0)
        {
            MessageBox.Show("Поля не могут быть пустыми", "Ошибка");
            return;
        }

        var db = ((MainForm)FindForm()!).Db;
        try
        {
            if (db.QueryOne("SELECT * FROM demo_users WHERE login=@l", ("@l", login)) != null)
            {
                MessageBox.Show("Логин занят! Попробуйте другой логин", "Ошибка");
                return;
            }
            db.Execute("INSERT INTO demo_users VALUES (NULL,@l,@p,@r,@f,@s)",
                ("@l", login), ("@p", pass), ("@r", role), ("@f", fio), ("@s", status));
            MessageBox.Show("Пользователь успешно зарегистрирован", "Успех");
        }
        catch
        {
            MessageBox.Show("Ошибка! Проверьте данные и попробуйте еще раз", "Ошибка");
            db.Reconnect();
        }
    }

    private void OnAlterUser(object? sender, EventArgs e)
    {
        if (_usersList.SelectedItem is not string login) return;
        var db = ((MainForm)FindForm()!).Db;
        try
        {
            db.Execute("UPDATE demo_users SET password=@p,roles=@r,FIO=@f,status=@s WHERE login=@l",
                ("@p", _editPass.Text.Trim()), ("@r", _editRole.Text),
                ("@f", _editFio.Text.Trim()),  ("@s", _editStatus.Text), ("@l", login));
            MessageBox.Show("Пользователь успешно изменен", "Успех");
        }
        catch
        {
            MessageBox.Show("Ошибка! Проверьте данные и попробуйте еще раз", "Ошибка");
            db.Reconnect();
        }
    }

    private static void Add(TableLayoutPanel l, Control c, int col, int row) => l.Controls.Add(c, col, row);
    private static Label Lbl(string text) =>
        new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
}
```

---

## Task 6: DatabaseHelper

**Files:** `Demo/DatabaseHelper.cs`

- [ ] **Step 1: Создать DatabaseHelper.cs**

```csharp
using MySqlConnector;

namespace Demo;

public class DatabaseHelper : IDisposable
{
    private MySqlConnection _conn;
    private const string ConnStr =
        "Server=quaponumeno.beget.app;Port=3306;Database=default-db;Uid=default-db;Pwd=14112000Mavrin@;";

    public DatabaseHelper()
    {
        _conn = new MySqlConnection(ConnStr);
        _conn.Open();
    }

    public object?[]? QueryOne(string sql, params (string name, object value)[] parameters)
    {
        using var cmd = Build(sql, parameters);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var row = new object?[reader.FieldCount];
        reader.GetValues(row!);
        return row;
    }

    public List<string> QueryColumn(string sql)
    {
        using var cmd = new MySqlCommand(sql, _conn);
        using var reader = cmd.ExecuteReader();
        var list = new List<string>();
        while (reader.Read()) list.Add(reader.GetString(0));
        return list;
    }

    public void Execute(string sql, params (string name, object value)[] parameters)
    {
        using var cmd = Build(sql, parameters);
        cmd.ExecuteNonQuery();
    }

    public void Reconnect()
    {
        try { _conn.Close(); } catch { }
        _conn = new MySqlConnection(ConnStr);
        _conn.Open();
    }

    private MySqlCommand Build(string sql, (string name, object value)[] p)
    {
        var cmd = new MySqlCommand(sql, _conn);
        foreach (var (name, value) in p) cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }

    public void Dispose() => _conn.Dispose();
}
```

---

## Task 7: MainForm + Program.cs

**Files:** `Demo/MainForm.cs`, `Demo/Program.cs`

- [ ] **Step 1: Создать MainForm.cs**

```csharp
namespace Demo;

public class MainForm : Form
{
    public DatabaseHelper Db { get; private set; }
    private readonly LoginPanel _loginPanel;
    private readonly UserPanel  _userPanel;
    private readonly AdminPanel _adminPanel;

    public MainForm()
    {
        var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pics");

        Text = "Authorize_System";
        Size = new Size(1200, 720);

        try { Db = new DatabaseHelper(); }
        catch
        {
            MessageBox.Show("Ошибка связи с БД! Проверьте сеть и перезапустите приложение.", "Ошибка");
            Db = new DatabaseHelper();
        }

        _loginPanel = new LoginPanel(imagesPath);
        _userPanel  = new UserPanel();
        _adminPanel = new AdminPanel();

        Controls.AddRange(new Control[] { _loginPanel, _userPanel, _adminPanel });
        _userPanel.Visible  = false;
        _adminPanel.Visible = false;

        _loginPanel.LoginSuccess       += OnLoginSuccess;
        _userPanel.LogoutRequested     += (_, _) => Show(_loginPanel);
        _adminPanel.LogoutRequested    += (_, _) => Show(_loginPanel);

        BuildMenu();
        FormClosed += (_, _) => Db.Dispose();
    }

    private void OnLoginSuccess(object? sender, string role)
    {
        if      (role == "user")  { Show(_userPanel); }
        else if (role == "admin") { _adminPanel.Init(); Show(_adminPanel); }
        else MessageBox.Show("Ошибка с ролью. Обратитесь к администратору.", "Ошибка");
    }

    private void Show(Panel panel)
    {
        _loginPanel.Visible = _userPanel.Visible = _adminPanel.Visible = false;
        panel.Visible = true;
    }

    private void BuildMenu()
    {
        var themeMenu = new ToolStripMenuItem("Theme");
        var themes = new (string name, Color back, Color fore)[]
        {
            ("Light", SystemColors.Control,          SystemColors.ControlText),
            ("Dark",  Color.FromArgb(40, 40, 40),    Color.WhiteSmoke),
            ("Blue",  Color.FromArgb(220, 230, 245), Color.FromArgb(20, 40, 80)),
        };
        foreach (var (name, back, fore) in themes)
        {
            var item = new ToolStripMenuItem(name);
            item.Click += (_, _) => { BackColor = back; ForeColor = fore; };
            themeMenu.DropDownItems.Add(item);
        }
        var menu = new MenuStrip();
        menu.Items.Add(themeMenu);
        Controls.Add(menu);
        MainMenuStrip = menu;
    }
}
```

- [ ] **Step 2: Обновить Program.cs**

```csharp
namespace Demo;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
```

- [ ] **Step 3: Финальная сборка**

```bash
dotnet build Demo/Demo.csproj
```
Expected: `Build succeeded. 0 Error(s)`

---

## Итоговая структура

```
Demo/
├── Demo.csproj
├── Program.cs
├── MainForm.cs
├── DatabaseHelper.cs
├── CaptchaControl.cs
├── LoginPanel.cs
├── UserPanel.cs
├── AdminPanel.cs
└── pics/
    ├── 1.png .. 4.png
```