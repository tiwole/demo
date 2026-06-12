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
        
        var inner = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(0),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
            Anchor = AnchorStyles.None
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (int i = 0; i < 7; i++) inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _loginBox.Width = 300;
        _passBox.Width = 300;

        var shuffleBtn = new Button
        {
            Text = "Решить капчу", 
            Width = 300, 
            Height = 28, 
            Margin = new Padding(0, 4, 0, 2)
        };
        shuffleBtn.Click += (_, _) => _captcha.Shuffle();

        var loginBtn = new Button { Text = "Войти", Width = 300, Height = 28, Margin = new Padding(0, 2, 0, 0) };
        loginBtn.Click += OnLogin;

        inner.Controls.Add(Lbl("Логин:"), 0, 0);
        inner.Controls.Add(_loginBox, 0, 1);
        inner.Controls.Add(Lbl("Пароль:"), 0, 2);
        inner.Controls.Add(_passBox, 0, 3);
        inner.Controls.Add(_captcha, 0, 4);
        inner.Controls.Add(shuffleBtn, 0, 5);
        inner.Controls.Add(loginBtn, 0, 6);

        // Центрируем inner внутри этого Panel
        SizeChanged += (_, _) =>
        {
            inner.Location = new Point(
                (Width - inner.Width) / 2,
                (Height - inner.Height) / 2);
        };

        Controls.Add(inner);
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

            var byBoth = db.QueryOne("SELECT * FROM demo_users WHERE login=@l AND password=@p", ("@l", login),
                ("@p", pass));
            if (byBoth == null || !_captcha.IsSolved)
            {
                MessageBox.Show(
                    "Вы ввели неверный пароль или не решили капчу.\nПожалуйста проверьте еще раз введенные данные",
                    "Ошибка");
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
        new() { Text = text, AutoSize = true, Margin = new Padding(0, 6, 0, 2) };
}