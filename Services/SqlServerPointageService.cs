using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

public class SqlServerPointageService : ISqlServerPointageService
{
    private readonly string _connectionString;

    public SqlServerPointageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection");
    }

    public async Task<IEnumerable<PointageDistantDto>> GetPointageDistantAsync(string matricule, string tpers, string datedeb)
    {
        var sql = @"
            SELECT nom_prenom as NomPrenom, matricule, entree_sortie as EntreeSortie, mouvement, departement,
                   CASE WHEN mouvement % 2 = 0 THEN 'Sortie' ELSE 'Entrée' END AS motif
            FROM pointage1
            WHERE matricule = @matricule
              AND CONVERT(DATE, entree_sortie) = @datedeb
            ORDER BY entree_sortie";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<PointageDistantDto>(sql, new { matricule, tpers, datedeb });
    }
}
