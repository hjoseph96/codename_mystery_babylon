// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Manages decision-making so only X objects from same bin make decisions each frame.
       \remarks
       A bin is a group of particular decision makers that should be timescliced. You can have multiple bins to control the update rate of multiple decisions in different ways.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Tickers/Time-Sliced DecisionTicker")]
    public class TimesliceDecisionMaking : DecisionTicker
    {
        public string BinName
        {
            get
            {
                return m_binName;
            }
            set
            {
                RemoveFromBin();
                AddToBin(value);
            }
        }

        //////////////////////////////////////////////////

        /** decisions made per frame. changes after first Start() are not advised. */
        [SerializeField] private int m_decisionsPerFrame = 1;

        /** 
            all decisions with same bin name are timesliced together.
            don't change after start. use BinName code property instead.
        */
        [SerializeField] private string m_binName = "default";

        // a bin is a group of decisions that should be timesliced together.
        private class Bin
        {
            public IList<TimesliceDecisionMaking> Tickers = 
                new List<TimesliceDecisionMaking>();
            public int CurrentIndex = 0;

            public TimesliceDecisionMaking Controller
            {
                get { return m_controller; }
                set
                {
                    if (m_controller != null)
                    {
                        m_controller.m_isController = false;
                    }
                    m_controller = value;
                    if (m_controller != null)
                    {
                        m_controller.m_isController = true;
                    }
                }
            }

            //////////////////////////////////////////////////

            TimesliceDecisionMaking m_controller = null;
        }

        private static readonly Dictionary<string, Bin> s_bins = 
            new Dictionary<string, Bin>();
        bool m_isController = false;

        //////////////////////////////////////////////////

        private void RemoveFromBin()
        {
            var oldBin = s_bins[m_binName];
            oldBin.Tickers.Remove(this);
            if (oldBin.CurrentIndex > 0)
            {
                oldBin.CurrentIndex = oldBin.CurrentIndex % oldBin.Tickers.Count;
            }
            
            if (oldBin.Controller == this)
            {
                // defer control if there's someone left
                if (oldBin.Tickers.Count > 0)
                {
                    oldBin.Controller = oldBin.Tickers[0];
                } 
                else
                {
                    // abandon
                    oldBin.Controller = null;
                }
            }
        }

        private void AddToBin(string newBinName)
        {
            m_binName = newBinName;
            Bin newBin = null;
            if (s_bins.TryGetValue(newBinName, out newBin) == false)
            {
                newBin = new Bin();
                s_bins[newBinName] = newBin;
            }

            newBin.Tickers.Add(this);

            // take control if we have to
            if (newBin.Controller == null)
            {
                newBin.Controller = this;
            }
        }

        private void OnEnable()
        {
            AddToBin(m_binName);
        }

        private void OnDisable()
        {
            RemoveFromBin();
        }

        private void OnDestroy()
        {
            // this is re-entrant, so let's call it again just to check
            RemoveFromBin();
        }

        private void Update()
        {
            if (m_isController)
            {
                MasterUpdate();
            }
        }

        private void MasterUpdate()
        {
            var bin = s_bins[m_binName];
            for(int decisionIndex = 0; 
                // Count could go 0 during loop - see inner comment
                bin.Tickers.Count > 0 && decisionIndex < m_decisionsPerFrame; 
                ++decisionIndex)
            {
                bin.Tickers[bin.CurrentIndex].MakeDecision();
                // Count could have gone 0, if we're the last guy and 
                // we deleted ourselves during the decision.
                bin.CurrentIndex = bin.Tickers.Count == 0 ? 0 
                    : (bin.CurrentIndex + 1) % bin.Tickers.Count;
            }
        }
    }
}
