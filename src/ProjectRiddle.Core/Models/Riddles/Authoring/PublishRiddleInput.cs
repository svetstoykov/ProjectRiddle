namespace ProjectRiddle.Core.Models.Riddles.Authoring;

/// <summary>
/// Represents the input required to publish a riddle.
/// </summary>
/// <param name="Id">The riddle identifier.</param>
/// <param name="PublicationDate">The Sofia calendar date to publish onto when the riddle does not already have one.</param>
public sealed record PublishRiddleInput(Guid Id, DateOnly? PublicationDate);
