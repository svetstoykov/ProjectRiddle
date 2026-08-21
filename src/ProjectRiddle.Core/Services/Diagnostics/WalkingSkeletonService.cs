using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Diagnostics;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Services.Diagnostics;

/// <summary>
/// Executes the trivial operation used to verify the Phase 0 application boundary.
/// </summary>
public sealed class WalkingSkeletonService : IWalkingSkeletonService
{
    private readonly IPublicationDateProvider publicationDateProvider;

    /// <summary>
    /// Initializes the walking-skeleton service.
    /// </summary>
    /// <param name="publicationDateProvider">The provider for the centralized publication date calculation.</param>
    public WalkingSkeletonService(IPublicationDateProvider publicationDateProvider)
    {
        ArgumentNullException.ThrowIfNull(publicationDateProvider);

        this.publicationDateProvider = publicationDateProvider;
    }

    /// <inheritdoc />
    public Task<Result<WalkingSkeletonOutput>> ExecuteAsync(
        WalkingSkeletonInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        if (input.ShouldFail)
        {
            return Task.FromResult(
                Result.Failure<WalkingSkeletonOutput>(
                    new OperationError(
                        "The walking-skeleton failure was requested.",
                        ErrorType.Validation,
                        "WalkingSkeleton.Failure")));
        }

        var output = new WalkingSkeletonOutput(
            "Project Riddle is ready.",
            publicationDateProvider.CurrentDate);

        return Task.FromResult(Result.Success(output));
    }
}
