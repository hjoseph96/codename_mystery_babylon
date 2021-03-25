// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using System;
using System.Linq;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /**
       \brief context decorator for a tree of contexts
       \details
       the weapon selection demo uses one context per enemy, with each parented to a master context that contains inventory context.
    */
    public class HierarchicalContext : IContext
    {
        /**
           \param self the first context to check for a key in.
           \param parent the context to defer to if self doesn't contain the key. can be null.
        */
        public HierarchicalContext(IContext self, IContext parent)
        {
            m_self = self;
            m_parent = parent;
        }

        /** true if key in self, or parent */
        public bool HasContext<T>(Object key)
        {
            return m_self.HasContext<T>(key) 
                || (m_parent != null && m_parent.HasContext<T>(key));
        }

        /** fetches from self first, and parent if not found */
        public T GetContext<T>(Object key)
        {
            if (m_self.HasContext<T>(key))
            {
                return m_self.GetContext<T>(key);
            }
            else if (m_parent != null && m_parent.HasContext<T>(key))
            {
                return m_parent.GetContext<T>(key);
            }
            else 
            {
                // replace this with illegal argument exception when we have internet
                throw new ArgumentException(
                    "no context " + key + " of type " + typeof(T) + " is available");
            }
        }

#if UNITY_EDITOR
        /** helper function only available in Editor */
        public IEnumerable<Object> AllKeys()
        {
            var selfKeys = m_self.AllKeys();
            var parentKeys = m_parent == null ? Enumerable.Empty<Object>()
                : m_parent.AllKeys();
            var all = selfKeys.Concat(parentKeys).Distinct();
            return all;
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
        }
#endif

        //////////////////////////////////////////////////

        private IContext m_self;
        private IContext m_parent = null;

        //////////////////////////////////////////////////
    }
}