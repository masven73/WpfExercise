using System;
using System.IO;
using System.Security.Cryptography;

namespace WpfExercise.Services;

/// <summary>
/// Calculates a hash on a file to detect changes
/// </summary>
public static class HashCalculator
{
    public static string ComputeHash(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException();

        using var algorithm = SHA256.Create();
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var hash = algorithm.ComputeHash(fs);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }
}