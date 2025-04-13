namespace Code.LevelEditor
{
    public struct LevelDataDTO
    {
        public int IndexLevel;
        public LevelCell[,] Cells;

        public LevelDataDTO(LevelCell[,] cells, int indexLevel)
        {
            Cells = cells;
            IndexLevel = indexLevel;
        }
    }
}