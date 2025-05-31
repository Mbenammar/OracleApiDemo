using Oracle.ManagedDataAccess.Client;
using OracleApiDemo.Models;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Text;

namespace OracleApiDemo.Services
{
    public class OraclePersonnelService
    {
        private readonly string _connectionString;
private readonly ILogger<OraclePersonnelService> _logger; // ⬅️ ajoute cette ligne
        public OraclePersonnelService(IConfiguration config, ILogger<OraclePersonnelService> logger)
{
    _connectionString = config.GetConnectionString("OracleDb");
    _logger = logger;  // Assigne correctement le logger ici
}
        public async Task<Personnel?> GetByIdAsync(string id)
        {
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT p.mat_pers,
                       p.nom_pers AS Nom,
                       p.pren_pers AS Prenom,
                       fs.lib_fonct AS Responsabilite,
                       fo1.lib_fonct AS Fonction,
                       s1.cod_serv AS CodeAffection,
                       s1.lib_serv AS Affectation,
                       (SELECT sd.lib_serv FROM grh.service sd WHERE sd.cod_serv = SUBSTR(s1.cod_serv,1,1) || '00000') AS DG,
                       (SELECT sd.lib_serv FROM grh.service sd WHERE sd.cod_serv = SUBSTR(s1.cod_serv,1,2) || '0000' AND sd.cod_serv <> SUBSTR(s1.cod_serv,1,1) || '00000') AS POLE,
                       (SELECT sd.lib_serv FROM grh.service sd WHERE sd.cod_serv = SUBSTR(s1.cod_serv,1,3) || '000' AND sd.cod_serv <> SUBSTR(s1.cod_serv,1,2) || '0000') AS DIV,
                       (SELECT sd.lib_serv FROM grh.service sd WHERE sd.cod_serv = SUBSTR(s1.cod_serv,1,4) || '00' AND sd.cod_serv <> SUBSTR(s1.cod_serv,1,3) || '000') AS DEP,
                       (SELECT sd.lib_serv FROM grh.service sd WHERE sd.cod_serv = SUBSTR(s1.cod_serv,1,5) || '0' AND sd.cod_serv <> SUBSTR(s1.cod_serv,1,4) || '00') AS STR
                  FROM grh.personnel p
             LEFT JOIN grh.fonctions fs ON p.mat_cpt = fs.cod_fonct AND fs.typ_fonct = 'R'
             LEFT JOIN (SELECT * FROM grh.fonctions f1 WHERE f1.typ_fonct = 'F') fo1 ON p.cod_fonct = fo1.cod_fonct
             LEFT JOIN grh.service s1 ON p.cod_serv = s1.cod_serv
                 WHERE p.etat_act = 'A' AND p.mat_pers = :id"; // la même requête que précédemment

            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("id", id));

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Personnel
                {
                    Matricule = reader["mat_pers"]?.ToString(),
                    Nom = reader["Nom"]?.ToString(),
                    Prenom = reader["Prenom"]?.ToString(),
                    Responsabilite = reader["Responsabilite"]?.ToString(),
                    Fonction = reader["Fonction"]?.ToString(),
                    CodeAffection = reader["CodeAffection"]?.ToString(),
                    Affectation = reader["Affectation"]?.ToString(),
                    DG = reader["DG"]?.ToString(),
                    POLE = reader["POLE"]?.ToString(),
                    DIV = reader["DIV"]?.ToString(),
                    DEP = reader["DEP"]?.ToString(),
                    STR = reader["STR"]?.ToString()
                };
            }

