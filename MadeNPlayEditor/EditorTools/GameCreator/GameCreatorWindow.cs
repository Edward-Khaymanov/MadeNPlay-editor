#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MadeNPlayShared
{
    public class GameCreatorWindow : EditorWindow
    {
        [SerializeField] private LocalGameData _gameData = new LocalGameData();
        [SerializeField] private List<GameTeamAsset> _teams = new List<GameTeamAsset>();

        private readonly string _savedGameDataPath = Path.Combine(Application.dataPath, CONSTANTS.GAMEDATA_FILENAME);
        private readonly string _versionMask = @"^\d{1,3}\.\d{1,3}\.\d{1,4}$";

        private string _buildDirectoryPath;
        private string _dllPath;
        private string _iconPath;

        [MenuItem("Window/Game creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameCreatorWindow>("Game creator");
            window.Show();
        }

        private void OnEnable()
        {
            Load();
        }

        private void OnDisable()
        {
            Save(_savedGameDataPath);
        }

        private void OnGUI()
        {
            var buttonHeight = GUILayout.Height(20);

            RenderFields();
            RenderTeams();
            EditorGUILayout.Space();
            RenderIcon();
            RenderBuildDirectory();
            RenderDll();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            var generateId = GUILayout.Button("Generate Id", buttonHeight);
            var create = GUILayout.Button("Create game", buttonHeight);
            EditorGUILayout.EndHorizontal();

            if (generateId)
            {
                _gameData.Id = Guid.NewGuid();
                Save(_savedGameDataPath);
            }

            if (create)
                CreateGame();
        }

        private bool IsValid()
        {
            var stringBuilder = new StringBuilder();
            var errorMessage = string.Empty;

            if (_gameData.Id == Guid.Empty)
                stringBuilder.AppendLine("Generate guid");

            if (string.IsNullOrEmpty(_gameData.Name))
                stringBuilder.AppendLine("Enter name");

            if (string.IsNullOrEmpty(_gameData.Version))
                stringBuilder.AppendLine("Enter version");
            else if (Regex.IsMatch(_gameData.Version, _versionMask) == false)
                stringBuilder.AppendLine("Incorrect version");

            if (_teams.Count == 0)
                stringBuilder.AppendLine("Add team");

            if (_teams.Any(x => x == null))
                stringBuilder.AppendLine("Remove empty team");

            if (string.IsNullOrEmpty(_iconPath))
                stringBuilder.AppendLine("Select icon");

            if (string.IsNullOrEmpty(_buildDirectoryPath))
                stringBuilder.AppendLine("Select build directory");

            if (string.IsNullOrEmpty(_dllPath))
                stringBuilder.AppendLine("Select dll");

            errorMessage = stringBuilder.ToString();
            if (string.IsNullOrEmpty(errorMessage))
                return true;

            EditorUtility.DisplayDialog("Validate error", errorMessage, "OK");
            return false;
        }

        private void CreateGame()
        {
            if (IsValid() == false)
                return;

            var selectedDirectory = EditorUtility.OpenFolderPanel("Select directory", null, null);
            if (string.IsNullOrEmpty(selectedDirectory))
                return;

            var targetDirectory = Directory.CreateDirectory(
                Path.Combine(selectedDirectory, $"{_gameData.Name} {_gameData.Version}"))
                .FullName;
            var gameDataPath = Path.Combine(targetDirectory, CONSTANTS.GAMEDATA_FILENAME);

            ClearDirectory(targetDirectory);
            CopyBuild(targetDirectory);
            CopyIcon(targetDirectory);
            CopyDll(targetDirectory);
            Save(gameDataPath);
            Process.Start("explorer.exe", targetDirectory);
        }

        private void ClearDirectory(string directory)
        {
            var sourceDirectory = new DirectoryInfo(directory);

            foreach (var file in sourceDirectory.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in sourceDirectory.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void CopyIcon(string selectedDirectory)
        {
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            var file = File.ReadAllBytes(_iconPath);
            ImageConversion.LoadImage(tex, file);

            var targetBytes = ImageConversion.EncodeToPNG(tex);
            var iconPath = Path.Combine(selectedDirectory, CONSTANTS.ICON_FILENAME);

            File.WriteAllBytes(iconPath, targetBytes);
        }

        private void CopyDll(string targetDirectory)
        {
            var fileName = Path.GetFileName(_dllPath);
            var targetFilePath = Path.Combine(targetDirectory, fileName);
            File.Copy(_dllPath, targetFilePath, true);
        }

        private void CopyBuild(string selectedDirectory)
        {
            var sourceDirectory = new DirectoryInfo(_buildDirectoryPath);
            var targetDirectory = new DirectoryInfo(selectedDirectory);
            var targetBuildDirectory = targetDirectory.CreateSubdirectory(sourceDirectory.Name);

            foreach (var file in sourceDirectory.GetFiles())
            {
                var targetFilePath = Path.Combine(targetBuildDirectory.FullName, file.Name);
                file.CopyTo(targetFilePath, true);
            }
        }

        private void RenderDll()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                wordWrap = true
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Dll path");
            var button = GUILayout.Button($"{_dllPath}", style);
            EditorGUILayout.EndHorizontal();

            if (button)
            {
                var selectedPath = EditorUtility.OpenFilePanelWithFilters("Select dll", _dllPath, new[] { "Dll files", "dll" });
                if (string.IsNullOrEmpty(selectedPath))
                    return;

                _dllPath = selectedPath;
            }
        }

        private void RenderBuildDirectory()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                wordWrap = true
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel($"Build directory");
            var selectDirectory = GUILayout.Button($"{_buildDirectoryPath}", style);
            EditorGUILayout.EndHorizontal();

            if (selectDirectory)
            {
                var selectedDirectory = EditorUtility.OpenFolderPanel("Select directory", _buildDirectoryPath, null);
                if (string.IsNullOrEmpty(selectedDirectory))
                    return;

                _buildDirectoryPath = selectedDirectory;
            }
        }

        private void RenderIcon()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                wordWrap = true
            };
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Icon path");
            var button = GUILayout.Button($"{_iconPath}", style);
            EditorGUILayout.EndHorizontal();

            if (button)
            {
                var selectedIconPath = EditorUtility
                    .OpenFilePanelWithFilters("Select icon", _iconPath, new[] { "Image files", "png,jpg,jpeg" });
                if (string.IsNullOrEmpty(selectedIconPath))
                    return;

                _iconPath = selectedIconPath;
            }
        }

        private void RenderFields()
        {
            EditorGUILayout.LabelField(nameof(_gameData.Id), $"{_gameData.Id}");
            _gameData.Name = EditorGUILayout.TextField(nameof(_gameData.Name), _gameData.Name);
            RenderVersion();
        }

        private void RenderVersion()
        {
            var label = new GUIContent(
                nameof(_gameData.Version),
                "Example: 111.111.1111 \nMajor: 1-3 \nMinor: 1-3 \nRevision: 1-4");
            var version = EditorGUILayout.TextField(label, _gameData.Version);

            if (version == null)
                version = string.Empty;
            _gameData.Version = Regex.Replace(version, @"[^.\d]", string.Empty);

            if (Regex.IsMatch(_gameData.Version, _versionMask) == false)
            {
                var style = GUI.skin.label;
                var defaultColor = style.normal.textColor;
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("Incorrect version", style);
                style.normal.textColor = defaultColor;
            }
        }

        private void RenderTeams()
        {
            var so = new SerializedObject(this);
            var teamsProperty = so.FindProperty(nameof(_teams));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(teamsProperty, true);
            so.ApplyModifiedProperties();
        }

        private void Save(string path)
        {
            _teams.RemoveAll(x => x == null);
            _teams = _teams.Distinct().ToList();
            _gameData.Teams = _teams.Select(x => x.Team).ToList();
            var serializeSettings = new JsonSerializerSettings()
            {
                ContractResolver = new IgnoreResolver(),
                Formatting = Formatting.Indented
            };
            var jsonData = JsonConvert.SerializeObject(_gameData, serializeSettings);
            File.WriteAllText(path, jsonData);
        }

        private void Load()
        {
            if (File.Exists(_savedGameDataPath) == false)
                return;

            var jsonData = File.ReadAllText(_savedGameDataPath);
            var serializeSettings = new JsonSerializerSettings()
            {
                ContractResolver = new IgnoreResolver()
            };
            var gameData = JsonConvert.DeserializeObject<LocalGameData>(jsonData, serializeSettings);
            if (gameData == null)
                return;

            _gameData = gameData;
            _teams = FindGameTeamAssets(_gameData.Teams);
        }

        private List<GameTeamAsset> FindGameTeamAssets(List<GameTeam> teams)
        {
            var result = new List<GameTeamAsset>();
            var teamsData = new List<GameTeamAsset>();
            var assetsGuid = AssetDatabase.FindAssets($"t:{nameof(GameTeamAsset)}");

            foreach (string guid in assetsGuid)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var teamData = AssetDatabase.LoadAssetAtPath<GameTeamAsset>(path);
                teamsData.Add(teamData);
            }

            foreach (var team in teams)
            {
                var teamso = teamsData.FirstOrDefault(x => x.Team.Id == team.Id);
                result.Add(teamso);
            }

            return result;
        }
    }
}
#endif