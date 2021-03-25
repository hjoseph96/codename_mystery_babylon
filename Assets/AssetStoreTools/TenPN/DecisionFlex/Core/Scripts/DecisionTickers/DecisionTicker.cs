// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       base class for behaviours that tell the decision maker to do something

       \details
       Useful to decouple rate of update from the decision maker
    */
    public abstract class DecisionTicker : MonoBehaviour
    {
        /** called by derived classes to trigger the message being sent */
        protected void MakeDecision()
        {
            m_target.PerformAction();
        }

        //////////////////////////////////////////////////

        /** if null, message sent to own gameobject. */
        [SerializeField] private DecisionFlex m_targetIfNotSelf;

        private DecisionFlex m_target;

        //////////////////////////////////////////////////

        private void Awake()
        {
            if (m_targetIfNotSelf != null)
            {
                m_target = m_targetIfNotSelf;
            }
            else
            {
                m_target = GetComponent<DecisionFlex>();

                if (m_target == null)
                {
                    Debug.LogError("No DecisionFlex found on object " + gameObject, 
                                   gameObject);
                }
            }
        }
    }
}
