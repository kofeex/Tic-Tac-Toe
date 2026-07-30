using System;
using System.Collections;
using System.Collections.Generic;
using TicTacToe.Core;
using TicTacToe.Themes;
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
        /// <summary>How long the strike line takes to sweep across the winning cells.</summary>
        public const float StrikeDurationSeconds = 0.45f;

        private const float CellSpacing = 20f;
        private const float MarkInsetScale = 0.16f;
        private const float StrikeThicknessScale = 0.16f;
        private const float StrikeExtendScale = 0.32f;

        private static readonly Color CellColor = new Color(0.30f, 0.16f, 0.04f, 0.55f);
        private static readonly Color CellHighlightTint = new Color(1.15f, 1.15f, 1.15f);
        private static readonly Color CellPressedTint = new Color(1.30f, 1.30f, 1.30f);

        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Sprite _cellSprite;
        [SerializeField] private ParticleSystem _placementVfxPrefab;
        [SerializeField] private ParticleSystem _strikeVfxPrefab;

        private Cell[,] _cells;
        private ThemeSelection _themes;
        private GameObject _strike;
        private Coroutine _strikeAnimation;

        /// <summary>
        /// Destroys any previous cells and builds a fresh <paramref name="size"/>×<paramref name="size"/>
        /// grid, with square cells sized so the whole board fits the grid's rect in either orientation.
        /// </summary>
        /// <param name="size">Number of rows and columns to create.</param>
        /// <param name="themes">Both players' themes; each mark is drawn with its own player's artwork.</param>
        /// <param name="cellClicked">Invoked with (row, column) when a cell button is pressed.</param>
        public void Build(int size, ThemeSelection themes, Action<int, int> cellClicked)
        {
            Clear();
            _themes = themes;

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

        /// <summary>
        /// Shows the placed mark in the given cell, locks its button, and spawns the placement
        /// VFX at the mark's world position (no-op if that effect isn't assigned).
        /// </summary>
        public void SetMark(int row, int col, Mark mark)
        {
            Cell cell = _cells[row, col];
            cell.IsMarked = true;
            cell.Button.interactable = false;
            cell.Mark.sprite = _themes == null ? null : _themes.GetSprite(mark);
            cell.Mark.enabled = cell.Mark.sprite != null;
            SpawnVfx(_placementVfxPrefab, cell.Mark.transform.position);
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

        /// <summary>
        /// Sweeps an animated strike line across the winning cells; the line grows from the
        /// first cell of the run towards the last over <see cref="StrikeDurationSeconds"/>.
        /// </summary>
        /// <param name="cells">The winning run in line order, as reported by the board.</param>
        /// <param name="winner">
        /// The mark that won, so the line is drawn in that player's own theme colour.
        /// </param>
        /// <remarks>Also spawns the strike VFX at the midpoint of the winning line (no-op if that effect isn't assigned).</remarks>
        public void ShowStrike(IReadOnlyList<(int Row, int Col)> cells, Mark winner)
        {
            if (cells == null || cells.Count < 2)
            {
                return;
            }

            RectTransform first = (RectTransform)_cells[cells[0].Row, cells[0].Col].Button.transform;
            RectTransform last = (RectTransform)_cells[cells[cells.Count - 1].Row, cells[cells.Count - 1].Col].Button.transform;

            // Cells are centre-pivoted, so their anchored positions are the cell centres.
            Vector2 from = first.anchoredPosition;
            Vector2 to = last.anchoredPosition;
            Vector2 direction = (to - from).normalized;
            float extension = _grid.cellSize.x * StrikeExtendScale;
            float length = (to - from).magnitude + extension * 2f;

            _strike = new GameObject("Strike", typeof(RectTransform));
            _strike.transform.SetParent(_grid.transform, false);
            _strike.AddComponent<LayoutElement>().ignoreLayout = true;

            // Left-middle pivot just before the first cell, rotated towards the last cell,
            // so animating the width sweeps the line along the winning run.
            var rect = (RectTransform)_strike.transform;
            rect.anchorMin = first.anchorMin;
            rect.anchorMax = first.anchorMax;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = from - direction * extension;
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            rect.sizeDelta = new Vector2(0f, _grid.cellSize.x * StrikeThicknessScale);

            var image = _strike.AddComponent<Image>();
            image.sprite = _cellSprite;
            image.type = Image.Type.Sliced;
            image.color = _themes == null ? Color.white : _themes.GetStrikeColor(winner);
            image.raycastTarget = false;

            SpawnVfx(_strikeVfxPrefab, (first.position + last.position) * 0.5f);

            _strikeAnimation = StartCoroutine(GrowStrike(rect, length));
        }

        /// <summary>Destroys all cells and any strike line. Safe to call in play mode, including right before a rebuild.</summary>
        public void Clear()
        {
            if (_strikeAnimation != null)
            {
                StopCoroutine(_strikeAnimation);
                _strikeAnimation = null;
            }

            if (_strike != null)
            {
                _strike.transform.SetParent(null, false);
                Destroy(_strike);
                _strike = null;
            }

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

        private IEnumerator GrowStrike(RectTransform rect, float length)
        {
            float thickness = rect.sizeDelta.y;
            for (float t = 0f; t < 1f; t += Time.deltaTime / StrikeDurationSeconds)
            {
                float eased = 1f - (1f - t) * (1f - t);
                rect.sizeDelta = new Vector2(length * eased, thickness);
                yield return null;
            }

            rect.sizeDelta = new Vector2(length, thickness);
            _strikeAnimation = null;
        }

        /// <summary>
        /// Instantiates a one-shot VFX prefab at a world position and destroys it once its
        /// particles have finished emitting and the longest-lived one has faded out. No-op if
        /// <paramref name="prefab"/> is unassigned, so a missing VFX asset never takes the
        /// gameplay feature down with it.
        /// </summary>
        private void SpawnVfx(ParticleSystem prefab, Vector3 worldPosition)
        {
            if (prefab == null)
            {
                return;
            }

            ParticleSystem instance = Instantiate(prefab, worldPosition, Quaternion.identity);
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
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

            // The mark sits inside the cell with a margin so the artwork never touches the edges.
            var markObject = new GameObject("Mark", typeof(RectTransform));
            markObject.transform.SetParent(cellObject.transform, false);
            var markRect = (RectTransform)markObject.transform;
            markRect.anchorMin = Vector2.zero;
            markRect.anchorMax = Vector2.one;
            float inset = cellSize * MarkInsetScale;
            markRect.offsetMin = new Vector2(inset, inset);
            markRect.offsetMax = new Vector2(-inset, -inset);

            var mark = markObject.AddComponent<Image>();
            mark.raycastTarget = false;
            mark.preserveAspect = true;
            mark.enabled = false;

            return new Cell(button, mark);
        }

        /// <summary>The per-cell scene pieces the view needs to update after creation.</summary>
        private sealed class Cell
        {
            public Cell(Button button, Image mark)
            {
                Button = button;
                Mark = mark;
            }

            public Button Button { get; }

            /// <summary>Displays the themed X or O artwork; disabled while the cell is empty.</summary>
            public Image Mark { get; }

            public bool IsMarked { get; set; }
        }
    }
}
