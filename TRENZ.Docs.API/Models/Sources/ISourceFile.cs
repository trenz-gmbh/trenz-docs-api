namespace TRENZ.Docs.API.Models.Sources;

public interface ISourceFile
{
    /// <summary>
    /// The last part of the location.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The <see cref="RelativePath"/> converted to a display-friendly, normalized format (i.e. using
    /// <see cref="NavNode.PathToLocation"/>).
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// The relative path from a <see cref="ISource"/> path to the file. Contains platform specific separators.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Asynchronously opens a binary file, reads the contents of the file into a byte array, and then closes the file.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the byte array containing the
    /// contents of the file.</returns>
    public Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in
    /// the file.</returns>
    public Task<string> GetTextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all
    /// lines of the file.</returns>
    public Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default);
}