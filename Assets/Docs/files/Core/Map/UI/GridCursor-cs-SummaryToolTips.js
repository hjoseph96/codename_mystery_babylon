﻿NDSummary.OnToolTipsLoaded("File:Core/Map/UI/GridCursor.cs",{944:"<div class=\"NDToolTip TClass LCSharp\"><div class=\"NDClassPrototype\" id=\"NDClassPrototype944\"><div class=\"CPEntry TClass Current\"><div class=\"CPPrePrototypeLine\"><span class=\"SHMetadata\">[SelectionBase]</span></div><div class=\"CPPrePrototypeLine\"><span class=\"SHMetadata\">[RequireComponent(typeof(CellHighlighter))]</span></div><div class=\"CPModifiers\"><span class=\"SHKeyword\">public</span></div><div class=\"CPName\">GridCursor</div></div></div></div>",946:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype946\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public static</span> GridCursor Instance</div></div></div>",947:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype947\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Cursor State&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> CursorMode _mode</div></div></div>",948:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype948\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Audio&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public string</span> selectedUnitSound</div></div></div>",949:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype949\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public string</span> deselectedUnitSound</div></div></div>",950:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype950\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public string</span> cursorMoveSound</div></div></div>",951:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype951\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SoundGroup]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public string</span> notAllowedSound</div></div></div>",952:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype952\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;Movement settings&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _totalMovementTime</div></div></div>",953:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype953\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField, Range(0, 1)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _normalizedDistanceThreshold</div></div></div>",954:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype954\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> List&lt;ProCamera2DTriggerBoundaries&gt; CameraBoundaries</div></div></div>",955:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype955\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[Header(&quot;References&quot;)]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHMetadata\">[SerializeField]</span></div><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> ArrowPath _arrowPath</div></div></div>",957:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype957\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> Vector2Int GridPosition { <span class=\"SHKeyword\">get</span>; <span class=\"SHKeyword\">private set</span> }</div></div></div>",958:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype958\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public bool</span> IsMoving { <span class=\"SHKeyword\">get</span>; <span class=\"SHKeyword\">private set</span> }</div></div></div>",960:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype960\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Action&lt;Vector2Int&gt; _onPositionChanged</div></div></div>",961:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype961\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Action&lt;Vector2Int&gt; _onCellSelected</div></div></div>",962:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype962\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Action _onExit</div></div></div>",963:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype963\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Vector2Int _targetGridPosition</div></div></div>",964:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype964\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> HashSet&lt;Vector2Int&gt; _allowedPositions</div></div></div>",965:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype965\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> List&lt;Vector2Int&gt; _restrictedPositionsList</div></div></div>",966:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype966\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> WorldGrid _worldGrid</div></div></div>",967:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype967\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> UserInput _userInput</div></div></div>",968:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype968\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private float</span> _movementStartTime</div></div></div>",969:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype969\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Unit _selectedUnit</div></div></div>",970:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype970\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> ActionSelectMenu _actionSelectMenu</div></div></div>",971:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype971\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> Renderer[] _renderers</div></div></div>",972:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype972\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> CellHighlighter _cellHighlighter</div></div></div>",973:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype973\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private</span> ProCamera2D _camera</div></div></div>",975:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype975\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> Init()</div></div></div>",976:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype976\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> Update()</div></div></div>",977:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype977\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> ProcessInput(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">InputData&nbsp;</td><td class=\"PName last\">inputData</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",978:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype978\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> SetFreeMode()</div></div></div>",979:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype979\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> SetRestrictedMode(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",980:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype980\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> SetRestrictedToListMode(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">List&lt;Vector2Int&gt;&nbsp;</td><td class=\"PName last\">cells,</td></tr><tr><td class=\"PType first\">Action&lt;Vector2Int&gt;&nbsp;</td><td class=\"PName last\">onMove,</td></tr><tr><td class=\"PType first\">Action&lt;Vector2Int&gt;&nbsp;</td><td class=\"PName last\">onSelect,</td></tr><tr><td class=\"PType first\">Action&nbsp;</td><td class=\"PName last\">onExit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",981:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype981\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> ExitRestrictedMode()</div></div></div>",982:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype982\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> SetLockedMode()</div></div></div>",983:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype983\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> Show()</div></div></div>",984:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype984\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> Hide()</div></div></div>",985:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype985\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> ResetUnit(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",986:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype986\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> MoveInstant(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Vector2Int&nbsp;</td><td class=\"PName last\">destination</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",987:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype987\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> MoveSelectedUnit()</div></div></div>",988:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype988\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> ClearAll()</div></div></div>",989:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype989\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public void</span> SetAsCameraTarget()</div></div></div>",990:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype990\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private</span> IEnumerator MoveSelectedUnitCoroutine(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">GridPath&nbsp;</td><td class=\"PName last\">path</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",991:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype991\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> ShowActionSelectMenu()</div></div></div>",992:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype992\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> Move()</div></div></div>",993:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype993\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private void</span> StartMovement(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Vector2Int&nbsp;</td><td class=\"PName last\">destination</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",994:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype994\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private void</span> EndMovement()</div></div></div>",995:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype995\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private bool</span> IsWithinCameraBounds(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Vector3&nbsp;</td><td class=\"PName last\">worldPosition</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>"});