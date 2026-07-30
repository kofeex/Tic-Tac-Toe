using System.Collections;
using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// Works around a TMP_Dropdown bug seen on this project's Screen Space - Camera canvas: on a
    /// non-1:1 CanvasScaler scale factor, <c>Show()</c> sometimes shrinks the open list to fit
    /// the screen using a miscalculated budget, and leaves the list's rows positioned for the
    /// height they were built at rather than the shrunk one — so the first and last rows spill
    /// past the Viewport's mask by the same amount either side, and the visible window itself
    /// ends up shorter than intended even before that.
    /// Lives on the dropdown's Template, so it is cloned into the "Dropdown List" GameObject
    /// every time <c>Show()</c> builds one, and re-applies this popup's own sizing a frame after
    /// TMP's pass has finished fighting over it: the visible window capped at
    /// <see cref="ThemePopup.MaxVisibleOptions"/> rows exactly as <c>ThemePopup.SizeTemplate</c>
    /// set it going in, and Content sized for every row and pinned to open scrolled to the top —
    /// TMP's own clamp, whatever it decided, is simply overwritten rather than reasoned about.
    /// </summary>
    public sealed class DropdownListFix : MonoBehaviour
    {
        private void OnEnable()
        {
            StartCoroutine(FixNextFrame());
        }

        private IEnumerator FixNextFrame()
        {
            // TMP's own screen-fit pass runs as part of Show() but is not necessarily finished
            // the instant this clone is activated; waiting a frame lets it finish first so this
            // fix is the last thing to touch the sizes rather than the first.
            yield return null;

            var template = (RectTransform)transform;
            Transform viewport = template.Find("Viewport");
            Transform content = viewport == null ? null : viewport.Find("Content");
            if (content == null || content.childCount == 0)
            {
                yield break;
            }

            int rowCount = 0;
            float itemHeight = 0f;
            foreach (Transform row in content)
            {
                if (!row.gameObject.activeSelf)
                {
                    continue;
                }

                itemHeight = ((RectTransform)row).rect.height;
                rowCount++;
            }

            if (rowCount == 0 || itemHeight <= 0f)
            {
                yield break;
            }

            template.sizeDelta = new Vector2(template.sizeDelta.x, itemHeight * Mathf.Min(rowCount, ThemePopup.MaxVisibleOptions));

            float contentHeight = itemHeight * rowCount;
            var contentRect = (RectTransform)content;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0f);

            // Resizing Content alone is not enough: each row's own anchoredPosition was set by
            // TMP for whichever (wrong) height it had in mind, not this one, so the rows stay
            // stuck at the old spots — spilling out of Content's newly-correct bounds exactly as
            // before. Rows anchor to Content's bottom edge, so re-stack them top-down from
            // Content's real height instead of trusting whatever TMP already wrote there.
            int visualIndex = 0;
            foreach (Transform row in content)
            {
                if (!row.gameObject.activeSelf)
                {
                    continue;
                }

                var rowRect = (RectTransform)row;
                float centerY = contentHeight - (visualIndex + 0.5f) * itemHeight;
                rowRect.anchoredPosition = new Vector2(rowRect.anchoredPosition.x, centerY);
                visualIndex++;
            }
        }
    }
}
