//----------------------------------------------
// Flip Web Apps: Beautiful Transitions
// Copyright © 2016 Flip Web Apps / Mark Hewitt
//
// Please direct any bugs/comments/suggestions to http://www.flipwebapps.com
// 
// The copyright owner grants to the end user a non-exclusive, worldwide, and perpetual license to this Asset
// to integrate only as incorporated and embedded components of electronic games and interactive media and 
// distribute such electronic game and interactive media. End user may modify Assets. End user may otherwise 
// not reproduce, distribute, sublicense, rent, lease or lend the Assets. It is emphasized that the end 
// user shall not be entitled to distribute or transfer in any way (including, without, limitation by way of 
// sublicense) the Assets in any other way than as integrated components of electronic games and interactive media. 

// The above copyright notice and this permission notice must not be removed from any files.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

using BeautifulTransitions.Scripts.Transitions.Components.GameObject.AbstractClasses;
using BeautifulTransitions.Scripts.Transitions.TransitionSteps;
using BeautifulTransitions.Scripts.Transitions.TransitionSteps.AbstractClasses;
using UnityEngine;

namespace BeautifulTransitions.Scripts.Transitions.Components.GameObject
{
    [AddComponentMenu("Beautiful Transitions/GameObject + UI/Wipe Transition")]
    [HelpURL("http://www.flipwebapps.com/beautiful-transitions/")]
    public class TransitionWipe : TransitionGameObjectBase
    {
        [Header("Wipe Specific")]
        public InSettings WipeInConfig;
        public OutSettings WipeOutConfig;

        float _originalAmount;

        #region TransitionBase Overrides

        /// <summary>
        /// Gather any initial state - See TrtansitionBase for further details
        /// </summary>
        public override void SetupInitialState()
        {
            _originalAmount = ((TransitionSteps.Wipe)CreateTransitionStep()).OriginalValue;
        }

        /// <summary>
        /// Get an instance of the current transition item
        /// </summary>
        /// <returns></returns>
        public override TransitionStep CreateTransitionStep()
        {
            return new TransitionSteps.Wipe(Target);
        }

        /// <summary>
        /// Add common values to the transitionStep for the in transition
        /// </summary>
        /// <param name="transitionStep"></param>
        public override void SetupTransitionStepIn(TransitionStep transitionStep)
        {
            var transitionStepWipe = transitionStep as TransitionSteps.Wipe;
            if (transitionStepWipe != null)
            {
                transitionStepWipe.StartValue = WipeInConfig.StartAmount;
                transitionStepWipe.EndValue = _originalAmount;
            }
            base.SetupTransitionStepIn(transitionStep);
        }

        /// <summary>
        /// Add common values to the transitionStep for the out transition
        /// </summary>
        /// <param name="transitionStep"></param>
        public override void SetupTransitionStepOut(TransitionStep transitionStep)
        {
            var transitionStepWipe = transitionStep as TransitionSteps.Wipe;
            if (transitionStepWipe != null)
            {
                transitionStepWipe.StartValue = transitionStepWipe.GetCurrent();
                transitionStepWipe.EndValue = WipeOutConfig.EndAmount;
            }
            base.SetupTransitionStepOut(transitionStep);
        }

        #endregion TransitionBase Overrides

        #region Transition specific settings
        [System.Serializable]
        public class InSettings
        {
            [Tooltip("Normalised wipe amount at the start of the transition (ends at the GameObjects initial transparency).")]
            public float StartAmount = 0;
        }

        [System.Serializable]
        public class OutSettings
        {
            [Tooltip("Normalised wipe amount at the end of the transition (starts at the GameObjects current transparency).")]
            public float EndAmount = 0;
        }
        #endregion Transition specific settings
    }
}
