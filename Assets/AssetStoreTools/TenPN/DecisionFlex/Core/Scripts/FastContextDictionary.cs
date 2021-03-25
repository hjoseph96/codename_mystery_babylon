// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using System;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       One possible implementation of IContext

       \details
       You are responsible for populating this, in your implementation of ContextFactory. Passed to every UtilityConsideration in the decision in turn. 

       faster than regular ContextDictionary, but makes some small allocs for basic-type values. 

       Think of it as a blackboard, or dynamic storage.
    */
    public class FastContextDictionary : IContext
    {
        public void SetContext<T>(object key, T obj)
        {
            m_defaultStore[key] = obj;
        }

        public bool HasContext<T>(object key)
        {
            return m_defaultStore.ContainsKey(key);
        }

        /** \throws ApplicationException if this key isn't in the context */
        public T GetContext<T>(object key)
        {
            return (T) m_defaultStore[key];
        }

#if UNITY_EDITOR
        public IEnumerable<object> AllKeys()
        {
            return m_defaultStore.Keys;
        }
#endif

#if UNITY_IPHONE
        // AOT compiling on iPhone can't cope well with a templated interface definition.
        // doing this forces code to get generated.
        public void ForceAOTCompile()
        {
            GetContext<float>(new object());
            GetContext<int>(new object());
            GetContext<string>(new object());
            GetContext<bool>(new object());
            HasContext<float>(new object());
            HasContext<int>(new object());
            HasContext<bool>(new object());
            HasContext<string>(new object());
            SetContext<float>(new object(), 0f);
            SetContext<int>(new object(), 0);
            SetContext<bool>(new object(), false);
            SetContext<string>(new object(), "");
        }
#endif
        
        //////////////////////////////////////////////////

        private Dictionary<object, Object> m_defaultStore = new Dictionary<object, object>();

        //////////////////////////////////////////////////
    }
}
