using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BannerOfBones.CardGame.Editor
{
    public class CardDatabaseWindow : EditorWindow
    {
        private Vector2 _cardScroll;
        private Vector2 _detailScroll;
        private CardDatabase _database;
        private CardData _selectedCard;
        private UnityEditor.Editor _selectedCardEditor;

        public static void OpenWindow()
        {
            var window = GetWindow<CardDatabaseWindow>("Card Database");
            window.minSize = new Vector2(950f, 500f);
            window.Refresh();
            window.Show();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDisable()
        {
            DestroyImmediate(_selectedCardEditor);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_database == null)
            {
                EditorGUILayout.HelpBox("Create the card database to edit card values in the Unity editor.", MessageType.Info);
                if (GUILayout.Button("Create Card Database From Defaults", GUILayout.Height(30f)))
                {
                    _database = CardDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(true);
                    Refresh();
                }

                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawCardTable();
            DrawDetailsPane();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Refresh();
            }

            if (GUILayout.Button("Create Missing Assets", EditorStyles.toolbarButton))
            {
                CardDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(false);
                Refresh();
            }

            if (GUILayout.Button("Reset Defaults", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Reset Card Database", "Overwrite card values with the hard-coded defaults?", "Reset", "Cancel"))
                {
                    CardDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(true);
                    Refresh();
                }
            }

            GUILayout.FlexibleSpace();
            if (_database != null)
            {
                EditorGUILayout.ObjectField(_database, typeof(CardDatabase), false, GUILayout.Width(250f));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCardTable()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f));
            EditorGUILayout.LabelField("Cards", EditorStyles.boldLabel);
            DrawTableHeader();
            _cardScroll = EditorGUILayout.BeginScrollView(_cardScroll);

            foreach (var card in _database.allCards.Where(card => card != null))
            {
                DrawCardRow(card);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPane()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);

            if (_selectedCard == null)
            {
                EditorGUILayout.HelpBox("Select a card to edit its description and effect list.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.ObjectField("Selected Card", _selectedCard, typeof(CardData), false);
            EditorGUILayout.Space();

            if (_selectedCardEditor == null || _selectedCardEditor.target != _selectedCard)
            {
                DestroyImmediate(_selectedCardEditor);
                _selectedCardEditor = UnityEditor.Editor.CreateEditor(_selectedCard);
            }

            EditorGUI.BeginChangeCheck();
            _selectedCardEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _selectedCard.name = _selectedCard.cardName;
                EditorUtility.SetDirty(_selectedCard);
                AssetDatabase.SaveAssets();
                Repaint();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Card", EditorStyles.miniBoldLabel, GUILayout.Width(180f));
            GUILayout.Label("Cost", EditorStyles.miniBoldLabel, GUILayout.Width(42f));
            GUILayout.Label("Duration", EditorStyles.miniBoldLabel, GUILayout.Width(85f));
            GUILayout.Label("All", EditorStyles.miniBoldLabel, GUILayout.Width(30f));
            GUILayout.Label("Starter", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
            GUILayout.Label("Effects", EditorStyles.miniBoldLabel, GUILayout.Width(45f));
            GUILayout.Label("Description", EditorStyles.miniBoldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCardRow(CardData card)
        {
            var starterEntry = GetOrCreateStarterEntry(card);
            var isSelected = _selectedCard == card;
            var background = isSelected ? new Color(0.24f, 0.33f, 0.45f, 0.35f) : Color.clear;
            var rowRect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rowRect, background);

            EditorGUI.BeginChangeCheck();
            var buttonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
            if (GUILayout.Button(card.cardName, buttonStyle, GUILayout.Width(180f)))
                SelectCard(card);

            var energyCost = EditorGUILayout.IntField(card.energyCost, GUILayout.Width(42f));
            var duration = (ECardDuration)EditorGUILayout.EnumPopup(card.duration, GUILayout.Width(85f));
            var targetsAllEnemies = EditorGUILayout.Toggle(card.targetsAllEnemies, GUILayout.Width(30f));
            var copies = Mathf.Max(0, EditorGUILayout.IntField(starterEntry.copies, GUILayout.Width(50f)));
            GUILayout.Label(card.effects?.Count.ToString() ?? "0", GUILayout.Width(45f));
            var description = EditorGUILayout.TextField(card.description ?? string.Empty);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(card, "Edit Card Row");
                Undo.RecordObject(_database, "Edit Starter Deck Copies");
                card.energyCost = Mathf.Clamp(energyCost, 0, 5);
                card.duration = duration;
                card.targetsAllEnemies = targetsAllEnemies;
                card.description = description;
                starterEntry.copies = copies;
                card.name = card.cardName;
                EditorUtility.SetDirty(card);
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Ping", GUILayout.Width(42f)))
            {
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);
            }

            EditorGUILayout.EndHorizontal();
        }

        private CardDeckEntry GetOrCreateStarterEntry(CardData card)
        {
            var entry = _database.starterDeck.FirstOrDefault(item => item != null && item.card == card);
            if (entry != null)
                return entry;

            Undo.RecordObject(_database, "Add Starter Deck Entry");
            entry = new CardDeckEntry { card = card, copies = 0 };
            _database.starterDeck.Add(entry);
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            return entry;
        }

        private void SelectCard(CardData card)
        {
            _selectedCard = card;
            _detailScroll = Vector2.zero;
            Repaint();
        }

        private void Refresh()
        {
            _database = CardDatabaseEditorUtility.LoadDatabase();
            if (_database == null)
            {
                _selectedCard = null;
                DestroyImmediate(_selectedCardEditor);
                _selectedCardEditor = null;
                return;
            }

            _database.allCards = _database.allCards?
                .Where(card => card != null)
                .OrderBy(card => card.cardName)
                .ToList() ?? new List<CardData>();

            if (_selectedCard == null || !_database.allCards.Contains(_selectedCard))
                _selectedCard = _database.allCards.FirstOrDefault();

            if (_selectedCardEditor != null && (_selectedCard == null || _selectedCardEditor.target != _selectedCard))
            {
                DestroyImmediate(_selectedCardEditor);
                _selectedCardEditor = null;
            }
        }
    }
}
