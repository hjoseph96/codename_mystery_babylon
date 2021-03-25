# DecisionFlex Demos {#demos}

These demos go hand-in-hand with the DecisionFlex library http://www.tenpn.com/decisionflex.html

They are made open-source under the MIT license but they will not function standing alone. They need the DecisionFlex core library too.

The **iPerson** scene shows a single sims-like character. DecisionFlex is helping them choose what their next action should be. The iPerson wants to reduce thirst and hunger, while increasing wealth and fitness. They can do this by drinking, eating, exercising, working or resting.

Every second, the iPerson populates an IContext with the current attribute values, and evaluates the suitability of each action. Once it calculates the scores of each action, DecisionFlex uses them as random weights to make a final decision. This slightly-random behaviour makes things seem a bit more human-like than just picking the best action.

The **WeaponSelection** scene contains a soldier object and a few moving targets. The soldier has multiple weapons that are good at different ranges. DecisionFlex decides what weapon and target the soldier should use at regular intervals. The soldier's WeaponSelection gameobject contains the DecisionFlex as well as Inventory, listing the current ammo reserves of the soldier.

It behaves similarly to the iPerson demo, except the WeaponSelectionContextFactory returns multiple contexts, rather than just one. Each contains information about a single enemy, as well as soldier inventory status. Each weapon action is then scored multiple times, once per context, and the highest enemy/weapon combo is used. 

Notice that the chosen enemy context is passed to the weapon action, so it can correctly shoot.

When playing this scene, you can edit the soldier's ammo counts to see how this adjusts the actions, as well as pause the oscillating enemies. If running this in the editor, you can use the scene view to drag around the enemies and play with different scenarios. 

The **ufo** scene shows how to use DecisionFlex at scale. Here UFOs are spawned to orbit and attack a spaceship. As the spaceship whittles down the UFO health, the UFO may opt to orbit the green health station instead to recharge. If the UFO dies, a new UFO is instantiated.

The UFOs use \ref TenPN.DecisionFlex.TimesliceDecisionMaking "TimesliceDecisionMaking" to manage CPU load. If you have Unity Pro, you can profile the scene and see there's no allocations made during decision. 
