namespace AmSoul.Core.Interfaces;

public interface IBaseResponse
{
    bool Succeeded { get; set; }
    string? Message { get; set; }
}