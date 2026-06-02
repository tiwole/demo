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
