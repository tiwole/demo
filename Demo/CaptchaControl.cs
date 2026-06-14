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

        do { Shuffle(); } while (IsSolved);
        _counter = 0;
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
