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

using System;
using System.Linq;
using BeautifulTransitions.Scripts.Transitions.TransitionSteps.AbstractClasses;
using UnityEngine;
using UnityEngine.UI;

namespace BeautifulTransitions.Scripts.Transitions.TransitionSteps
{
    /// <summary>
    /// Transition step for wiping anything that has a material with the ScreenWipe shader (image, mesh, ...)
    /// </summary>
    public class Wipe : TransitionStepFloat {

        Material[] _materials = new Material[0];
        bool _hasComponentReferences;

#region Constructors

        public Wipe(UnityEngine.GameObject target,
            float startAmount = 0,
            float endAmount = 1,
            float delay = 0,
            float duration = 0.5f,
            TransitionModeType transitionMode = TransitionModeType.Specified,
            TimeUpdateMethodType timeUpdateMethod = TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null) :
                base(target, startValue: startAmount, endValue: endAmount, 
                    delay: delay, duration: duration, transitionMode: transitionMode, timeUpdateMethod: timeUpdateMethod, tweenType: tweenType,
                    animationCurve: animationCurve, onStart: onStart,onUpdate: onUpdate, onComplete: onComplete)
        {
        }

#endregion Constructors

#region TransitionStepValue Overrides

        /// <summary>
        /// Get the current transparency level.
        /// </summary>
        /// <returns></returns>
        public override float GetCurrent()
        {
            if (!_hasComponentReferences)
                SetupComponentReferences();

            if (_materials.Length > 0)
                return _materials[0].GetFloat("_Amount");

            return 1;
        }

        /// <summary>
        /// Set the current transparency level
        /// </summary>
        /// <param name="transparency"></param>
        public override void SetCurrent(float value)
        {
            if (!_hasComponentReferences)
                SetupComponentReferences();

            foreach (var material in _materials)
                material.SetFloat("_Amount", value);
        }

        #endregion TransitionStepValue Overrides

        /// <summary>
        /// Get component references
        /// </summary>
        void SetupComponentReferences()
        {
            _materials = new Material[0];
            //#if TEXTMESH_PRO
            //            _tmpuiTexts = new TMPro.TextMeshProUGUI[0];
            //#endif
            //            // get the components to work on target
            //            var canvasGroup = Target.GetComponent<CanvasGroup>();
            //            if (canvasGroup != null)
            //            {
            //                _canvasGroups = _canvasGroups.Concat(Enumerable.Repeat(canvasGroup, 1)).ToArray();
            //            }
            //            else
            //            {
            //var image = Target.GetComponent<Image>();
            //if (image != null)
            //    _images = _images.Concat(Enumerable.Repeat(image, 1)).ToArray();

            //    var rawImage = Target.GetComponent<RawImage>();
            //    if (rawImage != null)
            //        _rawImages = _rawImages.Concat(Enumerable.Repeat(rawImage, 1)).ToArray();

            //    var text = Target.GetComponent<Text>();
            //    if (text != null)
            //        _texts = _texts.Concat(Enumerable.Repeat(text, 1)).ToArray();
            //}

            //var spriteRenderer = Target.GetComponent<SpriteRenderer>();
            //if (spriteRenderer != null)
            //    _spriteRenderers = _spriteRenderers.Concat(Enumerable.Repeat(spriteRenderer, 1)).ToArray();
            var image = Target.GetComponent<Image>();
            if (image != null)
            {
                // TODO: The following line creates a copy of the material otherwise we override the asset - check that we actually mean to do this, and that we don't do it too often. Should we set the old material back when done?
                image.material = new Material(image.material);
                _materials = _materials.Concat(Enumerable.Repeat(image.material, 1)).ToArray();
            }

            var meshRenderer = Target.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material = new Material(meshRenderer.material);
                _materials = _materials.Concat(Enumerable.Repeat(meshRenderer.material, 1)).ToArray();
            }

//#if TEXTMESH_PRO
//            var tmpuiTexts = Target.GetComponent<TMPro.TextMeshProUGUI>();
//            if (tmpuiTexts != null)
//            {
//                _tmpuiTexts = _tmpuiTexts.Concat(Enumerable.Repeat(tmpuiTexts, 1)).ToArray();
//            }
//#endif

            _hasComponentReferences = true;
        }
    }