            return null;
        }

        public async Task<List<Personnel>> GetAllAsync()
{
    var personnels = new List<Personnel>();
    _logger.LogInformation("Exécution de la requête GetAllAsync");

    using var connection = new OracleConnection(_connectionString);
    await connection.OpenAsync();

    string sql = @"
    select p.mat_pers, p.nom_pers as Nom, p.pren_pers as Prenom,
           fs.lib_fonct as Responsabilite, fo1.lib_fonct as fonction,
           s1.cod_serv  as Code_Affection, s1.lib_serv as Affectation,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,1),'00000')) as DG,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,2),'0000') and sd.cod_serv<>concat(substr(s1.cod_serv,1,1),'00000')) as POLE,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,3),'000') and sd.cod_serv<>concat(substr(s1.cod_serv,1,2),'0000')) as DIV,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,4),'00') and sd.cod_serv<>concat(substr(s1.cod_serv,1,3),'000')) as DEP,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,5),'0') and sd.cod_serv<>concat(substr(s1.cod_serv,1,4),'00')) as STR
    from grh.personnel p
    left join grh.fonctions fs on p.mat_cpt = fs.cod_fonct and fs.typ_fonct = 'R'
    left join (select * from grh.fonctions f1 where f1.typ_fonct='F') fo1 on p.cod_fonct=fo1.cod_fonct
    left join grh.service s1 on p.cod_serv=s1.cod_serv
    where p.etat_act = 'A'";

    using var command = new OracleCommand(sql, connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        personnels.Add(new Personnel
        {
            Matricule = reader["mat_pers"]?.ToString(),
            Nom = reader["Nom"]?.ToString(),
            Prenom = reader["Prenom"]?.ToString(),
            Fonction = reader["fonction"]?.ToString(),
            Responsabilite = reader["Responsabilite"]?.ToString(),
            CodeAffection = reader["Code_Affection"]?.ToString(),
            Affectation = reader["Affectation"]?.ToString(),
            DG = reader["DG"]?.ToString(),
            POLE = reader["POLE"]?.ToString(),
            DIV = reader["DIV"]?.ToString(),
            DEP = reader["DEP"]?.ToString(),
            STR = reader["STR"]?.ToString(),
        });
    }

    return personnels;
}

public async Task<List<Personnel>> GetByCodeAffectionAsync(string code)
{
    var personnels = new List<Personnel>();

    using var connection = new OracleConnection(_connectionString);
    await connection.OpenAsync();

    string sql = @"
    select p.mat_pers, p.nom_pers as Nom, p.pren_pers as Prenom,
           fs.lib_fonct as Responsabilite, fo1.lib_fonct as fonction,
           s1.cod_serv  as Code_Affection, s1.lib_serv as Affectation,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,1),'00000')) as DG,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,2),'0000') and sd.cod_serv<>concat(substr(s1.cod_serv,1,1),'00000')) as POLE,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,3),'000') and sd.cod_serv<>concat(substr(s1.cod_serv,1,2),'0000')) as DIV,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,4),'00') and sd.cod_serv<>concat(substr(s1.cod_serv,1,3),'000')) as DEP,
           (select sd.lib_serv from grh.service sd where sd.cod_serv=concat(substr(s1.cod_serv,1,5),'0') and sd.cod_serv<>concat(substr(s1.cod_serv,1,4),'00')) as STR
    from grh.personnel p
    left join grh.fonctions fs on p.mat_cpt = fs.cod_fonct and fs.typ_fonct = 'R'
    left join (select * from grh.fonctions f1 where f1.typ_fonct='F') fo1 on p.cod_fonct=fo1.cod_fonct
    left join grh.service s1 on p.cod_serv=s1.cod_serv
    where p.etat_act = 'A' and s1.cod_serv = :code";

    using var command = new OracleCommand(sql, connection);
    command.Parameters.Add(new OracleParameter("code", code));

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        personnels.Add(new Personnel
        {
            Matricule = reader["mat_pers"]?.ToString(),
            Nom = reader["Nom"]?.ToString(),
            Prenom = reader["Prenom"]?.ToString(),
            Fonction = reader["fonction"]?.ToString(),
            Responsabilite = reader["Responsabilite"]?.ToString(),
            CodeAffection = reader["Code_Affection"]?.ToString(),
            Affectation = reader["Affectation"]?.ToString(),
            DG = reader["DG"]?.ToString(),
            POLE = reader["POLE"]?.ToString(),
            DIV = reader["DIV"]?.ToString(),
            DEP = reader["DEP"]?.ToString(),
            STR = reader["STR"]?.ToString(),
        });
    }

    return personnels;
}
public async Task<List<Dictionary<string, object>>> GetCongeAsync(string matPers)
{
    var result = new List<Dictionary<string, object>>();

    using var connection = new OracleConnection(_connectionString);
    await connection.OpenAsync();

    string query = @"
        select annee_cng,frequence,sold_cng,cum_cng ,to_char(pris_cng,'90D0') as pris_cng,
               cum_cng_ann_1 ,cng_justif,bonus_cng ,obs_cng_a,recup_cng ,init_cng ,jour_fns,
               (select sold_cng from grh.dem_cng ct where ct.num_dcng = (
                    select max (num_dcng) from grh.dem_cng c where c.mat_pers=ac.mat_pers 
                    and c.annee_cng=extract(YEAR from SYSTIMESTAMP) 
                    and to_char(c.dat_debut,'YYYYMMDD')<to_char(sysdate,'YYYYMMDD') 
                    and c.sold_cng is not null) 
                and ct.mat_pers=ac.mat_pers) as sold_actuelle
        from grh.sold_cng ac
        where ac.annee_cng= extract(YEAR from SYSTIMESTAMP) 
        and ac.mat_pers=:mat_pers 
        and ac.typ_cng='01'
    ";

    using var command = new OracleCommand(query, connection);
    command.Parameters.Add(new OracleParameter("mat_pers", matPers));

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
        }
        result.Add(row);
    }

    return result;
}

