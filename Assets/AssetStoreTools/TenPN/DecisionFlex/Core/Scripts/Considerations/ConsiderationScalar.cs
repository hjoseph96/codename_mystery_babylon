// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;

namespace TenPN.DecisionFlex
{
    /**
       \brief A consideration that always returns the same score, whatever context is given
       \details
       Useful for capping the priority of an action. 
    */
    [AddComponentMenu("TenPN/DecisionFlex/Considerations/Scalar Consideration")]
    public class ConsiderationScalar : Consideration
    {
        protected override float MakeConsideration(IContext context)
        {
            return m_scalar;
        }

        //////////////////////////////////////////////////

        [SerializeField] private float m_scalar;

        //////////////////////////////////////////////////
    }
}
