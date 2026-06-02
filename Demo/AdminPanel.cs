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
