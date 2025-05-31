using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using OracleApiDemo.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace OracleApiDemo.Services
{
    public class OraclePointageService : IOraclePointageService
    {
        private readonly string _connectionString;
        private readonly ILogger<OraclePaieService> _logger;

        public OraclePointageService(IConfiguration configuration, ILogger<OraclePaieService> logger)
    {
        _connectionString = configuration.GetConnectionString("OracleDb"); // ou "OracleDb"
        _logger = logger;
    }

        public async Task<IEnumerable<PointageDto>> GetPointageAsync(string matricule, int? mois)
        {
            var sql = @"
            SELECT cod_soc, mat_pers, date_point, cod_reg, base, brut, pause, marge, retard, pres_j,
                   pres_n, hsupj, hsupn, credit, semaine, dat_poinfr, s
            FROM (
                SELECT cod_soc, mat_pers, TO_CHAR(dat_point, 'DD/MM/YYYY') AS date_point, cod_reg,
                       TO_CHAR(base, '0D90') AS base,
                       TO_NUMBER(brut) AS brut, pause, marge,
                       TO_CHAR(retard, '990D90') AS retard, pres_j, pres_n,
                       TO_CHAR(hsupj, '0D90') AS hsupj, hsupn,
                       TO_CHAR(credit, '0D90') AS credit, semaine,
                       TO_CHAR(dat_point, 'MONTH', 'NLS_DATE_LANGUAGE = French') AS dat_poinfr,
                       TO_CHAR(debit, '990D90') AS s
                FROM grh.recap_journee
                WHERE TO_NUMBER(mat_pers) = :matricule AND dat_point > TO_DATE('03/11/2024', 'DD/MM/YYYY')
            ) t
            WHERE TO_NUMBER(EXTRACT(MONTH FROM TO_DATE(date_point, 'DD/MM/YYYY'))) = :mois
              AND TO_NUMBER(mat_pers) = :matricule
            ORDER BY date_point DESC";

            using var connection = new OracleConnection(_connectionString);
            var result = await connection.QueryAsync<PointageDto>(sql, new { matricule, mois });
            return result;
        }

        /************pointage du jour******************/
        public async Task<IEnumerable<PointageDuJourDto>> GetPointageDuJourAsync(string matricule, string datepointage)
{
    var sql = @"
        SELECT 
            u.mat_pers AS Matricule,
            u.form_pers AS Form,
            u.nom_pers AS Nom,
            u.pren_pers AS Prenom,
            p.h_point AS Heure,
            p.min_point AS Minute,
            p.nat_point AS NatPoint,
            CASE p.nat_point
                WHEN 'E' THEN 'Entree'
                WHEN 'S' THEN 'Sortie'
                WHEN 'P' THEN 'Debut autorisation'
                WHEN 'R' THEN 'Fin autorisation'
                WHEN 'N' THEN 'Entree régime'
                WHEN 'T' THEN 'Sortie régime'
                ELSE p.nat_point
            END AS NaturePointage
        FROM grh.personnel u
        JOIN grh.pointer_ind p ON u.mat_pers = p.mat_pers
        WHERE p.days = TO_CHAR(TO_DATE(:datepointage,'dd/mm/yyyy'), 'dd')
          AND p.months = TO_CHAR(TO_DATE(:datepointage,'dd/mm/yyyy'), 'mm')
          AND p.years = TO_CHAR(TO_DATE(:datepointage,'dd/mm/yyyy'), 'yy')
          AND u.etat_act = 'A'
          AND p.cod_soc = '01'
          AND TO_NUMBER(u.mat_pers) = :matricule
        ORDER BY p.h_point, p.min_point";

    using var connection = new OracleConnection(_connectionString);
    return await connection.QueryAsync<PointageDuJourDto>(sql, new { matricule, datepointage });
}

public async Task<IEnumerable<PointageSemaineDto>> GetPointageSemaineAsync(string matricule)
{
    var sql = @"
        select cod_soc,
               mat_pers as matricule,
               '102' as moth,
               to_char(base,'00D90') as base,
               to_char(retard,'90D90') as retard,
               to_char(pres_j,'99D90') as presj,
               pres_n,
               hsupj,
               hsupn,
               to_char(credit,'0D90') as credit,
               semaine,
               annee,
               debit,
               suppm,
               creditm,
               retardm
        from grh.recap_semaine 
        where to_number(mat_pers) = to_number(:matricule)
          and ANNEE = (extract(YEAR from SYSTIMESTAMP) - 1)
          and semaine > '43'
        union
        select cod_soc,
               mat_pers as matricule,
               '102' as moth,
               to_char(base,'00D90') as base,
               to_char(retard,'90D90') as retard,
               to_char(pres_j,'99D90') as presj,
               pres_n,
               hsupj,
               hsupn,
               to_char(credit,'0D90') as credit,
               semaine,
               annee,
               debit,
               suppm,
               creditm,
               retardm
        from grh.recap_semaine 
        where to_number(mat_pers) = to_number(:matricule)
          and ANNEE = extract(YEAR from SYSTIMESTAMP)
        order by annee desc, semaine desc";

    using var connection = new OracleConnection(_connectionString);
    var result = await connection.QueryAsync<PointageSemaineDto>(sql, new { matricule });
    return result;
}
public async Task<IEnumerable<AutorisationDto>> GetAutorisationsAsync(string matricule , string mois, string? annee = null)
{
    if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }
    var sql = @"
        SELECT 
            s.date_point as DatePoint,
            (s.h_sortie || ':' || s.min_sortie) AS hsortie,
            (s.h_entree || ':' || s.min_entree) AS hentree,
            s.duree,
            m.lib_mot as LibMot,
            s.duree_h as DureeH,
            s.duree_m as DureeM,
            m.cod_m as CodM
        FROM grh.sortie s
        JOIN grh.motif_j m ON s.cod_m = m.cod_m
        JOIN grh.personnel p ON p.mat_pers = s.mat_pers
        WHERE TO_NUMBER(s.mat_pers) = :matricule
          AND EXTRACT(MONTH FROM s.date_point) = :mois
          AND EXTRACT(YEAR FROM s.date_point) = :annee
          AND s.cod_m <> 'CORI'
    ";

    using var connection = new OracleConnection(_connectionString);
    var result = await connection.QueryAsync<AutorisationDto>(sql, new { matricule , mois ,annee});
    return result;
}
public async Task<IEnumerable<RetardDto>> GetRetardsAsync(string matricule, string mois)
{
    var sql = @"
        SELECT DAT_POINT, NUM_POINT, pointage, H_REG, M_REG, TYPE, DUREE_H, DUREE_M, DUREE_TOT
        FROM (
            SELECT 
                po.DAT_POINT,
                po.NUM_POINT,
                (po.H_POINT || ':' || po.M_POINT) AS pointage,
                po.H_REG,
                po.M_REG,
                po.TYPE,
                po.DUREE_H,
                po.DUREE_M,
                po.DUREE_TOT
            FROM point_journee po
            JOIN personnel p ON p.cod_soc = po.cod_soc AND TO_NUMBER(p.mat_pers) = TO_NUMBER(po.mat_pers)
            WHERE 
                po.cod_soc = '01'
                AND po.TYPE != 'N'
                AND NVL(po.DUREE_TOT, 0) > 0
                AND TO_NUMBER(po.mat_pers) = :matricule
                AND po.dat_point > TO_DATE('03/11/2024', 'DD/MM/YYYY')
        )
        WHERE EXTRACT(MONTH FROM DAT_POINT) = :mois
        ORDER BY DAT_POINT DESC";

    using var connection = new OracleConnection(_connectionString);
    var result = await connection.QueryAsync<RetardDto>(sql, new { matricule, mois });
    return result;
}

public async Task<IEnumerable<NonAutoriseDto>> GetNonAutorisesAsync(string matricule , string mois, string? annee = null)
{
    if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }
    var sql = @"
        SELECT DAT_POINT AS DATPOINT, NUM_POINT AS NUMPOINT, pointage, H_REG, M_REG, TYPE, DUREE_H, DUREE_M, DUREE_TOT
        FROM (
            SELECT 
                po.DAT_POINT,
                po.NUM_POINT,
                (po.H_POINT || ':' || po.M_POINT) AS pointage,
                po.H_REG,
                po.M_REG,
                po.TYPE,
                po.DUREE_H,
                po.DUREE_M,
                po.DUREE_TOT
            FROM point_journee po
            JOIN personnel p ON p.cod_soc = po.cod_soc AND TO_NUMBER(p.mat_pers) = TO_NUMBER(po.mat_pers)
            WHERE 
                po.cod_soc = '01'
                AND po.TYPE = 'N'
                AND TO_NUMBER(po.mat_pers) = :matricule
                AND po.dat_point > TO_DATE('03/11/2024', 'DD/MM/YYYY')
        )
        WHERE EXTRACT(MONTH FROM DAT_POINT) = :mois
        and EXTRACT(YEAR FROM DAT_POINT) = :annee
        ORDER BY DAT_POINT DESC";

    using var connection = new OracleConnection(_connectionString);
    var result = await connection.QueryAsync<NonAutoriseDto>(sql, new { matricule, mois,annee });
    return result;
}
public async Task<IEnumerable<SanctionDto>> GetSanctionsAsync(string matricule, string? annee = null)
{
    if (string.IsNullOrEmpty(annee))
    {
        annee = DateTime.Now.Year.ToString();
    }

    var sql = @"
        SELECT 
            d.mat_pers AS MatPers,
            d.code_m AS CodeM,
            TO_CHAR(d.dat_dcng, 'dd/mm/yyyy') AS DatDcng,
            TO_CHAR(d.dat_debut, 'dd/mm/yyyy') AS DatDebut,
            TO_CHAR(d.dat_fin, 'dd/mm/yyyy') AS DatFin,
            d.dat_retour AS DatRetour,
            TO_CHAR(d.nbr_jours, '90D0') AS NbrJours,
            m.LIB_MOT AS LibMot
        FROM grh.dem_cng d
        JOIN grh.motif_j m ON d.code_m = m.COD_M
        WHERE 
            d.code_m IN ('IRR', 'MRCR', 'MRSS')
            AND TO_NUMBER(TO_CHAR(d.mat_pers)) = :matricule
            AND d.valid = 'O'
            AND d.dat_debut >= TO_DATE('03/11/' || :annee, 'DD/MM/YYYY')
        ORDER BY d.dat_dcng DESC";

    using var connection = new OracleConnection(_connectionString);
    var result = await connection.QueryAsync<SanctionDto>(sql, new { matricule, annee });
    return result;
}

    }
}
