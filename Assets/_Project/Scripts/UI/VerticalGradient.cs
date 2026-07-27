using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    /// <summary>
    /// Vertical two-color vertex gradient applied to any Graphic.
    /// Multiplies the Graphic's own color, so keep that white for pure gradient colors.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Effects/Vertical Gradient")]
    public sealed class VerticalGradient : BaseMeshEffect
    {
        [SerializeField] private Color _topColor = Color.white;
        [SerializeField] private Color _bottomColor = Color.black;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            var vertex = default(UIVertex);
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                minY = Mathf.Min(minY, vertex.position.y);
                maxY = Mathf.Max(maxY, vertex.position.y);
            }

            float height = Mathf.Max(maxY - minY, 1e-5f);
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                float t = (vertex.position.y - minY) / height;
                vertex.color = (Color)vertex.color * Color.Lerp(_bottomColor, _topColor, t);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}