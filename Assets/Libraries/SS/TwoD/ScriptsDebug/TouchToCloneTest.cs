using UnityEngine;
using System.Collections;
using SS.Cache;

namespace SS.TwoD
{
    public class TouchToCloneTest : UpdateRegister
    {
        [SerializeField] Camera m_Camera;
        [SerializeField] GameObject m_LeftMouseObject;
        [SerializeField] GameObject m_RightMouseObject;

        public override void UpdateMe()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Create(m_LeftMouseObject);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Create(m_RightMouseObject);
            }
        }

        void Create(GameObject prefab)
        {
            RaycastHit hit; 
            Ray ray = m_Camera.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 2))
            {
                GameObject go = CacheManager.Instantiate(prefab);
                go.transform.position = hit.point;
            }
        }
    }
}