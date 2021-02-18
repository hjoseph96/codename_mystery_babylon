using UnityEngine;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class RenderingOrderRegister : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] m_SpriteRenderers;

        public SpriteRenderer spriteRenderer { get; protected set; }
        public SpriteRenderer[] spriteRenderers { get { return m_SpriteRenderers; } }

        void Awake()
        {
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (m_SpriteRenderers != null && m_SpriteRenderers.Length > 1)
            {
                List<SpriteRenderer> temp = new List<SpriteRenderer>(m_SpriteRenderers);
                temp.Sort(OrderSort);
                m_SpriteRenderers = temp.ToArray();
            }
        }

        int OrderSort(SpriteRenderer a, SpriteRenderer b)
        {
            if (a.sortingOrder > b.sortingOrder)
                return 1;
            else if (a.sortingOrder < b.sortingOrder)
                return -1;
            return 0;
        }

        protected virtual void OnEnable()
        {
            if (RenderingOrderManager.instance == null)
            {
                new GameObject("Rendering Order Manager").AddComponent<RenderingOrderManager>();
            }

            RenderingOrderManager.instance.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (RenderingOrderManager.instance != null)
            {
                RenderingOrderManager.instance.Remove(this);
            }
        }
    }
}