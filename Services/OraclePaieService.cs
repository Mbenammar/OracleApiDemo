using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using OracleApiDemo.Models;
using Microsoft.Extensions.Logging;
using System.Text;


namespace OracleApiDemo.Services
{public class OraclePaieService : IOraclePaieService
{
    private readonly string _connectionString;
    private readonly ILogger<OraclePaieService> _logger;

    public OraclePaieService(IConfiguration configuration, ILogger<OraclePaieService> logger)
    {
        _connectionString = configuration.GetConnectionString("OracleDb"); // ou "OracleDb"
        _logger = logger;
    }

    public async Task<IEnumerable<VoletPaieDto>> GetVoletPaieAsync(string? matricule,int? mois,int? annee,string?codeRubrique)
    {
        var sql = @"WITH Niveau2 AS (
            SELECT mat_pers, MAX(num_niveau) AS num_niveau 
            FROM GRH.NIVEAU_PERS 
            GROUP BY mat_pers
        )
        SELECT 
            p.MAT_PERS AS Matricule,
            p.NOM_PERS AS NOM,
            p.PREN_PERS AS PRENOM,
            p.DAT_NAIS AS DATENAISSANCE,
            p.sexe AS genre,
            bh.cod_sit AS Civilite,
            bh.COD_LIEU_GEOG AS CODESITE,
            pl.LIB_LIEU AS SITE,
            p.dat_fonct AS datefonction,
            p.DAT_EMB AS dateembauche,
            p.dat_affect AS DateAffectation,
            (bh.cod_cat || ' ' || bh.cod_grad || '/' || bh.cod_ech) AS classe,
            bh.cod_cat AS Grade,
            bh.cod_grad AS echelle,
            bh.cod_ech AS Echelon,
            bh.qualf AS Codequalification,
            f3.LIB_FONCT AS Qualification,
            bh.COD_FONCT AS CODEFONCTION,
            f1.LIB_FONCT AS FONCTION,
            bh.cod_affect AS CODEAFFECTATION,
            af.LIB_AFFECT AS AFFECTATION,
            bh.COD_SERV AS CODESERVICE,
            s.LIB_SERV AS SERVICES,
            bh.MAT_CPT AS codeRESP,
            f2.LIB_FONCT AS Responsabilite,
            MM.LIB_MOTIF AS position,
            n.cod_niveau as CODENIVEAU,
            n.num_niveau as NUMNIVEAU,
            d.lib_niveau as LIBNIVEAU,
            phe.COD_TYP_BUL AS codetypebulletin,
            phe.BUL_COD_TYP_BUL AS codebulletin,
            phe.ABRV_FIXE AS coderubrique,
            ab.LIB_FIXE as LIBFIXE,
            ab.SENS_IMPUT as SENSIMPUT,
            phe.MONTV AS montant,
            phe.DATEV AS datevirement
        FROM grh.bulletinh bh
        LEFT JOIN GRH.PERSONNEL p ON bh.mat_pers = p.mat_pers AND bh.cod_soc = p.cod_soc
        LEFT JOIN GRH.SERVICE s ON bh.COD_SERV = s.COD_SERV
        LEFT JOIN GRH.FONCTIONS f1 ON bh.COD_FONCT = f1.COD_FONCT AND f1.TYP_FONCT = 'F'
        LEFT JOIN GRH.FONCTIONS f2 ON bh.MAT_CPT = f2.COD_FONCT AND f2.TYP_FONCT = 'R'
        LEFT JOIN GRH.FONCTIONS f3 ON bh.QUALF = f3.COD_FONCT AND f3.TYP_FONCT = 'Q'
        LEFT JOIN GRH.PRM_LIEU_GEOGRAPHIQUE pl ON bh.COD_LIEU_GEOG = pl.COD_LIEU_GEOG
        LEFT JOIN GRH.GRADE_ADMINISTRATIF ga ON bh.GRADE_ADM = ga.GRADE_ADM
        LEFT JOIN GRH.AFFECTATION af ON bh.cod_affect = af.cod_affect
        LEFT JOIN GRH.MOTIF_SORT mm ON bh.COD_MOTIF = MM.COD_MOTIF
        LEFT JOIN grh.possedevh phe ON bh.dt_bul = phe.dt_bul AND bh.mat_pers = phe.mat_pers 
            AND bh.cod_soc = phe.cod_soc AND bh.cod_typ_bul = phe.cod_typ_bul AND bh.bul_cod_typ_bul = phe.bul_cod_typ_bul 
        LEFT JOIN GRH.PAR_FIXE ab ON phe.ABRV_FIXE = ab.ABRV_FIXE
        LEFT JOIN Niveau2 n2 ON bh.mat_pers = n2.mat_pers
        LEFT JOIN grh.niveau_pers n ON n2.mat_pers = n.mat_pers AND n2.num_niveau = n.num_niveau
        LEFT JOIN grh.param_niveau d ON n.cod_niveau = d.cod_niveau
        WHERE TO_CHAR(bh.dt_bul,'YYYY') = '2025'
          AND (:matricule IS NULL OR p.MAT_PERS = :matricule)
          AND (:mois IS NULL OR TO_CHAR(bh.dt_bul, 'MM') = LPAD(:mois, 2, '0'))
        AND (:annee IS NULL OR TO_CHAR(bh.dt_bul, 'YYYY') = :annee)
        AND (:codeRubrique IS NULL OR phe.ABRV_FIXE = :codeRubrique)

        ORDER BY bh.COD_SERV";

        using var connection = new OracleConnection(_connectionString);

        var result = await connection.QueryAsync<VoletPaieDto>(sql, new { matricule,mois, annee,codeRubrique});
        return result;
    }
}
}