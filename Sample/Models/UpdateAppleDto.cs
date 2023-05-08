using AmSoul.Core;

namespace Sample.Models;

/// <summary>
/// UpdateAppleDto
/// </summary>
public class UpdateAppleDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    public string? Name { get; set; }
}
/// <summary>
/// Model1
/// </summary>
public class UpdateAppleDto1 : IDataModel
{
    /// <summary>
    /// Id
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    public string? Name { get; set; }
}
