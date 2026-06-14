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
            Text = "Вы успешно вошли как пользователь.",
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
