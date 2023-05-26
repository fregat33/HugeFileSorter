namespace HugeFileSorter.Repositories;

public record struct RowsChunk(int Id, IEnumerable<Row> Rows, Action Release);