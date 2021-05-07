﻿NDSummary.OnToolTipsLoaded("CSharpClass:CarriageController",{833:"<div class=\"NDToolTip TClass LCSharp\"><div class=\"NDClassPrototype\" id=\"NDClassPrototype833\"><div class=\"CPEntry TClass Current\"><div class=\"CPModifiers\"><span class=\"SHKeyword\">public</span></div><div class=\"CPName\">CarriageController</div></div></div></div>",835:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype835\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Basic Properties&quot;), SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _walkSpeed</div></div></div>",836:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype836\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Basic Properties&quot;), SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _runSpeed</div></div></div>",837:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype837\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Basic Properties&quot;), SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _runAnimationSpeed</div></div></div>",838:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype838\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Basic Properties&quot;), SerializeField, HideIf(&quot;IsPlaying&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> CarriageControlMode _ControlMode</div></div></div>",840:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype840\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Basic Properties&quot;), ShowInInspector, ShowIf(&quot;IsPlaying&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> CarriageControlMode ControlMode { <span class=\"SHKeyword\">get</span> }</div></div></div>",842:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype842\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Animations&quot;), SerializeField, ValueDropdown(&quot;ValidDirections&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Direction startingLookDirection</div></div></div>",843:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype843\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;Direction&gt; ValidDirections</div></div></div>",844:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype844\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Dictionary&lt;Direction, Vector2&gt; DirectionToFacing</div></div></div>",845:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype845\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Animations&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> DirectionalAnimationSet _CarriageIdles</div></div></div>",846:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype846\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Animations&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> DirectionalAnimationSet _CarriageMoving</div></div></div>",847:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype847\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;Audio&quot;), SerializeField, SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private string</span> _movingSound</div></div></div>",848:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype848\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;Horse&gt; _horses</div></div></div>",850:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype850\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;References&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[ShowInInspector]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> List&lt;Horse&gt; Horses { <span class=\"SHKeyword\">get</span> }</div></div></div>",852:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype852\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[FoldoutGroup(&quot;References&quot;), SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;GameObject&gt; _horseHolders</div></div></div>",854:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype854\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> List&lt;GameObject&gt; WoodenHorseHolders { <span class=\"SHKeyword\">get</span> }</div></div></div>",856:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype856\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> TimeSynchronisationGroup _MovementSynchronisation</div></div></div>",857:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype857\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> DirectionalAnimationSet _CurrentAnimationSet</div></div></div>",858:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype858\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> AnimancerComponent _animancer</div></div></div>",859:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype859\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Vector2 _Movement</div></div></div>",860:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype860\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Vector2 _Facing</div></div></div>",861:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype861\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;CachedPositions&gt; _spritePositions</div></div></div>",863:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype863\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">void</span> Start()</div></div></div>",864:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype864\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">void</span> Update()</div></div></div>",865:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype865\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> Play(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">DirectionalAnimationSet&nbsp;</td><td class=\"PName\">animations,</td><td></td><td class=\"last\"></td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">float</span>&nbsp;</td><td class=\"PName\">animationSpeed</td><td class=\"PDefaultValueSeparator\">&nbsp;=&nbsp;</td><td class=\"PDefaultValue last\"> <span class=\"SHNumber\">1</span></td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",866:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype866\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> HandlePlayerMovement()</div></div></div>",867:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype867\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> SwapPositionForFacing()</div></div></div>",868:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype868\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> SetHorsesOrderInLayer(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">orderInLayer</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",869:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype869\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> SetHorsesInPosition(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">CachedPositions&nbsp;</td><td class=\"PName last\">cache</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",870:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype870\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> SetHorseBarsInPosition(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">CachedPositions&nbsp;</td><td class=\"PName last\">cache</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",871:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype871\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;CachedPositions&gt; GetCachedPositionsByDirection()</div></div></div>",872:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype872\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> MakeHorsesRun(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">float</span>&nbsp;</td><td class=\"PName last\">animSpeed</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",873:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype873\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> MakeHorsesWalk()</div></div></div>",874:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype874\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> MakeHorsesIdle()</div></div></div>",875:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype875\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> HideHorseHolders()</div></div></div>",876:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype876\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> ShowHorseHolders()</div></div></div>",878:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype878\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public bool</span> IsPlaying { <span class=\"SHKeyword\">get</span> }</div></div></div>"});