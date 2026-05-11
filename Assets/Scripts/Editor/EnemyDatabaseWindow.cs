using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BannerOfBones.CardGame.Editor
{
    public class EnemyDatabaseWindow : EditorWindow
    {
        private Vector2 _enemyScroll;
        private Vector2 _detailScroll;
        private EnemyDatabase _database;
        private EnemyData _selectedEnemy;
        private UnityEditor.Editor _selectedEnemyEditor;

        public static void OpenWindow()
        {
            var window = GetWindow<EnemyDatabaseWindow>("Enemy Database");
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
            DestroyImmediate(_selectedEnemyEditor);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_database == null)
            {
                EditorGUILayout.HelpBox("Create the enemy database to edit enemy health and passive abilities in the Unity editor.", MessageType.Info);
                if (GUILayout.Button("Create Enemy Database From Defaults", GUILayout.Height(30f)))
                {
                    _database = EnemyDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(true);
                    Refresh();
                }

                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawEnemyTable();
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
                EnemyDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(false);
                Refresh();
            }

            if (GUILayout.Button("Reset Defaults", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Reset Enemy Database", "Overwrite enemy values with the hard-coded defaults?", "Reset", "Cancel"))
                {
                    EnemyDatabaseEditorUtility.CreateOrUpdateDatabaseFromDefaults(true);
                    Refresh();
                }
            }

            GUILayout.FlexibleSpace();
            if (_database != null)
                EditorGUILayout.ObjectField(_database, typeof(EnemyDatabase), false, GUILayout.Width(250f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEnemyTable()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f));
            EditorGUILayout.LabelField("Enemies", EditorStyles.boldLabel);
            DrawTableHeader();
            _enemyScroll = EditorGUILayout.BeginScrollView(_enemyScroll);

            foreach (var enemy in _database.allEnemies.Where(enemy => enemy != null))
                DrawEnemyRow(enemy);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPane()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);

            if (_selectedEnemy == null)
            {
                EditorGUILayout.HelpBox("Select an enemy to edit intents and passive abilities.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.ObjectField("Selected Enemy", _selectedEnemy, typeof(EnemyData), false);
            EditorGUILayout.Space();

            if (_selectedEnemyEditor == null || _selectedEnemyEditor.target != _selectedEnemy)
            {
                DestroyImmediate(_selectedEnemyEditor);
                _selectedEnemyEditor = UnityEditor.Editor.CreateEditor(_selectedEnemy);
            }

            EditorGUI.BeginChangeCheck();
            _selectedEnemyEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _selectedEnemy.name = _selectedEnemy.enemyName;
                EditorUtility.SetDirty(_selectedEnemy);
                AssetDatabase.SaveAssets();
                Repaint();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enemy", EditorStyles.miniBoldLabel, GUILayout.Width(180f));
            GUILayout.Label("HP", EditorStyles.miniBoldLabel, GUILayout.Width(42f));
            GUILayout.Label("Intents", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
            GUILayout.Label("Passives", EditorStyles.miniBoldLabel, GUILayout.Width(55f));
            GUILayout.Label("Description", EditorStyles.miniBoldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEnemyRow(EnemyData enemy)
        {
            var isSelected = _selectedEnemy == enemy;
            var background = isSelected ? new Color(0.24f, 0.33f, 0.45f, 0.35f) : Color.clear;
            var rowRect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rowRect, background);

            EditorGUI.BeginChangeCheck();
            var buttonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
            if (GUILayout.Button(enemy.enemyName, buttonStyle, GUILayout.Width(180f)))
                SelectEnemy(enemy);

            int maxHealth = EditorGUILayout.IntField(enemy.maxHealth, GUILayout.Width(42f));
            GUILayout.Label((enemy.roundIntents?.Count ?? 0).ToString(), GUILayout.Width(50f));
            GUILayout.Label((enemy.passiveEffects?.Count ?? 0).ToString(), GUILayout.Width(55f));
            var description = EditorGUILayout.TextField(enemy.description ?? string.Empty);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(enemy, "Edit Enemy Row");
                enemy.maxHealth = Mathf.Max(1, maxHealth);
                enemy.description = description;
                enemy.name = enemy.enemyName;
                EditorUtility.SetDirty(enemy);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Ping", GUILayout.Width(42f)))
            {
                Selection.activeObject = enemy;
                EditorGUIUtility.PingObject(enemy);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SelectEnemy(EnemyData enemy)
        {
            _selectedEnemy = enemy;
            _detailScroll = Vector2.zero;
            Repaint();
        }

        private void Refresh()
        {
            _database = EnemyDatabaseEditorUtility.LoadDatabase();
            if (_database == null)
            {
                _selectedEnemy = null;
                DestroyImmediate(_selectedEnemyEditor);
                _selectedEnemyEditor = null;
                return;
            }

            _database.allEnemies = _database.allEnemies?
                .Where(enemy => enemy != null)
                .OrderBy(enemy => enemy.enemyName)
                .ToList() ?? new List<EnemyData>();

            if (_selectedEnemy == null || !_database.allEnemies.Contains(_selectedEnemy))
                _selectedEnemy = _database.allEnemies.FirstOrDefault();

            if (_selectedEnemyEditor != null && (_selectedEnemy == null || _selectedEnemyEditor.target != _selectedEnemy))
            {
                DestroyImmediate(_selectedEnemyEditor);
                _selectedEnemyEditor = null;
            }
        }
    }
}
