/*
The MIT License (MIT)

Copyright (c) 2014 Andrew Fray

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using UnityEngine;
using System;

namespace TenPN.DecisionFlex.Demos
{
    public enum Attribute {
        Fitness,
        Hunger,
        Thirst,
        Wealth,
    }
    
    /**
       \brief
       The core logic of the simulation, simulating eg Hunger increasing over time

       \details
       An iPerson is a collection of Attributes, which are just floats. Over time they drift up or down. We use DecisionFlex to decide if we should Eat or Work or whatever next, which ends up being just a call to \ref BoostAttribute with the attribute to boost.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Demos/iPerson/iPerson")]
    public class iPerson : MonoBehaviour
    {
        public static readonly Attribute[] Attributes =
            (Attribute[]) Enum.GetValues(typeof(Attribute));

        public float GetAttribute(Attribute att)
        {
            return values[(int)att];
        }

        public void BoostAttribute(Attribute att, float boost)
        {
            var newVal = values[(int)att] + boost;
            values[(int)att] = Mathf.Clamp(newVal, 0f, 1f);
        }
        
        //////////////////////////////////////////////////

        [Serializable]
        struct AttributeParams
        {
            [RangeAttribute(0.0f, 1.0f)]
            public float StartingValue;
            public float ChangePerSecond;
        }

        [SerializeField] AttributeParams fitnessParams;
        [SerializeField] AttributeParams hungerParams;
        [SerializeField] AttributeParams thirstParams;
        [SerializeField] AttributeParams wealthParams;

        float[] values;

        //////////////////////////////////////////////////

        void Awake()
        {
            values = new float[4];
            values[(int)Attribute.Fitness] = fitnessParams.StartingValue;
            values[(int)Attribute.Hunger] = hungerParams.StartingValue;
            values[(int)Attribute.Thirst] = thirstParams.StartingValue;
            values[(int)Attribute.Wealth] = wealthParams.StartingValue;
        }

        void Update()
        {
            UpdateAttribute(Attribute.Fitness, fitnessParams);
            UpdateAttribute(Attribute.Hunger, hungerParams);
            UpdateAttribute(Attribute.Thirst, thirstParams);
            UpdateAttribute(Attribute.Wealth, wealthParams);
        }

        // decay/grow values, make sure don't go above 1/below 0
        void UpdateAttribute(Attribute att, AttributeParams param)
        {
            var newVal = values[(int)att] + param.ChangePerSecond * Time.deltaTime;
            values[(int)att] = Mathf.Clamp(newVal, 0f, 1f);
        }
    }

}
