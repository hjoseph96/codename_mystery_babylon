# Tutorial {#tutorial}

DecisionFlex is at its best when there are many reasons different reasons for taking an action. Let's build one to run the life of a Sims-like sim person, who has to manage attributes like thirst and hunger by choosing between multiple actions like drink and eat.

We could do this with a big if block - "If hunger is low, eat. Else if thirst is low, drink..." However this approach:

* Can easily become unwieldy as you add more attributes and actions
* looks robotic
* is difficult to tweak at runtime
* can't capture subtleties. Imagine if our iPerson is only a little hungry and moderately thirsty, but is right next to a snack and far from the water fountain. What should they do? With DecisionFlex, that is resolved for you, along with all the other edge cases you didn't think of.

> Open the TenPN/DecisionFlex/Demos/iPerson/DecisionFlex_iPersonTutorial scene

When you press play, it should look like this:

![Initial scene](tutorial-initial.png "Initial scene")

Our sim person has four attributes: Fitness, Hunger, Thirst and Wealth. Wealth and Fitness should be high, and Hunger and Thirst should be low. The simulation already works - if you unpause the game in the top-left of the game window, you will see the attributes slowly change over time.

However, the sim person can't currently do anything! We need to do is integrate DecisionFlex so she has an idea of when to do what.

DecisionFlex uses the following definitions:
- an **Action** is discrete. DecisionFlex chooses between multiple actions to perform. There's normally one GameObject per action. In our sim person demo, the actions will be Eat, Drink, Work, Exercise and Rest.
- a **Decision** is the act of choosing between multiple actions. It's also the hierarchy containing the action gameobjects.
- a **Context** is the world as DecisionFlex sees it. It is a bunch of variables associated with current values. Our sim person's context will contain current Hunger, Thirst, Wealth and Fitness.
- a **Consideration** is a single reason for choosing one action. For instance, the Work action might have a HowPoor consideration that says it's important to work when Wealth is low. There's normally one gameobject per consideration, under the respective action.

## The Decision Object

Let's start to set up DecisionFlex:

