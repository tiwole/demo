using Npgsql;

namespace Demo;

public class PostgresDatabaseHelper : IDisposable
{
    private NpgsqlConnection _conn;
    private const string ConnStr =
        "Host=localhost;Port=5432;Database=demo-db;Username=postgres;Password=password;";

    public PostgresDatabaseHelper()
    {
        _conn = new NpgsqlConnection(ConnStr);
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
        using var cmd = new NpgsqlCommand(sql, _conn);
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
        _conn = new NpgsqlConnection(ConnStr);
        _conn.Open();
    }

    private NpgsqlCommand Build(string sql, (string name, object value)[] p)
    {
        var cmd = new NpgsqlCommand(sql, _conn);
        foreach (var (name, value) in p) cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }

    public void Dispose() => _conn.Dispose();
}