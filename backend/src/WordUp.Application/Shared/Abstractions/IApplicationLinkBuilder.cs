namespace WordUp.Application.Shared.Abstractions;

/// <summary>Builds trusted absolute links for email. Raw request host headers are never used.</summary>
public interface IApplicationLinkBuilder
{
    string Portal(string relativePath);
    string PublicSite(string relativePath);
}
