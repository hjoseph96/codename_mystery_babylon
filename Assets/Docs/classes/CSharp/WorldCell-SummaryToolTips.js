﻿NDSummary.OnToolTipsLoaded("CSharpClass:WorldCell",{1202:"<div class=\"NDToolTip TClass LCSharp\"><div class=\"NDClassPrototype\" id=\"NDClassPrototype1202\"><div class=\"CPEntry TClass Current\"><div class=\"CPPrePrototypeLine\"><span class=\"SHMetadata\">[Serializable]</span></div><div class=\"CPModifiers\"><span class=\"SHKeyword\">public</span></div><div class=\"CPName\">WorldCell</div></div></div></div>",1204:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype1204\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> Vector2Int Position { <span class=\"SHKeyword\">get</span> }</div></div></div>",1205:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype1205\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public int</span> Height { <span class=\"SHKeyword\">get</span>; <span class=\"SHKeyword\">set</span> }</div></div></div>",1206:"<div class=\"NDToolTip TProperty LCSharp\"><div id=\"NDPrototype1206\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">public</span> Unit Unit { <span class=\"SHKeyword\">get</span>; <span class=\"SHKeyword\">set</span> }</div></div></div>",1208:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype1208\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private readonly</span> Dictionary&lt;<span class=\"SHKeyword\">int</span>, WorldCellTile&gt; _tilesByLayer</div></div></div>",1209:"<div class=\"NDToolTip TVariable LCSharp\"><div id=\"NDPrototype1209\" class=\"NDPrototype\"><div class=\"PSection PPlainSection\"><span class=\"SHKeyword\">private readonly</span> Dictionary&lt;<span class=\"SHKeyword\">int</span>, WorldCellTile&gt; _overrideTilesByLayer</div></div></div>",1211:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1211\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> WorldCell(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Vector2Int&nbsp;</td><td class=\"PName last\">position,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">height</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1212:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1212\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> AddTile(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">TileConfiguration&nbsp;</td><td class=\"PName last\">config,</td></tr><tr><td class=\"PType first\">Vector3&nbsp;</td><td class=\"PName last\">scale</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1213:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1213\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> OverrideTile(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">WorldCellTile&nbsp;</td><td class=\"PName last\">tile</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1214:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1214\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public void</span> ClearOverride(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1215:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1215\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> WorldCellTile TileAtSortingLayer(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1216:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1216\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> StairsOrientation GetStairsOrientation(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1217:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1217\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> StairsOrientation GetStairsOrientation(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1218:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1218\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> SurfaceType GetSurfaceType(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1219:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1219\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public</span> SurfaceType GetSurfaceType(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1220:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1220\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public string</span> GetTerrainName(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1221:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1221\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public string</span> GetTerrainName(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1222:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1222\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> IsStairs(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1223:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1223\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> IsStairs(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1224:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1224\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> HasLineOfSight(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1225:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1225\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> HasLineOfSight(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1226:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1226\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> IsPassable(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1227:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1227\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> IsPassable(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1228:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1228\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanExit(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Direction&nbsp;</td><td class=\"PName last\">direction,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1229:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1229\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanExit(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Direction&nbsp;</td><td class=\"PName last\">direction,</td></tr><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1230:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1230\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanEnter(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Direction&nbsp;</td><td class=\"PName last\">direction,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1231:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1231\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanEnter(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Direction&nbsp;</td><td class=\"PName last\">direction,</td></tr><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1232:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1232\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public int</span> GetTravelCost(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1233:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1233\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public int</span> GetTravelCost(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1234:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1234\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public int</span> GetTravelCost(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1235:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1235\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public int</span> GetTravelCost(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1236:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1236\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanMove(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1237:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1237\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">public bool</span> CanMove(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\">Unit&nbsp;</td><td class=\"PName last\">unit</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1238:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1238\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private bool</span> CanMoveCardinalDirection(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">Direction&nbsp;</td><td class=\"PName last\">direction,</td></tr><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>",1239:"<div class=\"NDToolTip TFunction LCSharp\"><div id=\"NDPrototype1239\" class=\"NDPrototype WideForm\"><div class=\"PSection PParameterSection CStyle\"><table><tr><td class=\"PBeforeParameters\"><span class=\"SHKeyword\">private bool</span> CanMoveDiagonalDirection(</td><td class=\"PParametersParentCell\"><table class=\"PParameters\"><tr><td class=\"PType first\">WorldCell&nbsp;</td><td class=\"PName last\">destination,</td></tr><tr><td class=\"PType first\"><span class=\"SHKeyword\">int</span>&nbsp;</td><td class=\"PName last\">sortingLayerId,</td></tr><tr><td class=\"PType first\">UnitType&nbsp;</td><td class=\"PName last\">unitType</td></tr></table></td><td class=\"PAfterParameters\">)</td></tr></table></div></div></div>"});