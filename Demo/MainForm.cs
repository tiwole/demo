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

        Controls.AddRange(_loginPanel, _userPanel, _adminPanel);
        _userPanel.Visible = false;
        _adminPanel.Visible = false;

        _loginPanel.LoginSuccess += OnLoginSuccess;
        _userPanel.LogoutRequested += (_, _) => ShowPanel(_loginPanel);
        _adminPanel.LogoutRequested += (_, _) => ShowPanel(_loginPanel);

        FormClosed += (_, _) => Db.Dispose();
    }

    private void OnLoginSuccess(object? sender, string role)
    {
        switch (role)
        {
            case "user":
                ShowPanel(_userPanel);
                break;
            case "admin":
                _adminPanel.Init(); 
                ShowPanel(_adminPanel);
                break;
            default:
                MessageBox.Show("Ошибка с ролью. Обратитесь к администратору.", "Ошибка");
                break;
        }
    }

    private void ShowPanel(Panel panel)
    {
        _loginPanel.Visible = _userPanel.Visible = _adminPanel.Visible = false;
        panel.Visible = true;
    }
}
