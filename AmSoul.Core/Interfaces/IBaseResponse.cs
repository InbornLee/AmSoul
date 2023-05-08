namespace AmSoul.Core;

public interface IBaseResponse
{
    bool Succeeded { get; set; }
    string? Message { get; set; }
}