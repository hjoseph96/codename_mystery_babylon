﻿NDSummary.OnToolTipsLoaded("File:Core/Map/Tiles/AnimatedTiles/AnimatedDoor.cs",{725:"<div class=\"NDToolTip TClass LCSharp\"><div class=\"NDClassPrototype\" id=\"NDClassPrototype725\"><div class=\"CPEntry TClass Current\"><div class=\"CPModifiers\"><span class=\"SHKeyword\">public</span></div><div class=\"CPName\">AnimatedDoor</div></div></div></div>",727:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype727\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[InfoBox(&quot;AnimatedDoor requires AnimatedTileGroups named \'Open\' and \'Closed\' to update TileConfigurations based on animation state. If this door is not used in battle, it is optional.&quot;, InfoMessageType.Warning)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[InfoBox(&quot;If this AnimatedDoor open based on user input -- you must have a BoxCollider2D with isTrigger on this GameObject.&quot;, InfoMessageType.Warning)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[InfoBox(&quot;The \'Close\' &amp; \'Open\' animations on this door MUST call the AnimationEvents in this script!&quot;, InfoMessageType.Warning)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Upon Start Settings&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private bool</span> _startClosed</div></div></div>",728:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype728\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private bool</span> _animateOnStart</div></div></div>",729:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype729\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Collider Settings&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> ColliderGroupSimple _colliderGroup</div></div></div>",730:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype730\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Sorting Layer Changes&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private bool</span> _changeSortingLayerUponOpening</div></div></div>",731:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype731\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField, ShowIf(&quot;_changeSortingLayerUponOpening&quot;), SortingLayer]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private string</span> _openSortingLayer</div></div></div>",732:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype732\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private bool</span> _changeSortingLayerUponClosing</div></div></div>",733:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype733\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField, ShowIf(&quot;_changeSortingLayerUponClosing&quot;), SortingLayer]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private string</span> _closedSortingLayer</div></div></div>",734:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype734\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Sound&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField, SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private string</span> _openSound</div></div></div>",735:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype735\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField, SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private string</span> _closeSound</div></div></div>",737:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype737\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public bool</span> IsOpen { <span class=\"SHKeyword\">get</span>; <span class=\"SHKeyword\">private set</span> }</div></div></div>",739:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype739\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private bool</span> _isAnimating</div></div></div>",740:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype740\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> ActionNoticeManager _noticeManager</div></div></div>",742:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype742\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> Start()</div></div></div>",743:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype743\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> Update()</div></div></div>",744:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype744\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> SetOpenImmediate()</div></div></div>",745:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype745\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> Open(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">bool</span>&nbsp;</td><td class=\"PName\">instant</td><td class=\"PDefaultValueSeparator\">&nbsp;=&nbsp;</td><td class=\"PDefaultValue last\"> <span class=\"SHKeyword\">false</span></td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",746:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype746\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> Close(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">bool</span>&nbsp;</td><td class=\"PName\">instant</td><td class=\"PDefaultValueSeparator\">&nbsp;=&nbsp;</td><td class=\"PDefaultValue last\"> <span class=\"SHKeyword\">false</span></td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",747:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype747\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> OpenAnimationEvent()</div></div></div>",748:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype748\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> CloseAnimationEvent()</div></div></div>"});