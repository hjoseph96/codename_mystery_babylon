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

namespace TenPN.DecisionFlex.Demos
{
    /**
       \brief
       for the iPerson demo. Pushes what we know about the world into DFlex's database.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Demos/iPerson/Attribute ContextFactory")]
    public class AttributeContextFactory : SingleContextFactory
    {
        // return all person attributes
        public override IContext SingleContext(Logging loggingSetting)
        {
            var context = new ContextDictionary();
            
            float currentFitness = person.GetAttribute(Attribute.Fitness);
            context.SetContext(Attribute.Fitness.ToString(), currentFitness);

            float currentHunger = person.GetAttribute(Attribute.Hunger);
            context.SetContext(Attribute.Hunger.ToString(), currentHunger);

            float currentThirst = person.GetAttribute(Attribute.Thirst);
            context.SetContext(Attribute.Thirst.ToString(), currentThirst);

            float currentWealth = person.GetAttribute(Attribute.Wealth);
            context.SetContext(Attribute.Wealth.ToString(), currentWealth);
            
            return context;
        }

        //////////////////////////////////////////////////

        private iPerson person;

        //////////////////////////////////////////////////

        private void Awake()
        {
            person = GetComponentInParent<iPerson>();
        }
    }
}
