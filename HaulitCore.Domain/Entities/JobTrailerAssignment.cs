namespace HaulitCore.Domain.Entities;

public class JobTrailerAssignment
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid JobId { get; private set; }
    public Guid TrailerAssetId { get; private set; }

    // 1 = first trailer, 2 = second trailer
    public int Position { get; private set; }

    private JobTrailerAssignment() { } // EF

    public JobTrailerAssignment(Guid jobId, Guid trailerAssetId, int position)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("JobId required.", nameof(jobId));

        if (trailerAssetId == Guid.Empty)
            throw new ArgumentException("TrailerAssetId required.", nameof(trailerAssetId));

        if (position is < 1 or > 2)
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be 1 or 2.");

        JobId = jobId;
        TrailerAssetId = trailerAssetId;
        Position = position;
    }

    public void SetPosition(int position)
    {
        if (position is < 1 or > 2)
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be 1 or 2.");

        Position = position;
    }

    public void SetTrailerAsset(Guid trailerAssetId)
    {
        if (trailerAssetId == Guid.Empty)
            throw new ArgumentException("TrailerAssetId required.", nameof(trailerAssetId));

        TrailerAssetId = trailerAssetId;
    }
}