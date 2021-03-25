// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
namespace TenPN.DecisionFlex
{
    /** 
        \brief
        Base class for actions DecisionFlex.cs can do.

        \details
        DecisionFlex.cs looks for this interface when enumerating possible actions. See \ref structure for more on how to set this up.

        If you are integrating DecisionFlex into existing code, you can use this to extend your current behaviours. If you are writing new code, you can derive from Action directly.
    */
    public interface IAction
    {
        /** 
            Called once DecisionFlex has decided you can run.

            \param[in] context the IContext in which \ref TenPN.DecisionFlex.DecisionFlex "DecisionFlex" decided to run this action
        */
        void Perform(IContext context);
    }
}
