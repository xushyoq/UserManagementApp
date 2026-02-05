namespace UserManagement.Helpers;

/// <summary>
/// IMPORTANT: Helper class for generating unique identifiers.
/// NOTE: Used for email confirmation tokens and other unique values.
/// </summary>
public static class IdHelper
{
    /// <summary>
    /// IMPORTANT: Generates a unique ID value.
    /// NOTE: Uses GUID to ensure uniqueness across all instances.
    /// NOTA BENE: This function is required by the task specification.
    /// </summary>
    /// <returns>A unique string identifier</returns>
    public static string GetUniqIdValue()
    {
        // NOTE: GUID provides sufficient uniqueness for tokens
        return Guid.NewGuid().ToString("N");
    }
}



