using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ClashWindowsBuild
{
    private const string BuildExecutableArgument = "-clashBuildExecutable";
    private static readonly string[] RequiredStartingScenes =
    {
        "Assets/Scenes/TitleMenu.unity",
        "Assets/Scenes/SampleScene.unity"
    };

    public static void BuildWindows64()
    {
        try
        {
            string executablePath = PrepareOutputPath(
                ReadRequiredArgument(BuildExecutableArgument));
            string[] scenes = GetValidatedScenes();

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.StrictMode
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Windows player build {report.summary.result} with " +
                    $"{report.summary.totalErrors} error(s) and " +
                    $"{report.summary.totalWarnings} warning(s).");
            }

            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException(
                    "Unity reported a successful build but the executable is missing.",
                    executablePath);
            }

            Debug.Log(
                $"Windows x64 player build succeeded: {executablePath} " +
                $"({report.summary.totalSize} bytes, {report.summary.totalTime}).");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetValidatedScenes()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        if (scenes.Length < RequiredStartingScenes.Length)
        {
            throw new InvalidOperationException(
                "The Windows build requires TitleMenu and SampleScene as its first two enabled scenes.");
        }

        for (int index = 0; index < RequiredStartingScenes.Length; index++)
        {
            if (!string.Equals(
                    scenes[index],
                    RequiredStartingScenes[index],
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Enabled scene {index} must be '{RequiredStartingScenes[index]}' " +
                    $"but is '{scenes[index]}'.");
            }
        }

        List<string> missingScenes = scenes
            .Where(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            .ToList();

        if (missingScenes.Count > 0)
        {
            throw new FileNotFoundException(
                $"Enabled build scene(s) are missing: {string.Join(", ", missingScenes)}");
        }

        List<string> duplicateScenes = scenes
            .GroupBy(path => path, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateScenes.Count > 0)
        {
            throw new InvalidOperationException(
                $"Enabled build scene(s) are duplicated: {string.Join(", ", duplicateScenes)}");
        }

        return scenes;
    }

    private static string PrepareOutputPath(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("The build executable path cannot be empty.");
        }

        string fullExecutablePath = Path.GetFullPath(executablePath);
        if (!string.Equals(
                Path.GetExtension(fullExecutablePath),
                ".exe",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The Windows build output must be an .exe path: {fullExecutablePath}");
        }

        string fullOutputDirectory = Path.GetDirectoryName(fullExecutablePath);
        if (string.IsNullOrWhiteSpace(fullOutputDirectory))
        {
            throw new ArgumentException(
                $"The Windows build output has no parent directory: {fullExecutablePath}");
        }

        Directory.CreateDirectory(fullOutputDirectory);

        if (Directory.EnumerateFileSystemEntries(fullOutputDirectory).Any())
        {
            throw new InvalidOperationException(
                $"Build output staging directory must be empty: {fullOutputDirectory}");
        }

        return fullExecutablePath;
    }

    private static string ReadRequiredArgument(string argumentName)
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int index = 0; index < arguments.Length - 1; index++)
        {
            if (string.Equals(arguments[index], argumentName, StringComparison.Ordinal))
            {
                string value = arguments[index + 1];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Path.GetFullPath(value);
                }
            }
        }

        throw new ArgumentException($"Missing required command-line argument '{argumentName}'.");
    }
}
