using UnityEngine;
using System.Collections;

namespace SS.Cache
{
    public class CacheKey : MonoBehaviour
    {
        [ReadOnly] [SerializeField] int m_Id;

        public int id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        void Start()
        {
            CacheManager.Add(gameObject, id);
        }

        void OnDestroy()
        {
            CacheManager.Remove(gameObject, id);
        }
    }
}