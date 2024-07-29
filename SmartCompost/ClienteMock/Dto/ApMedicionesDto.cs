namespace MockSmartcompost.Dto
{
    public class ApMedicionesDto
    {
        public DateTime last_updated { get; set; }
        public List<MedicionesNodoDto> nodes_measurements { get; set; } = new List<MedicionesNodoDto>();
    }
}
