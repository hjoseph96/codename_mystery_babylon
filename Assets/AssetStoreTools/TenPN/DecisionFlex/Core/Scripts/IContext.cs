// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       All the information needed to evaluate action considerations.

       \details
       You are responsible for making this, in your implementation of ContextFactory. Passed to every Consideration in the decision in turn. 

       \ref ContextDictionary, FastContextDictionary and HierarchicalContext exists as possible implementations
    */
    public interface IContext
    {
        /** T is type of value we expect for this context */
        bool HasContext<T>(object key);

        /** behaviour if HasContext(key) is false is undefined */
        T GetContext<T>(object key); 

#if UNITY_EDITOR
        /** helper function only available in Editor */
        IEnumerable<object> AllKeys();
#endif
    }
}
