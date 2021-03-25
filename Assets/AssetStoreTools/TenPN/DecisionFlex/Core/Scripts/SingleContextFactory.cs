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
        Produces a single IContext object, used to evaulate Considerations when making decisions.

        \details
        Populate your IContext object with the data and details that help your Considerations produce scores.
        If you want to return multiple contexts, eg for deciding on best enemy/weapon pairs, use a base ContextFactory instead.
    */
    public abstract class SingleContextFactory : ContextFactory
    {
        /** 
            \returns a list of IContext objects, each of which will be scored against the decision's actions seperately.
            \param loggingSetting if Logging.Enabled, DecisionFlex#m_isLoggingEnabled is true. You should print or display some debugging info.
        */
        public abstract IContext SingleContext(Logging loggingSetting);

        /** 
            \returns the packaged single context produced from SingleContext
        */
        public sealed override IList<IContext> AllContexts(Logging loggingSetting)
        {
            m_soloContainer[0] = SingleContext(loggingSetting);
            return m_soloContainer;
        }

        //////////////////////////////////////////////////

        IContext[] m_soloContainer = new IContext[1];

        //////////////////////////////////////////////////
    }
}
