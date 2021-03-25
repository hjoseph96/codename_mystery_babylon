// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       One possible implementation of IContext

       \detailed
       You are responsible for populating this, in your implementation of ContextFactory. It will be passed to every Consideration in the decision in turn. 

       Will not allocate, as long as you only store floats, ints, bools or objects.

       Think of it as a blackboard, or dynamic storage.
    */
    public class ContextDictionary : IContext
    {
        public void SetContext(object key, int value)
        {
            m_intStore[key] = value;
        }

        public void SetContext(object key, float value)
        {
            m_floatStore[key] = value;
        }

        public void SetContext(object key, bool flag)
        {
            m_boolStore[key] = flag;
        }

        public void SetContext<T>(object key, T obj)
        {
            m_defaultStore[key] = obj;
        }

        public bool HasContext<T>(object key)
        {
            if (typeof(T) == typeof(object)) 
            {
                // if asking for object, we have no idea what type it is,
                // so look everywhere.
                return m_intStore.ContainsKey(key)
                    || m_floatStore.ContainsKey(key)
                    || m_boolStore.ContainsKey(key)
                    || m_defaultStore.ContainsKey(key);
            }
            // otherwise look in the correct stores
            if (typeof(T) == typeof(int))
            {
                return m_intStore.ContainsKey(key);
            }
            else if (typeof(T) == typeof(float))
            {
                return m_floatStore.ContainsKey(key);
            }
            else if (typeof(T) == typeof(bool)) 
            {
                return m_boolStore.ContainsKey(key);
            }
            else
            {
                return m_defaultStore.ContainsKey(key);
            }
        }

        /** \throws ApplicationException if this key isn't in the context */
        public T GetContext<T>(object key)
        {
            if (typeof(T) == typeof(object))
            {
                // we just want the contents. we don't care about the type. box it.
                var value 
                    = m_intStore.ContainsKey(key) ? (object)m_intStore[key]
                    : m_floatStore.ContainsKey(key) ? (object)m_floatStore[key]
                    : m_boolStore.ContainsKey(key) ? (object)m_boolStore[key]
                    : m_defaultStore[key];
                return (T)value;
            }

            // avoids boxing by this clever/hacky double-cast
            Dictionary<object, T> store;
            if (typeof(T) == typeof(int))
            {
                store = (Dictionary<object,T>)(object)m_intStore;
            }
            else if (typeof(T) == typeof(float))
            {
                store = (Dictionary<object,T>)(object)m_floatStore;
            }
            else if (typeof(T) == typeof(bool)) 
            {
                store = (Dictionary<object,T>)(object)m_boolStore;
            }
            else
            {
                return (T) m_defaultStore[key];
            }
            return store[key];
        }

#if UNITY_EDITOR
        public IEnumerable<object> AllKeys()
        {
            return m_defaultStore.Keys
                .Concat(m_intStore.Keys)
                .Concat(m_floatStore.Keys)
                .Concat(m_boolStore.Keys)
                .Distinct();
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
        private Dictionary<object, int> m_intStore = new Dictionary<object, int>();
        private Dictionary<object, float> m_floatStore = new Dictionary<object, float>();
        private Dictionary<object, bool> m_boolStore = new Dictionary<object, bool>();

        //////////////////////////////////////////////////
    }
}