#region TransitionStep extensions
    public static class WipeExtensions
    {
        /// <summary>
        /// Wipe extension method for TransitionStep
        /// </summary>
        /// <returns></returns>
        public static Wipe Wipe(this TransitionStep transitionStep,
            float startAmount,
            float endAmount,
            float delay = 0,
            float duration = 0.5f,
            TransitionStep.TransitionModeType transitionMode = TransitionStep.TransitionModeType.Specified,
            TransitionStep.TimeUpdateMethodType timeUpdateMethod = TransitionStep.TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            bool runAtStart = false,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null)
        {
            var newTransitionStep = new Wipe(transitionStep.Target,
                startAmount,
                endAmount,
                delay,
                duration,
                transitionMode,
                timeUpdateMethod,
                tweenType,
                animationCurve,
                onStart,
                onUpdate,
                onComplete);
            newTransitionStep.AddToChain(transitionStep, runAtStart);
            return newTransitionStep;
        }

        /// <summary>
        /// Wipe extension method for TransitionStep
        /// </summary>
        /// <returns></returns>
        public static Wipe WipeToOriginal(this TransitionStep transitionStep,
            float startAmount,
            float delay = 0,
            float duration = 0.5f,
            TransitionStep.TimeUpdateMethodType timeUpdateMethod = TransitionStep.TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            bool runAtStart = false,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null)
        {
            var newTransitionStep = transitionStep.Wipe(startAmount,
                0,
                delay,
                duration,
                TransitionStep.TransitionModeType.ToOriginal,
                timeUpdateMethod,
                tweenType,
                animationCurve,
                runAtStart,
                onStart,
                onUpdate,
                onComplete);
            return newTransitionStep;
        }

        /// <summary>
        /// Wipe extension method for TransitionStep
        /// </summary>
        /// <returns></returns>
        public static Wipe WipeToCurrent(this TransitionStep transitionStep,
            float startAmount,
            float delay = 0,
            float duration = 0.5f,
            TransitionStep.TimeUpdateMethodType timeUpdateMethod = TransitionStep.TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            bool runAtStart = false,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null)
        {
            var newTransitionStep = transitionStep.Wipe(startAmount,
                0,
                delay,
                duration,
                TransitionStep.TransitionModeType.ToCurrent,
                timeUpdateMethod,
                tweenType,
                animationCurve,
                runAtStart,
                onStart,
                onUpdate,
                onComplete);
            return newTransitionStep;
        }

        /// <summary>
        /// Wipe extension method for TransitionStep
        /// </summary>
        /// <returns></returns>
        public static Wipe WipeFromOriginal(this TransitionStep transitionStep,
            float endAmount,
            float delay = 0,
            float duration = 0.5f,
            TransitionStep.TimeUpdateMethodType timeUpdateMethod = TransitionStep.TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            bool runAtStart = false,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null)
        {
            var newTransitionStep = transitionStep.Wipe(0,
                endAmount,
                delay,
                duration,
                TransitionStep.TransitionModeType.FromOriginal,
                timeUpdateMethod,
                tweenType,
                animationCurve,
                runAtStart,
                onStart,
                onUpdate,
                onComplete);
            return newTransitionStep;
        }

        /// <summary>
        /// Wipe extension method for TransitionStep
        /// </summary>
        /// <returns></returns>
        public static Wipe WipeFromCurrent(this TransitionStep transitionStep,
            float endAmount,
            float delay = 0,
            float duration = 0.5f,
            TransitionStep.TimeUpdateMethodType timeUpdateMethod = TransitionStep.TimeUpdateMethodType.GameTime,
            TransitionHelper.TweenType tweenType = TransitionHelper.TweenType.linear,
            AnimationCurve animationCurve = null,
            bool runAtStart = false,
            Action<TransitionStep> onStart = null,
            Action<TransitionStep> onUpdate = null,
            Action<TransitionStep> onComplete = null)
        {
            var newTransitionStep = transitionStep.Wipe(0,
                endAmount,
                delay,
                duration,
                TransitionStep.TransitionModeType.FromCurrent,
                timeUpdateMethod,
                tweenType,
                animationCurve,
                runAtStart,
                onStart,
                onUpdate,
                onComplete);
            return newTransitionStep;
        }
    }
#endregion TransitionStep extensions
}
