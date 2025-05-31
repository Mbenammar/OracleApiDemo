namespace OracleApiDemo.Models
{
    public class VoletPaieDto
    {
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string Genre { get; set; }
        public string Civilite { get; set; }
        public string CodeSite { get; set; }
        public string Site { get; set; }
        public DateTime? DateFonction { get; set; }
        public DateTime? DateEmbauche { get; set; }
        public DateTime? DateAffectation { get; set; }
        public string Classe { get; set; }
        public string Grade { get; set; }
        public string Echelle { get; set; }
        public string Echelon{ get; set; }
        public string CodeQualification { get; set; }
        public string Qualification { get; set; }
        public string CodeFonction { get; set; }
        public string Fonction { get; set; }
        public string CodeAffectation { get; set; }
        public string Affectation { get; set; }
        public string CodeService { get; set; }
        public string Services { get; set; }
        public string CodeRESP { get; set; }
        public string Responsabilite { get; set; }
        public string Position { get; set; }
        public string CodeNiveau { get; set; }
        public string NumNiveau { get; set; }
        public string LibNiveau { get; set; }
        public string CodeTypeBulletin { get; set; }
        public string CodeBulletin { get; set; }
        public string CodeRubrique { get; set; }
        public string LibFixe { get; set; }
        public string SensImput { get; set; }
        public decimal? Montant { get; set; }
        public DateTime? DateVirement { get; set; }
    }
}