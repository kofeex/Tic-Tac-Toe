using System;
using TicTacToe.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.Gameplay
{
    /// <summary>
    /// Builds and updates the visual N×N grid of cell buttons at runtime. Pure view:
    /// it reports clicks through a callback and knows nothing about the game rules.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private const float CellSpacing = 20f;
        private const float MarkFontScale = 0.62f;

        private static readonly Color CellColor = new Color(0.30f, 0.16f, 0.04f, 0.55f);
        private static readonly Color CellHighlightTint = new Color(1.15f, 1.15f, 1.15f);
        private static readonly Color CellPressedTint = new Color(1.30f, 1.30f, 1.30f);
        private static readonly Color XColor = new Color(0.44f, 0.81f, 0.97f);
        private static readonly Color OColor = new Color(0.61f, 0.97f, 0.72f);

        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Sprite _cellSprite;

        private Cell[,] _cells;

        /// <summary>
        /// Destroys any previous cells and builds a fresh <paramref name="size"/>×<paramref name="size"/>
        /// grid, with square cells sized so the whole board fits the grid's rect in either orientation.
        /// </summary>
        /// <param name="size">Number of rows and columns to create.</param>
        /// <param name="cellClicked">Invoked with (row, column) when a cell button is pressed.</param>
        public void Build(int size, Action<int, int> cellClicked)
        {
            Clear();

            Rect gridRect = ((RectTransform)_grid.transform).rect;
            float cellSize = (Mathf.Min(gridRect.width, gridRect.height) - CellSpacing * (size - 1)) / size;

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = size;
            _grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            _grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            _grid.childAlignment = TextAnchor.MiddleCenter;
            _grid.spacing = new Vector2(CellSpacing, CellSpacing);
            _grid.cellSize = new Vector2(cellSize, cellSize);

            _cells = new Cell[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    _cells[row, col] = CreateCell(row, col, cellSize, cellClicked);
                }
            }
        }

        /// <summary>Shows the placed mark in the given cell and locks its button.</summary>
        public void SetMark(int row, int col, Mark mark)
        {
            Cell cell = _cells[row, col];
            cell.IsMarked = true;
            cell.Button.interactable = false;
            cell.Label.text = mark == Mark.X ? "X" : "O";
            cell.Label.color = mark == Mark.X ? XColor : OColor;
            cell.Label.enabled = true;
        }

        /// <summary>Enables or disables the buttons of all empty cells; marked cells stay locked.</summary>
        public void SetBoardInteractable(bool interactable)
        {
            if (_cells == null)
            {
                return;
            }

            foreach (Cell cell in _cells)
            {
                if (!cell.IsMarked)
                {
                    cell.Button.interactable = interactable;
                }
            }
        }

        /// <summary>Destroys all cells. Safe to call in play mode, including right before a rebuild.</summary>
        public void Clear()
        {
            if (_cells == null)
            {
                return;
            }

            foreach (Cell cell in _cells)
            {
                // Detach first so the grid layout forgets the cell immediately;
                // Destroy only takes effect at the end of the frame.
                cell.Button.transform.SetParent(null, false);
                Destroy(cell.Button.gameObject);
            }

            _cells = null;
        }

        private Cell CreateCell(int row, int col, float cellSize, Action<int, int> cellClicked)
        {
            var cellObject = new GameObject($"Cell_{row}_{col}", typeof(RectTransform));
            cellObject.transform.SetParent(_grid.transform, false);

            var background = cellObject.AddComponent<Image>();
            background.sprite = _cellSprite;
            background.type = Image.Type.Sliced;
            background.color = CellColor;

            var button = cellObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;

            // Tints above white brighten the dark cell color on hover/press
            // (the product stays below 1, so nothing clips).
            ColorBlock colors = button.colors;
            colors.highlightedColor = CellHighlightTint;
            colors.pressedColor = CellPressedTint;
            colors.selectedColor = CellHighlightTint;
            button.colors = colors;
            button.onClick.AddListener(() => cellClicked(row, col));

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(cellObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.raycastTarget = false;
            label.fontStyle = FontStyles.Bold;
            label.fontSize = cellSize * MarkFontScale;
            label.alignment = TextAlignmentOptions.Center;
            label.enabled = false;

            return new Cell(button, label);
        }

        /// <summary>The per-cell scene pieces the view needs to update after creation.</summary>
        private sealed class Cell
        {
            public Cell(Button button, TMP_Text label)
            {
                Button = button;
                Label = label;
            }

            public Button Button { get; }

            public TMP_Text Label { get; }

            public bool IsMarked { get; set; }
        }
    }
}
