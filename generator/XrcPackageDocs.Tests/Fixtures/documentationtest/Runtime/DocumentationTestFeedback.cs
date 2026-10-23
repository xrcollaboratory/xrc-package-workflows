using UnityEngine;

namespace XRC.Toolkit.DocumentationTest
{
    /// <summary>
    /// Shows the test area of <see cref="DocumentationTestLogic"/>.
    /// <para>
    /// A second paragraph, to check that paragraphs survive. The area is drawn as a wire sphere with
    /// <see cref="color"/>.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(DocumentationTestLogic))]
    public class DocumentationTestFeedback : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The color of the test area.")]
        private Color m_Color = Color.cyan;
        /// <summary>
        /// The color of the test area. Changing it takes effect the next time the area is drawn.
        /// </summary>
        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        private void OnDrawGizmos()
        {
            var logic = GetComponent<DocumentationTestLogic>();
            Gizmos.color = m_Color;
            Gizmos.DrawWireSphere(transform.position, logic != null ? logic.radiusInMeters : 0.5f);
        }
    }
}
