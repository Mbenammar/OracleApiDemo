public class PointageDistantDto
{
    public string NomPrenom { get; set; }
    public string Matricule { get; set; }
    public DateTime? EntreeSortie { get; set; }
    public int Mouvement { get; set; }
    public string Departement { get; set; }
    public string EntreeSortieFormatted => EntreeSortie?.ToString("dd/MM/yyyy HH:mm:ss");
}
