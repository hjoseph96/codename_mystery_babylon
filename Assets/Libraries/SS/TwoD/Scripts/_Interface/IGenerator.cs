using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public delegate void Callback();

    public interface IGenerator
    {
        Callback onDone
        {
            get;
            set;
        }
        
        bool enabledComponent
        {
            get;
            set;
        }
        
        string progress
        {
            get;
            set;
        }
        
        Color maskColor
        {
            get;
            set;
        }

        string animationName
        {
            get;
            set;
        }
        
        float animationDuration
        {
            get;
            set;
        }
        
        float animationFrameRate
        {
            get;
            set;
        }
        
        bool animationLoop
        {
            get;
            set;
        }
        
        int animationDirections
        {
            get;
            set;
        }

        void Generate();
    }
}
