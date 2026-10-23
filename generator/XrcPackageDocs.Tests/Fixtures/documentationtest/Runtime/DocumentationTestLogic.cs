using System.Collections.Generic;
using UnityEngine;

namespace XRC.Toolkit.DocumentationTest
{
    /// <summary>
    /// The logic of the documentation test package. It covers the cases a components page has to handle: a
    /// <see langword="sealed"/> class, an inherited field (<see cref="DocumentationTestBase.speed"/>), properties named
    /// differently from their fields, and fields documented in different ways.
    /// </summary>
    public sealed class DocumentationTestLogic : DocumentationTestBase
    {
        [SerializeField, Tooltip("The radius of the test area, in meters.")]
        private float m_Radius = 0.5f;
        /// <summary>
        /// The radius of the test area, in meters. The property is named differently from its field
        /// (<c>m_Radius</c>), so the description must be found through the property's getter, not its name.
        /// </summary>
        public float radiusInMeters
        {
            get => m_Radius;
            set
            {
                if (value < 0f)
                {
                    value = 0f;
                }
                m_Radius = value;
            }
        }

        [SerializeField]
        [Tooltip("The transform the test area follows.")]
        private Transform m_Target;
        /// <summary>
        /// The transform the test area follows. When <see langword="null"/>, the area stays where it is.
        /// </summary>
        public Transform target => m_Target;

        [SerializeField]
        [Tooltip("Tags for the objects inside the area.")]
        private List<string> m_Tags = new List<string>();
        /// <summary>
        /// Tags for the objects inside the area. Objects with any of these tags are counted:
        /// <list type="bullet">
        /// <item>an empty list counts every object;</item>
        /// <item>tags are matched exactly.</item>
        /// </list>
        /// </summary>
        public List<string> tags => m_Tags;

        /// <summary>
        /// How many objects the area may hold. This field has no public property, so its own summary describes it.
        /// </summary>
        [SerializeField]
        [Tooltip("How many objects the area may hold.")]
        private int m_Capacity = 8;

        [SerializeField]
        [Tooltip("Whether to log each object that enters the area.")]
        private bool m_LogEntries;

        [SerializeField]
        [HideInInspector]
        private float m_InternalScale = 1f;

        private void Awake()
        {
            if (m_LogEntries)
            {
                Debug.Log($"Documentation test area: radius {m_Radius * m_InternalScale}, capacity {m_Capacity}.");
            }
        }
    }
}
