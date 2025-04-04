module Arbitration.Migrations.Migration

open System.IO
open Npgsql

let applyMigrations connectionString = task {
    let connectionString = connectionString
    use conn = new NpgsqlConnection(connectionString)
    do! conn.OpenAsync()

    use ensureTable = new NpgsqlCommand("""
        CREATE TABLE IF NOT EXISTS __migrations (
            id SERIAL PRIMARY KEY,
            name TEXT NOT NULL UNIQUE,
            applied_at TIMESTAMP NOT NULL DEFAULT NOW()
        );
    """, conn)
    let! _ = ensureTable.ExecuteNonQueryAsync()

    let applied = ResizeArray<string>()

    use getApplied = new NpgsqlCommand("SELECT name FROM __migrations", conn)
    use! reader = getApplied.ExecuteReaderAsync()
    while reader.Read() do
        applied.Add(reader.GetString 0)
    do! reader.DisposeAsync()

    let migrationFiles =
        Directory.GetFiles("migrations", "*.sql")
        |> Array.sort

    for file in migrationFiles do
        let name = Path.GetFileName file
        if not (applied.Contains name) then
            let sql = File.ReadAllText file
            use tx = conn.BeginTransaction()
            use cmd = new NpgsqlCommand(sql, conn, tx)
            let! _ = cmd.ExecuteNonQueryAsync()

            use mark = new NpgsqlCommand("INSERT INTO __migrations (name) VALUES (@name)", conn, tx)
            mark.Parameters.AddWithValue("@name", name) |> ignore
            let! _ = mark.ExecuteNonQueryAsync()

            do! tx.CommitAsync()
            printfn $"✅ Applied migration: {name}"
        else
            printfn $"⏩ Skipped already applied: {name}"
}