> Add a new gameobject under iPerson called Decision (the name isn't important)

> Add a DecisionFlex (Component->TenPN->DecisionFlex->Decision Flex) script to the Decision object

This is the script that will do all the heavy lifting. In the inspector, you'll see multiple warnings. DecisionFlex is highly flexible about how it updates and treats output, and you tell it how you want it to behave by adding some helper scripts to the decision object. There are many built-in helper scripts, but you can also write your own.

> Add a MakeDecisionsAtRegularIntervals script (Component->TenPN->DecisionFlex->Decision Tickers->Regular DecisionTicker") to the decision object. Set Tick Every to 1.

This tells DecisionFlex that it should update once a second. You could also use MakeDecisionEveryFrame or TimesliceDecisionMaking. The latter is useful when budgeting for many DecisionFlex instances, as it limits X to updating a frame. You can also write your own by deriving from \ref TenPN.DecisionFlex.DecisionTicker. 

> Add a SelectWeightedRandom script (Component->TenPN->DecisionFlex->Selectors->Select Weighted Random ActionSelector) to the decision object

This tells DecisionFlex that you want the sim person to make mistakes sometimes. It might be best to drink right now, but very occasionally, just to seem human, they should eat instead. DecisionFlex will make sure that the choice is never stupid, but it won't always be absolutely the best thing. We are making decisions often enough that the odd slightly-wrong choice won't matter that much.

You could also have used SelectHighestScore, which would pick the best thing every single time. This is best if your decisions are made very rarely, or not very tolerant of mistakes.

The decision object should now look like this:

![Decision object in-progress](decision-object-mid.png)

The only thing DecisionFlex is still complaining about is a ContextFactory. This is how your game tells DecisionFlex about the world. It provides one or more IContexts on request, which associates strings with values. For our sim person, that's the values for Thirst, Hunger, Wealth and Fitness. We've provided a script to do that for you this time, but your game will probably need a custom script.

> Add AttributeContextFactory (Component->TenPN->DecisionFlex->Demos->iPerson->Attribute ContextFactory) to the decision object.

Every game will need a custom ContextFactory, but if you look at the script, it's exceedingly simple. 

## Implementing Actions

Now we've finished the decision object, we need to create some actions. 

> Add gameobjects under the decision object for our actions - Drink, Eat, Exercise, Rest, Work.

Your hierarchy should look like this:

    iPerson
    |+ Attributes
    |- Decision
       |+ Drink
       |+ Eat
       |+ Exercise
       |+ Rest
       |+ Work

Each action object will have a script that implements \ref TenPN.DecisionFlex.IAction to actually do the work. If you're integrating DecisionFlex with an existing game, you might want to add the interface to your existing code. For new projects, you can also derive from \ref TenPN.DecisionFlex.Action which implements MonoBehaviour and IAction together.

The tutorial provides ModifyAttribute (Component->TenPN->DecisionFlex->Demos->iPerson->ModifyAttribute) for you, to change the attributes of the sim person.

> Add ModifyAttribute to the Drink gameobject. In the inspector for the new ModifyAttribute, click the Target dropdown, and select Thirst. Then set Boost Value to -0.8.

This says that drinking drops our thirst by 80%.

> Add ModifyAttribute to Exercise; set Boost Value to 0.4 and Target to Fitness.

> Add another ModifyAttribute to Work; set Boost to 0.04 and Target to Wealth.

> Add *two* ModifyAttribute scripts to Eat. Set one to Boost Hunger by -0.8, and the other to Boost Fitness by -0.2.

When DecisionFlex chooses an action, it executes all IAction scripts on that gameobject. So eating will make our sim person unfit, while at the same time reducing her hunger.

> Add an IdleAction script to Rest (Component->TenPN->DecisionFlex->IdleAction).

IdleAction is a dummy script provided by DecisionFlex for actions that should have no effect on the world. This is a fallback action, for when our sim person has found nothing particularly interesting to do.

## Considerations

The actions are complete, but DecisionFlex still doesn't know how to choose between them. You can run the scene now, and our person will behave seemingly at random. For them to act sensibly, we need to add Considerations to each action. Each consideration is a single reason for taking this action (or a reason not to take this action). 

> Create a gameobject under Drink, and name it HowThirsty (the name isn't important)

By convention I put my considerations under my actions like this, so I can give them nice self-documenting names. DecisionFlex would also find the considerations if you added the individual scripts to the action object.

Each consideration produces a value above or equal to 0 and normally below 1. The higher the value, the more we want to execute this action. 

A \ref TenPN.DecisionFlex.Consideration "consideration" script normally takes one entry from the context and processes it to produce a value. For instance, we really want to take the Drink action when Thirst is high. We added Thirst to the context in our AttributeContextFactory script.

> Add a FloatConsideration (Component->TenPN->DecisionFlex->Considerations->Float Consideration) to HowThirsty. 

> Set the Context Name to Thirst

> Set the Response Curve to something similar to the following

![HowThirsty response curve](tutorial-thirst-response-curve.png)

The x axis is the Thirst, and the y axis is the consideration value from 0 to 1. Coincidentally our Thirst axis runs from 0 to 1 too, but in the other demos, the x axis might be things like distance or ammo counts.

A FloatConsideration script works with a float value from the context. It then uses the animation curve you created to convert Thirst into a score. The shape of the curve tells DecisionFlex how important it is to drink given the Thirst value. 

![HowThirsty response curve](tutorial-thirst-response-ex.png)

Eg a Thirst value of 0.5 would be scored at just over 0.1, because who really cares if you're only a bit thirsty? You'll get to it later. 0.7 would be scored at 0.6, and 0.9 would be scored at 1, because now you really need a drink. 

We only need the one consideration for Drink, but theoretically we can have as many as we want. We'll see multiple considerations used later.

Let's set up Eat. We need to Eat when we're feeling particularly hungry:

> Add a gameobject under Eat called HowHungry

> Add a FloatConsideration to HowHungry, setting Context Name to Hunger and a Response Curve similar to:

![HowHungry response curve](tutorial-hungry-curve.png)

You see we don't really want to eat until we're quite hungry. No snacking!

Exercise is next:

> Add a gameobject under Exercise called HowUnfit

> Add a FloatConsideration to HowUnfit, setting Context Name to Fitness and a response curve similar to:

![HowUnfit response curve](tutorial-fitness-curve.png)

This is a little different to the other curves, as we care about *low* fitness, compared to high thirst or hunger. Notice how much subtlety you can get out of just curves!

Now, exercising is important, but you shouldn't do it if you're close to death from thirst or hunger. Because this curve reaches a max consideration value of 1, you could be quite thirsty and very unfit and look to exercise before drinking. That's dangerous! We could adjust our HowUnfit response curve so it tops out at some value, but it becomes awkward if we want to tune that in the future. Instead, we can use a built-in consideration script called ConsiderationScalar. 

> Add another gameobject under Exercise called Priority

> Add a ConsiderationScalar to Priority, and set the scalar to 0.5

A ConsiderationScalar's value is always constant, and isn't influenced by the context. When an action has more than one consideration, their values are multiplied together. Hence a value of 1 from HowUnfit is multiplied by the 0.5 Priority to be capped to 0.5. A 0.1 value from HowUnfit would result in an eventual 0.05 score for Exercise. This is a much more maintainable way to set up a priority than tweaking the HowUnfit curve by hand.

The Work action is set up similarly to Exercise:

> Add a gameobject called HowPoor to Work

> Add a FloatConsideration to HowPoor, converting Wealth with the following curve:

![HowPoor response curve](tutorial-howpoor-curve.png)

> Add another gameobject to Work called Priority

> Give Priority a ConsiderationScalar of 0.75

In our hierarchy of iPerson needs, eating and drinking (no scalars) are more important than working (scalar of 0.75), which is more important than exercise (scalar of 0.5). 

The Idle priority is our last resort, if everything else scores low. It doesn't need a response curve, just a low priority value to make it stand out above the noise:

> Add a Priority gameobject to Idle

> Give it a ConsiderationScalar of 0.15

## Our First Decisions

And that's it! You should be able to run the scene now and see the iPerson take sensible actions. 

![Sensible decisions](tutorial-in-progress.png)

Let's break down what's happening: 

- Every second, the MakeDecisionsAtRegularIntervals script calls PerformAction() on the DecisionFlex script
- The DecisionFlex script asks the nearest [ContextFactory](\ref TenPN.DecisionFlex.ContextFactory), in this case AttributeContextFactory, for an [IContext](\ref TenPN.DecisionFlex.IContext) object
    - The AttributeContextFactory adds the current Thirst, Hunger, Wealth and Fitness values to the IContext and returns it to DecisionFlex
- DecisionFlex loops through all the Action objects:
    - For one action, DecisionFlex loops through each Consideration, asking for a ConsiderationValue
        - Considerations do their calculations in different ways, but most of ours are FloatConsiderations that push a single attribute value through a response curve
    - All the consideration values are multiplied together to get a score for the action
- DecisionFlex passes all the scores to the nearest [ActionSelector](\ref TenPN.DecisionFlex.ActionSelector), in this case a SelectWeightedRandom
    - SelectWeightedRandom uses the scores as weights and picks randomly. If one action is scored 33% and another is scored 66%, it's twice as likely to pick the 66% action.
- DecisionFlex calls Perform() on all the IAction scripts on the winning action object

What about a concrete example? Let's say the iPerson's attributes are as follows:

Attribute | Value
----------|------
Hunger    | 0.7
Thirst    | 0.4
Fitness   | 0.6
Wealth    | 0.55

From our response curves above, that gives roughly these values for the actions:

Action   | Response Curve      | Priority | Total
---------|---------------------|----------|---------------------
Work     | 0.55 Wealth -> 0.7  | 0.75     | **0.525** (0.6x0.75)
Idle     | -                   | 0.15     | **0.15**
Eat      | 0.7 Hunger -> 0.1   | -        | **0.1**
Exercise | 0.55 Fitness -> 0.1 | 0.5      | **0.05** (0.1x0.5)
Drink    | 0.4 Thirst -> 0     | -        | **0**

Because of random weighting, we can't say for sure what the final outcome will be, but Work looks like a firm favourite. Eat will start to skyrocket as hunger creeps up, because our HowHungry response curve ramps up really fast at the end. Maybe in a few turns our iPerson will eat, which will cause Fitness to drop. Then Exercising will start to rise up the charts. 

## Runtime Debugging

You can tweak all the consideration and action parameters at runtime, and watch how that changes the iPerson behaviour. But remember changes made while the game is running are not saved when you stop running!

At runtime, the inspector for the DecisionFlex will show the last 5 decisions made, and the full context selected with the last decision. Enabling the logging flag found there will output more information to the console. Each action's inspector will show the scores generated by the considerations associated with that action. 

Here are some things to try:

- Make drinking, eating and exercising all cost wealth when executed, by adding a ModifyAttribute script to all of them. What happens?
- How could you make the iPerson more of a health freak?
- Add a new Snacking action that reduces hunger a little and increases fitness a little. Make an iPerson that snacks to keep hunger down, but still has complete meals occasionally.

## Further Resources

This system of "normalising" system variables to easily compare them is called utility theory. Here are more resources on the subject, although you can use DecisionFlex to its fullest without them:

Free GDC lectures from Dave Mark and Kevin Dill:  \n
http://www.gdcvault.com/play/1012410/Improving-AI-Decision-Modeling-Through  \n
http://www.gdcvault.com/play/1015683/Embracing-the-Dark-Art-of

Dave Mark's book on utility theory and modelling behaviours with mathematics:  \n
http://www.amazon.com/dp/1584506849

## The toolkit

DecisionFlex comes with some built-in consideration scripts:

- [BinaryContextConsideration](\ref TenPN.DecisionFlex.BinaryContextConsideration): runs a value from the context through an expression, and returns one of two values depending on success. For instance, if ammo count is greater than zero, the consideration returns one, otherwise it returns zero.

- [ConsiderationScalar](\ref TenPN.DecisionFlex.ConsiderationScalar): returns a constant score whatever context is given to it. Good for setting the priorities of an action.

- [FloatConsideration](\ref TenPN.DecisionFlex.FloatConsideration)/[IntConsideration](\ref TenPN.DecisionFlex.IntConsideration): pushes a value from the context through a response curve as defined by an AnimationCurve, returning the final score.

You can build your own by extending \ref TenPN.DecisionFlex.Consideration. 

When returning an IContext from your ContextFactory, you can use one of the built-in context objects:

- [ContextDictionary](\ref TenPN.DecisionFlex.ContextDictionary) should be your first call. It's easy to make and easy to use. It doesn't allocate memory by default.
- [FastContextDictionary](\ref TenPN.DecisionFlex.FastContextDictionary) does do some allocations, but is marginally faster than ContextDictionary. If profiling shows your ContextFactoryis a hotspot, switching to FastContextDictionary can be an easy win. 
- [HierarchicalContext](\ref TenPN.DecisionFlex.HierarchicalContext) pairs a self and parent IContext. If the self IContext doesn't have a particular key, it checks the parent. Of course there's nothing stopping you nesting multiple hierarchical contexts to create a whole tree. This can be useful to store common calculations between multiple objects, like a squad sharing target information.

Or of course you can derive your own implementation from \ref TenPN.DecisionFlex.IContext.

To schedule the decision, use one of the \ref TenPN.DecisionFlex.DecisionTicker scripts:

- [MakeDecisionAtRegularIntervals](\ref TenPN.DecisionFlex.MakeDecisionAtRegularIntervals) 
- [MakeDecisionEveryFrame](\ref TenPN.DecisionFlex.MakeDecisionEveryFrame) 
- [TimesliceDecisionMaking](\ref TenPN.DecisionFlex.TimesliceDecisionMaking) takes a bucket name and a fixed update limit. Every frame, of all the DecisionFlex objects in the same bucket, only a fixed number will update. The script queues all the objects so everything gets updated eventually. This script is helpful if you will have many decision makers in the scene at once and want to budget CPU usage.

You can also make your own script, and call `PerformAction()` on the DecisionFlex sript or `SendMessage("PerformAction")` to the DecisionFlex gameobject.

[ContextFactory](\ref TenPN.DecisionFlex.ContextFactory) implementers should return an `IList<IContext>` (which can be an array or a List<>). This is useful for when you have multiple situations you could execute actions in, and want to choose the best action/situation pair at any one time. The weapon demo scene returns one IContext per target, and chooses the best weapon/target combo. 

However if your decision does not have multiple contexts, you can instead derive your factory from [SingleContextFactory](\ref TenPN.DecisionFlex.SingleContextFactory) and return a single IContext instead. The iPerson AttributeContextFactory does this. 

## Demos

[Check out a detailed breakdown of the built-in demos here](@ref demos). Run them from the editor and poke around their hierarchies to explore how they're put together.