public async Task<List<DetailCongeModel>> GetDetailCongeAsync(string id)
{
    var result = new List<DetailCongeModel>();

    using var connection = new OracleConnection(_connectionString);
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT 
            d.mat_pers,
            d.code_m,
            TO_CHAR(d.dat_dcng, 'dd/mm/yyyy') AS date_dcng,
            TO_CHAR(d.dat_debut, 'dd/mm/yyyy') AS date_debut,
            TO_CHAR(d.dat_fin, 'dd/mm/yyyy') AS date_fin,
            d.dat_retour,
            TO_CHAR(d.nbr_jours, '90D0') AS jours,
            m.LIB_MOT,
            d.sold_cng,
            TO_CHAR(d.dat_debut, 'yyyymmdd') AS dtf
        FROM grh.dem_cng d
        JOIN grh.motif_j m ON d.code_m = m.COD_M
        WHERE d.mat_pers LIKE :id
          AND d.valid = 'O'
          AND EXTRACT(YEAR FROM d.dat_debut) = EXTRACT(YEAR FROM SYSTIMESTAMP)
        ORDER BY dat_dcng DESC";

    command.Parameters.Add(new OracleParameter("id", $"%{id}"));

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(new DetailCongeModel
        {
            Matricule = reader["mat_pers"]?.ToString(),
            CodeMotif = reader["code_m"]?.ToString(),
            DateDemande = reader["date_dcng"]?.ToString(),
            DateDebut = reader["date_debut"]?.ToString(),
            DateFin = reader["date_fin"]?.ToString(),
            DateRetour = reader["dat_retour"]?.ToString(),
            NombreJours = reader["jours"]?.ToString(),
            LibelleMotif = reader["LIB_MOT"]?.ToString(),
            Solde = reader["sold_cng"]?.ToString(),
            DateDebutFormat = reader["dtf"]?.ToString()
        });
    }

    return result;
}
public async Task<IEnumerable<CongeDetail>> GetDetailCongeAsync(CongeFilter filter)
{
    var results = new List<CongeDetail>();
    var sql = new StringBuilder();
    sql.Append(@"
        select d.mat_pers AS Matricule,
  d.code_m AS CodeMotif,
  TO_CHAR(d.dat_dcng, 'dd/mm/yyyy') AS DateDemande,
  TO_CHAR(d.dat_debut, 'dd/mm/yyyy') AS DateDebut,
  TO_CHAR(d.dat_fin, 'dd/mm/yyyy') AS DateFin,
  d.dat_retour AS DateRetour,
  TO_CHAR(d.nbr_jours, '90D0') AS NombreJours,
  m.LIB_MOT AS LibelleMotif,
  d.sold_cng AS SoldConge,
  TO_CHAR(d.dat_debut, 'yymmdd') AS DateFormatted
        FROM grh.dem_cng d
        JOIN grh.motif_j m ON d.code_m = m.COD_M
        WHERE 1=1
    ");

    var parameters = new DynamicParameters();

    if (!string.IsNullOrEmpty(filter.Matricule))
    {
        sql.Append(" AND d.mat_pers = :matricule");
        parameters.Add("matricule", filter.Matricule);
    }

    if (filter.Annee.HasValue)
    {
        sql.Append(" AND EXTRACT(YEAR FROM d.dat_debut) = :annee");
        parameters.Add("annee", filter.Annee.Value);
    }

    if (!string.IsNullOrEmpty(filter.CodeMotif))
    {
        sql.Append(" AND d.code_m = :codeMotif");
        parameters.Add("codeMotif", filter.CodeMotif);
    }

    if (filter.Valide.HasValue)
    {
        sql.Append(" AND d.valid = :valide");
        parameters.Add("valide", filter.Valide.Value ? "O" : "N");
    }

    sql.Append(" ORDER BY d.dat_dcng DESC");

    _logger.LogInformation("SQL Conge Detail exécuté : {Query}", sql.ToString());

    using var connection = new OracleConnection(_connectionString);
    results = (await connection.QueryAsync<CongeDetail>(sql.ToString(), parameters)).ToList();
    return results;
}

    }
}
