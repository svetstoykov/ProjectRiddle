using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents account or in-memory play progress for one riddle.
/// </summary>
public sealed class RiddleProgress
{
    private readonly List<RiddleProgressHint> _hints;
    private readonly List<RiddleProgressPosition> _positions;

    /// <summary>
    /// Initializes riddle progress.
    /// </summary>
    /// <param name="id">The stable progress identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="accountId">The owning account identifier, or <see cref="Guid.Empty" /> for anonymous in-memory progress.</param>
    /// <param name="riddleId">The riddle identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="status">The current play status.</param>
    /// <param name="answerAttemptCount">The total number of accepted answer submissions.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the last change.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an identifier is empty or <paramref name="answerAttemptCount" /> is negative.</exception>
    public RiddleProgress(
        Guid id,
        Guid accountId,
        Guid riddleId,
        RiddleProgressStatus status,
        int answerAttemptCount,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(riddleId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegative(answerAttemptCount);

        Id = id;
        AccountId = accountId;
        RiddleId = riddleId;
        Status = status;
        AnswerAttemptCount = answerAttemptCount;
        UpdatedAtUtc = updatedAtUtc;
        _hints = [];
        _positions = [];
    }

    /// <summary>
    /// Gets the stable progress identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the owning account identifier, or <see cref="Guid.Empty" /> for anonymous in-memory progress.
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Gets the riddle identifier.
    /// </summary>
    public Guid RiddleId { get; }

    /// <summary>
    /// Gets the current play status.
    /// </summary>
    public RiddleProgressStatus Status { get; private set; }

    /// <summary>
    /// Gets the total number of accepted answer submissions.
    /// </summary>
    public int AnswerAttemptCount { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of the last change.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the recorded structural hint kinds.
    /// </summary>
    public IReadOnlyList<RiddleProgressHint> Hints => _hints;

    /// <summary>
    /// Gets the unique revealed letter positions.
    /// </summary>
    public IReadOnlyList<RiddleProgressPosition> Positions => _positions;

    /// <summary>
    /// Gets the recorded structural hint kinds as a set.
    /// </summary>
    public IReadOnlySet<RiddleRangeKind> UsedHints => _hints.Select(hint => hint.Kind).ToHashSet();

    /// <summary>
    /// Gets the unique revealed letter positions as a set.
    /// </summary>
    public IReadOnlySet<int> RevealedPositions => _positions.Select(position => position.LetterPosition).ToHashSet();

    /// <summary>
    /// Gets the number of unique revealed letter positions.
    /// </summary>
    public int LetterRevealCount => _positions.Count;

    /// <summary>
    /// Creates empty in-progress state for a riddle.
    /// </summary>
    /// <param name="accountId">The owning account identifier, or <see cref="Guid.Empty" /> for anonymous progress.</param>
    /// <param name="riddleId">The riddle identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    /// <returns>A new in-progress snapshot.</returns>
    public static RiddleProgress Start(Guid accountId, Guid riddleId, DateTimeOffset updatedAtUtc)
    {
        return new RiddleProgress(
            Guid.NewGuid(),
            accountId,
            riddleId,
            RiddleProgressStatus.InProgress,
            answerAttemptCount: 0,
            updatedAtUtc);
    }

    /// <summary>
    /// Replaces recorded hint kinds.
    /// </summary>
    /// <param name="hints">The hint records. Cannot be <see langword="null" />.</param>
    public void ReplaceHints(IReadOnlyList<RiddleProgressHint> hints)
    {
        ArgumentNullException.ThrowIfNull(hints);
        _hints.Clear();
        _hints.AddRange(hints);
    }

    /// <summary>
    /// Replaces recorded revealed positions.
    /// </summary>
    /// <param name="positions">The position records. Cannot be <see langword="null" />.</param>
    public void ReplacePositions(IReadOnlyList<RiddleProgressPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);
        _positions.Clear();
        _positions.AddRange(positions);
    }

    /// <summary>
    /// Records an answer submission while the snapshot is in progress.
    /// </summary>
    /// <param name="isCorrect">A value indicating whether the submitted answer is correct.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    /// <remarks>
    /// Terminal snapshots are left unchanged. A correct in-progress answer becomes <see cref="RiddleProgressStatus.Solved" />.
    /// </remarks>
    public void RecordAnswer(bool isCorrect, DateTimeOffset updatedAtUtc)
    {
        if (Status is not RiddleProgressStatus.InProgress)
        {
            return;
        }

        AnswerAttemptCount++;
        if (isCorrect)
        {
            Status = RiddleProgressStatus.Solved;
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Records a structural hint kind once.
    /// </summary>
    /// <param name="kind">The structural hint kind.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    public void RecordHint(RiddleRangeKind kind, DateTimeOffset updatedAtUtc)
    {
        if (_hints.Any(hint => hint.Kind == kind))
        {
            return;
        }

        _hints.Add(new RiddleProgressHint(kind));
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Records a previously unrevealed letter position and marks complete reveal when every letter is present.
    /// </summary>
    /// <param name="letterPosition">The zero-based letter position. Cannot be negative.</param>
    /// <param name="letterCount">The total number of letters in the answer. Must be greater than zero.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    /// <returns><see langword="true" /> when the position was newly stored; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="letterPosition" /> is negative or <paramref name="letterCount" /> is not positive.</exception>
    public bool RecordReveal(int letterPosition, int letterCount, DateTimeOffset updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(letterPosition);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(letterCount);

        if (Status is RiddleProgressStatus.Solved)
        {
            return false;
        }

        if (_positions.Any(position => position.LetterPosition == letterPosition))
        {
            return false;
        }

        _positions.Add(new RiddleProgressPosition(letterPosition));
        if (Status is RiddleProgressStatus.InProgress && _positions.Count >= letterCount)
        {
            Status = RiddleProgressStatus.FullyRevealed;
        }

        UpdatedAtUtc = updatedAtUtc;
        return true;
    }

    /// <summary>
    /// Merges another snapshot monotonically into this record.
    /// </summary>
    /// <param name="answerAttemptCount">The imported attempt total.</param>
    /// <param name="status">The imported status.</param>
    /// <param name="usedHints">The imported hint kinds. Cannot be <see langword="null" />.</param>
    /// <param name="revealedPositions">The imported letter positions. Cannot be <see langword="null" />.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    public void MergeFrom(
        int answerAttemptCount,
        RiddleProgressStatus status,
        IReadOnlyCollection<RiddleRangeKind> usedHints,
        IReadOnlyCollection<int> revealedPositions,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(usedHints);
        ArgumentNullException.ThrowIfNull(revealedPositions);
        ArgumentOutOfRangeException.ThrowIfNegative(answerAttemptCount);

        AnswerAttemptCount = Math.Max(AnswerAttemptCount, answerAttemptCount);
        Status = MergeStatus(Status, status);

        foreach (var kind in usedHints)
        {
            if (_hints.All(hint => hint.Kind != kind))
            {
                _hints.Add(new RiddleProgressHint(kind));
            }
        }

        foreach (var letterPosition in revealedPositions)
        {
            if (_positions.All(position => position.LetterPosition != letterPosition))
            {
                _positions.Add(new RiddleProgressPosition(letterPosition));
            }
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Combines two statuses using solved-over-fully-revealed-over-in-progress precedence.
    /// </summary>
    /// <param name="left">The first status.</param>
    /// <param name="right">The second status.</param>
    /// <returns>The monotonic union of the two statuses.</returns>
    public static RiddleProgressStatus MergeStatus(RiddleProgressStatus left, RiddleProgressStatus right)
    {
        if (left is RiddleProgressStatus.Solved || right is RiddleProgressStatus.Solved)
        {
            return RiddleProgressStatus.Solved;
        }

        if (left is RiddleProgressStatus.FullyRevealed || right is RiddleProgressStatus.FullyRevealed)
        {
            return RiddleProgressStatus.FullyRevealed;
        }

        return RiddleProgressStatus.InProgress;
    }
}
