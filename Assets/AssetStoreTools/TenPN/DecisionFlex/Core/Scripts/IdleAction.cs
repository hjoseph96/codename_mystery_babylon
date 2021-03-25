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
       For when you want nothing to happen. Does Nothing. Nada. Zip.
    */
    [AddComponentMenu("TenPN/DecisionFlex/IdleAction")]
    public class IdleAction : Action
    {
        public override void Perform(IContext context) { }
    }
}
