using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TicTacToe.Core;
using TicTacToe.Themes;
using UnityEditor;
using UnityEngine;

namespace TicTacToe.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="ThemeLibrary"/>, which keeps one catalogue per mark.
    /// The reason the catalogues are split is that they are independent — free to differ in
    /// length and each with its own id space — so that is what these cover, together with the
    /// "a broken asset must not take the match down" fallbacks around them.
    /// </summary>
    public sealed class ThemeLibraryTests
    {
        /// <summary>Every asset a test created, destroyed afterwards so none leak into the editor session.</summary>
        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();

        [TearDown]
        public void DestroyCreatedAssets()
        {
            foreach (ScriptableObject asset in _created)
            {
                ScriptableObject.DestroyImmediate(asset);
            }

            _created.Clear();
        }

        // ----- Independent catalogues -----

        [Test]
        public void Catalogues_OfDifferentLengths_EachReportItsOwnCount()
        {
            ThemeLibrary library = CreateAsymmetricLibrary();

            // Counted through the interface rather than with Has.Count: the value behind it is
            // an array, which exposes Length publicly and Count only as an explicit interface
            // implementation, so a reflecting constraint would not find it.
            Assert.That(library.XThemes.Count, Is.EqualTo(4), "The X catalogue holds one look the O catalogue has no answer to.");
            Assert.That(library.OThemes.Count, Is.EqualTo(3));
            Assert.That(library.GetThemes(Mark.X).Count, Is.EqualTo(4));
            Assert.That(library.GetThemes(Mark.O).Count, Is.EqualTo(3));
        }

        [Test]
        public void GetThemes_ForEitherMark_HandsBackThatMarksOwnList()
        {
            // Same instance, not a copy: the arrays are exposed through covariance, so nothing
            // is allocated on the way out and a caller can index them straight away.
            ThemeLibrary library = CreateAsymmetricLibrary();

            Assert.That(library.GetThemes(Mark.X), Is.SameAs(library.XThemes));
            Assert.That(library.GetThemes(Mark.O), Is.SameAs(library.OThemes));
        }

        [Test]
        public void Default_ForEitherMark_IsThatCataloguesFirstEntry()
        {
            ThemeLibrary library = CreateAsymmetricLibrary();

            Assert.That(library.Default(Mark.X), Is.SameAs(library.XThemes[0]));
            Assert.That(library.Default(Mark.O), Is.SameAs(library.OThemes[0]));
        }

        // ----- Mark-scoped lookup -----

        [Test]
        public void Find_IdSharedByBothCatalogues_ResolvesInsideTheRequestedMark()
        {
            // Both halves of one visual style normally carry the same id; the two must never
            // collide, or a player would end up wearing the other mark's artwork.
            ThemeLibrary library = CreateAsymmetricLibrary();

            MarkTheme forX = library.Find(Mark.X, "classic");
            MarkTheme forO = library.Find(Mark.O, "classic");

            Assert.That(forX, Is.SameAs(library.XThemes[0]));
            Assert.That(forO, Is.SameAs(library.OThemes[0]));
            Assert.That(forX, Is.TypeOf<XMarkTheme>());
            Assert.That(forO, Is.TypeOf<OMarkTheme>());
        }

        [Test]
        public void Find_IdOnlyInTheXCatalogue_DoesNotResolveForO()
        {
            // The whole point of the split: "extra" is an X look with no O counterpart, so
            // Player 2 simply never sees it and falls back to their own default.
            ThemeLibrary library = CreateAsymmetricLibrary();

            Assert.That(library.Find(Mark.X, "extra"), Is.SameAs(library.XThemes[3]));
            Assert.That(library.Find(Mark.O, "extra"), Is.SameAs(library.Default(Mark.O)));
        }

        // ----- Fallbacks -----

        [TestCase(Mark.X)]
        [TestCase(Mark.O)]
        public void Find_UnknownId_FallsBackToTheMarksDefault(Mark mark)
        {
            ThemeLibrary library = CreateAsymmetricLibrary();

            Assert.That(library.Find(mark, "deleted-long-ago"), Is.SameAs(library.Default(mark)));
        }

        // Cast so the null picks the single-argument overload instead of the params array one.
        [TestCase((string)null)]
        [TestCase("")]
        public void Find_MissingId_FallsBackToTheMarksDefault(string id)
        {
            // This is the first-run state: nothing saved yet, so both players get the defaults.
            ThemeLibrary library = CreateAsymmetricLibrary();

            Assert.That(library.Find(Mark.X, id), Is.SameAs(library.Default(Mark.X)));
            Assert.That(library.Find(Mark.O, id), Is.SameAs(library.Default(Mark.O)));
        }

        [Test]
        public void Find_NullEntryInTheCatalogue_IsSkippedInsteadOfThrowing()
        {
            // Deleting a theme asset leaves a hole in the array that references it.
            ThemeLibrary library = CreateLibrary(
                new XMarkTheme[] { null, CreateXTheme("chalk") },
                new[] { CreateOTheme("classic") });

            Assert.That(library.Find(Mark.X, "chalk"), Is.SameAs(library.XThemes[1]));
        }

        // ----- Libraries that were never filled in -----

        [Test]
        public void Default_EmptyCatalogue_IsNullAndLeavesTheOtherMarkAlone()
        {
            ThemeLibrary library = CreateLibrary(new XMarkTheme[0], new[] { CreateOTheme("classic") });

            Assert.That(library.Default(Mark.X), Is.Null);
            Assert.That(library.Default(Mark.O), Is.SameAs(library.OThemes[0]), "One empty catalogue must not disable the other.");
        }

        [Test]
        public void Find_EmptyCatalogue_IsNullInsteadOfThrowing()
        {
            ThemeLibrary library = CreateLibrary(new XMarkTheme[0], new OMarkTheme[0]);

            Assert.That(library.Find(Mark.X, "classic"), Is.Null);
            Assert.That(library.Find(Mark.O, "classic"), Is.Null);
        }

        [TestCase(Mark.X)]
        [TestCase(Mark.O)]
        public void Default_NullCatalogue_IsNullInsteadOfThrowing(Mark mark)
        {
            ThemeLibrary library = CreateLibraryWithoutCatalogues();

            Assert.That(library.GetThemes(mark), Is.Null);
            Assert.That(library.Default(mark), Is.Null);
        }

        [TestCase(Mark.X)]
        [TestCase(Mark.O)]
        public void Find_NullCatalogue_IsNullInsteadOfThrowing(Mark mark)
        {
            ThemeLibrary library = CreateLibraryWithoutCatalogues();

            Assert.That(library.Find(mark, "classic"), Is.Null);
        }

        // ----- Helpers -----

        /// <summary>
        /// A deliberately lopsided library: four X looks against three O looks. Three ids are
        /// shared between the catalogues, and "extra" is the X-only look with no O counterpart.
        /// </summary>
        private ThemeLibrary CreateAsymmetricLibrary() => CreateLibrary(
            new[] { CreateXTheme("classic"), CreateXTheme("chalk"), CreateXTheme("crayon"), CreateXTheme("extra") },
            new[] { CreateOTheme("classic"), CreateOTheme("chalk"), CreateOTheme("crayon") });

        private ThemeLibrary CreateLibrary(XMarkTheme[] xThemes, OMarkTheme[] oThemes)
        {
            var library = Create<ThemeLibrary>();

            using var serialized = new SerializedObject(library);
            Fill(serialized.FindProperty("_xThemes"), xThemes);
            Fill(serialized.FindProperty("_oThemes"), oThemes);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return library;
        }

        /// <summary>
        /// A library whose catalogues are genuinely null. Unity turns a null array into an empty
        /// one the moment it serialises, and <see cref="SerializedObject"/> cannot express null
        /// either, so this one state has to be written straight onto the field — it is still
        /// worth covering, because the lookups promise never to throw whatever shape the asset
        /// arrives in.
        /// </summary>
        private ThemeLibrary CreateLibraryWithoutCatalogues()
        {
            var library = Create<ThemeLibrary>();
            ClearCatalogue(library, "_xThemes");
            ClearCatalogue(library, "_oThemes");
            return library;
        }

        private XMarkTheme CreateXTheme(string id) => CreateTheme<XMarkTheme>(id);

        private OMarkTheme CreateOTheme(string id) => CreateTheme<OMarkTheme>(id);

        /// <summary>
        /// A theme carrying just the id and name the lookups care about. The fields are private
        /// with no setters, so they are written through <see cref="SerializedObject"/> exactly as
        /// the Inspector would: the production type keeps its encapsulation intact.
        /// </summary>
        private T CreateTheme<T>(string id) where T : MarkTheme
        {
            var theme = Create<T>();

            using var serialized = new SerializedObject(theme);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return theme;
        }

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _created.Add(asset);
            return asset;
        }

        private static void Fill(SerializedProperty array, MarkTheme[] themes)
        {
            array.arraySize = themes.Length;
            for (int i = 0; i < themes.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = themes[i];
            }
        }

        private static void ClearCatalogue(ThemeLibrary library, string fieldName) =>
            typeof(ThemeLibrary)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(library, null);
    }
}
