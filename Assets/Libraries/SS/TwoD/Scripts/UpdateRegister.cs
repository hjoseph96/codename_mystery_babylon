using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class UpdateRegister : MonoBehaviour
    {
        public virtual void UpdateMe()
        {
        }

        public virtual void LateUpdateMe()
        {
        }

        public virtual void FixedUpdateMe()
        {
        }

        protected virtual void OnEnable()
        {
            if (UpdateManager.instance == null)
            {
                new GameObject("Update Manager").AddComponent<UpdateManager>();
            }

            UpdateManager.instance.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (UpdateManager.instance != null)
            {
                UpdateManager.instance.Remove(this);
            }
        }
    }
}