namespace OracleApiDemo.Models
{
    public class CongeDetail
    {
        public string Matricule { get; set; }
        public string CodeMotif { get; set; }
        public string DateDemande { get; set; }
        public string DateDebut { get; set; }
        public string DateFin { get; set; }
        public string DateRetour { get; set; }  // nullable si besoin (string ?)
        public string NombreJours { get; set; }
        public string LibelleMotif { get; set; }
        public decimal SoldConge { get; set; }
        public string DateFormatted { get; set; }
    }
}
