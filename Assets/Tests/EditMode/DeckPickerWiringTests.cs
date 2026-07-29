using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Pepemon.Tests
{
    /// <summary>
    /// Guards the battle deck picker's scene wiring.
    ///
    /// The picker shipped empty three times running, and every cause was scene data rather than
    /// code. The last one was the worst kind: MainMenuController._selectDeckListLoader was
    /// assigned, so a null check passed, but it pointed at a DeckList parented to nothing whose
    /// output target lived inside Screen_5_ManageDecksNew. Decks loaded fine and were parented
    /// into the Mint Deck screen, so that screen filled up while the picker stayed empty. No
    /// error was logged because nothing had failed.
    ///
    /// The scene is read as text on purpose. Opening it needs UnityEditor.SceneManagement plus a
    /// reference to Assembly-CSharp for the component types, and an asmdef test assembly cannot
    /// reference Assembly-CSharp. Parsing the YAML sidesteps that entirely and runs anywhere,
    /// including headless CI where this class of bug has to be caught.
    /// </summary>
    public class DeckPickerWiringTests
    {
        private const string DeckListLoaderGuid = "586d812f1d3d3214c8f36802f1c76a78";
        private const string MainMenuControllerGuid = "4d4e9932512721548b386f8a0399d6b3";
        private const string DeckSelectionScreen = "Screen_4_DeckSelection";

        private static string ScenePath =>
            Path.Combine(Application.dataPath, "Scenes", "StartScreen.unity");

        private Scene _scene;

        [SetUp]
        public void LoadScene()
        {
            Assert.IsTrue(File.Exists(ScenePath), $"Scene not found at {ScenePath}");
            _scene = Scene.Parse(File.ReadAllText(ScenePath));
        }

        [Test]
        public void DeckSelectionScreenExists()
        {
            Assert.IsNotNull(
                _scene.FindGameObjectByName(DeckSelectionScreen),
                $"{DeckSelectionScreen} is missing from the scene.");
        }

        [Test]
        public void DeckSelectionScreenOwnsADeckListLoader()
        {
            var loaders = LoadersUnderDeckSelection();

            Assert.IsNotEmpty(
                loaders,
                $"No DeckListLoader lives under {DeckSelectionScreen}. MainMenuController finds " +
                "the picker with GetComponentInChildren on that screen, so without one the " +
                "battle deck picker is empty and no battle can be started.");
        }

        [Test]
        public void DeckSelectionLoaderIsInSelectModeNotEditMode()
        {
            foreach (var loader in LoadersUnderDeckSelection())
            {
                var editMode = _scene.ReadInt(loader, "_deckEditMode");
                Assert.AreEqual(
                    0, editMode,
                    "The battle deck picker has _deckEditMode on, so each deck renders an Edit " +
                    "button instead of a Select button and no deck can be chosen to fight with.");
            }
        }

        [Test]
        public void DeckSelectionLoaderWritesIntoItsOwnScreen()
        {
            var screen = _scene.FindGameObjectByName(DeckSelectionScreen);

            foreach (var loader in LoadersUnderDeckSelection())
            {
                foreach (var field in new[] { "_deckList", "_loadingMessage" })
                {
                    var target = _scene.ReadReference(loader, field);
                    Assert.IsNotNull(target, $"Picker loader has no {field} assigned.");

                    Assert.IsTrue(
                        _scene.IsDescendantOf(target, screen),
                        $"The picker's {field} points at '{_scene.PathOf(target)}', which is not " +
                        $"inside {DeckSelectionScreen}. Decks would be parented into another " +
                        "screen, leaving the picker empty while that screen fills up.");
                }
            }
        }

        [Test]
        public void SelectDeckListLoaderPointsIntoTheDeckSelectionScreen()
        {
            var controller = _scene.ComponentsWithScript(MainMenuControllerGuid).FirstOrDefault();
            Assert.IsNotNull(controller, "No MainMenuController in the scene.");

            var target = _scene.ReadReference(controller, "_selectDeckListLoader");
            if (target == null) return; // Unassigned is fine: the code falls back to a search.

            var screen = _scene.FindGameObjectByName(DeckSelectionScreen);
            Assert.IsTrue(
                _scene.IsDescendantOf(target, screen),
                $"_selectDeckListLoader points at '{_scene.PathOf(target)}', outside " +
                $"{DeckSelectionScreen}. An assigned-but-wrong reference is more dangerous than " +
                "an unassigned one, because it passes every null check on the way to loading " +
                "decks into the wrong screen.");
        }

        private List<string> LoadersUnderDeckSelection()
        {
            var screen = _scene.FindGameObjectByName(DeckSelectionScreen);
            Assert.IsNotNull(screen, $"{DeckSelectionScreen} is missing from the scene.");

            return _scene.ComponentsWithScript(DeckListLoaderGuid)
                .Where(c => _scene.IsDescendantOf(_scene.OwnerOf(c), screen))
                .ToList();
        }

        /// <summary>Minimal reader for the bits of Unity's scene YAML these assertions need.</summary>
        private class Scene
        {
            private readonly Dictionary<string, string> _bodyById = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _classById = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _nameById = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _ownerByComponent = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _transformByGameObject = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _gameObjectByTransform = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _parentByTransform = new Dictionary<string, string>();

            public static Scene Parse(string text)
            {
                var scene = new Scene();

                foreach (var doc in Regex.Split(text, "\n--- !u!"))
                {
                    var header = Regex.Match(doc, @"^(\d+) &(\d+)");
                    if (!header.Success) continue;

                    string unityClass = header.Groups[1].Value, id = header.Groups[2].Value;
                    scene._bodyById[id] = doc;
                    scene._classById[id] = unityClass;

                    if (unityClass == "1")
                    {
                        var name = Regex.Match(doc, @"^  m_Name: (.*)$", RegexOptions.Multiline);
                        if (name.Success) scene._nameById[id] = name.Groups[1].Value.Trim();
                    }

                    var ownerMatch = Regex.Match(doc, @"m_GameObject: \{fileID: (\d+)\}");

                    // 114 MonoBehaviour, 4 Transform, 224 RectTransform.
                    if (unityClass == "114" && ownerMatch.Success)
                    {
                        scene._ownerByComponent[id] = ownerMatch.Groups[1].Value;
                    }

                    if ((unityClass == "4" || unityClass == "224") && ownerMatch.Success)
                    {
                        string owner = ownerMatch.Groups[1].Value;
                        scene._transformByGameObject[owner] = id;
                        scene._gameObjectByTransform[id] = owner;

                        var father = Regex.Match(doc, @"m_Father: \{fileID: (\d+)\}");
                        if (father.Success) scene._parentByTransform[id] = father.Groups[1].Value;
                    }
                }

                return scene;
            }

            public IEnumerable<string> ComponentsWithScript(string guid) =>
                _ownerByComponent.Keys.Where(id => _bodyById[id].Contains(guid));

            public string OwnerOf(string componentId) => _ownerByComponent[componentId];

            public string FindGameObjectByName(string name) =>
                _nameById.FirstOrDefault(kv => kv.Value == name).Key;

            public bool HasParent(string gameObjectId)
            {
                if (!_transformByGameObject.TryGetValue(gameObjectId, out var transform)) return false;
                return _parentByTransform.TryGetValue(transform, out var father) && father != "0";
            }

            public bool IsDescendantOf(string gameObjectId, string ancestorId)
            {
                for (var current = gameObjectId; current != null;)
                {
                    if (current == ancestorId) return true;
                    if (!_transformByGameObject.TryGetValue(current, out var transform)) return false;
                    if (!_parentByTransform.TryGetValue(transform, out var father) || father == "0") return false;
                    _gameObjectByTransform.TryGetValue(father, out current);
                }

                return false;
            }

            public string PathOf(string gameObjectId)
            {
                var parts = new List<string>();
                for (var current = gameObjectId; current != null;)
                {
                    parts.Add(_nameById.TryGetValue(current, out var n) ? n : "?");
                    if (!_transformByGameObject.TryGetValue(current, out var transform)) break;
                    if (!_parentByTransform.TryGetValue(transform, out var father) || father == "0") break;
                    if (!_gameObjectByTransform.TryGetValue(father, out current)) break;
                }

                parts.Reverse();
                return string.Join("/", parts);
            }

            public int ReadInt(string componentId, string field)
            {
                var m = Regex.Match(_bodyById[componentId], $@"{field}: (\d+)");
                return m.Success ? int.Parse(m.Groups[1].Value) : -1;
            }

            /// <summary>Returns the referenced GameObject id, or null when unassigned.</summary>
            public string ReadReference(string componentId, string field)
            {
                var m = Regex.Match(_bodyById[componentId], $@"{field}: \{{fileID: (-?\d+)");
                if (!m.Success || m.Groups[1].Value == "0") return null;

                string id = m.Groups[1].Value;

                // Scene references point at the GameObject directly; resolve any that name a
                // component so callers always get something comparable against a screen.
                if (_classById.TryGetValue(id, out var unityClass) && unityClass != "1")
                {
                    if (_ownerByComponent.TryGetValue(id, out var owner)) return owner;
                    if (_gameObjectByTransform.TryGetValue(id, out var go)) return go;
                }

                return id;
            }
        }
    }
}
