using UnityEngine;

namespace XRC.Toolkit.DocumentationTest
{
    /// <summary>
    /// Base class for the documentation test components, with a field every derived component inherits.
    /// </summary>
    public abstract class DocumentationTestBase : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("How fast the component works, in units per second.")]
        private float m_Speed = 1f;
        /// <summary>
        /// How fast the component works, in units per second. Inherited by every documentation test component, so its
        /// description has to come from this base class.
        /// </summary>
        public float speed
        {
            get => m_Speed;
            set => m_Speed = value;
        }
    }
}
