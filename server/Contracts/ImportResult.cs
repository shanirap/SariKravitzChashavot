namespace AccountingProject.Contracts
{
    public class ImportResult
    {
        public int Imported { get; set; }
        public int Errors { get; set; }
        public List<ImportRowResult> Rows { get; set; } = [];
    }
}
