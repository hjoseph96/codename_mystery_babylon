*************************************
*             GRIDS 2D              *
*  (C) Copyright 2016-2021 Kronnect * 
*            README FILE            *
*************************************


How to use this asset
---------------------
Firstly, you should run the Demo Scene provided to get an idea of the overall functionality.
Later, you should read the documentation and experiment with the tool.

Hint: to use the asset, drag the Grids2D prefab from Resources/Prefabs folder to your scene.


Demo Scene
----------
There's one demo scene, located in "Demos" folder. Just go there from Unity, open "Demo1" scene and run it.


Documentation/API reference
---------------------------
The PDF is located in the Doc folder. It contains instructions on how to use the prefab and the API so you can control it from your code.


Support
-------
Please read the documentation PDF and browse/play with the demo scene and sample source code included before contacting us for support :-)

* Support: contact@kronnect.com
* Website-Forum: https://kronnect.com/support
* Twitter: @KronnectGames


Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Grids 2D will be eventually available on the Asset Store.


Related assets
--------------

Please visit kronnect.com for related assets.
You may upgrade to Terrain Grid System, which in addition to 2D grids also supports dynamic 3D grids over Unity terrain.



Version history
---------------

Version 6.6 22/01/2020
- API: added CellGetRectWorldSpace / TerritoryGetRectWorldSpace
- Highlight can now be invoked with scripting using HighlightCellRegion()

Version 6.5
- API: change: HideHighlightedObject method now deactivates the underline highlighted cell surface
- API: added GetTerritoryCenter()

Version 6.4
- API: added Sort By Distance option to CellGetNeighbours function
- API: added OnMoveEnd event used with MoveTo() method (see documentation for sample code)

Version 6.3.4
- [Fix] Fixed crash when adding prefab to the scene in Unity 2019

Version 6.3.3
- [Fix] Fixed transparency issues when render queue is set to transparent

Version 6.3.2
- [Fix] Fixed potential error in the inspector when territories and cells are disabled

Version 6.3.1
- Updated Territory by Texture demo scene with blinking effect on territory click

Version 6.3
- Unity 2017.4.7 or later is now required
- Added "Render Queue" and "Sorting Order" options to inspector
- API: added canCrossCheckType parameter to CellGetNeighbours method
- [Fix] Fixed issue with international characters when exporting grid settings

Version 6.2
- [Fix] API: fixed issue with TerritoryGetNeighbour method
- Added compatibility with LWRP

Version 6.1.1
- Minor changes to demo scenes

Version 6.1
- New demo scene 19. Coloring cells by row / column from an input field.

Version 6.0.1
- [Fix] Fixed issue with sides blocking LOS not being detected when ray starts on a blocked cell
- [Fix] CellGetAtPosition(column, row) now accounts for merged cells

Version 6.0
- API: added CellSetSideBlocksLOS / CellGetSideBlocksLOS. Used by CellGetLineOfSight method. Check demo scene 13.
- New useCanvasRect option for CellToggle / TerritoryToggle (draws a region of the texture instead of the full texture inside the cell/territory)
- [Fix] Fixed DrawLine() with box topology
- [Fix] Fixed precision issue with >5000 cells in Voronoi topology

Version 5.9 2019-01-18
- API: added CellSetBorderVisible, TerritorySetBorderVisible
- [Fix] Fixed issue with setting diagonal cost in box topology

Version 5.8 2018-12-17
- Added camera property to Grid2D inspector to support multiple grid/camera setups
- Added support for per-side cross cost to grids of box topology
- Unity 2018.3 support

Version 5.7 2018-10-02
- Pathfinding costs switched to float
- Pathfinding: added diagonal cost parameter
- API: added CellGetFromGroup: returns all cells of a given group

Version 5.6 2018-08-27
- API: new pathfinding variant methods: CelSetCrossCost(xxx)

Version 5.5 2018-08-11
- Ability to set grid background to transparent (using CanvasBackgroundClear material and set camera's Clear Flags setting to Solid Color)
- Added texture read enabled check to grid mask inspector option
- API: Simplified setting cost of cells. New CellSetCrossCost method replaces CellSetAllSidesCrost

Version 5.4 2018-08-09
- API: added voronoiSites property which allows user-defined Voronoi sites (or cell centers in irregular topology)
- [Fix] Fixed territory cache issue when generating grid on runtime

Version 5.3 2018-06-11
- Added OnCellMouseUp event
- [Fix] Fixed group mask issue in pathfinding methods with non-squared grid
- [Fix] Fixed log messages regarding to texture missing in some materials

Version 5.2 2018-04-23
- Added enableTerritories option to take full control on when territories features are enabled
- [Fix] New memory manager prevents memory leak when scene is played again into Unity Editor

Version 5.1 2018-04-17
- New demo scene 18: Random Map

Version 5.0 2018-04-11
- Grids 2D now requires Unity 5.5 or later
- Reorganized public API (main public class file split into functional files for better function maintenance/locating)
- Cells can be grouped (useful to discard certain types of cells in PathFinding functions)
- API: added CellSetGroup / CellGetGroup
- API: added CellGetLineOfSight
- Updated demo scene 13 "PathFinding" to illustrate the new CellGetLineOfSight method

Version 4.4 2018-03-17
- New option to easily generate regular hexagons with desired size (video: https://youtu.be/ewy0DNNj5ZM)
- Added new demo scene 17: use of CellSetSprite method
- Support for sprite atlas in CellSetSprite method
- [Fix] Fixed CellSetTexture null exception error

Version 4.3 2017-DEC-12
- API: added MoveTo() method for moving objects over the grid. Updated demo scene 8.

Version 4.2.1 2017-OCT-5
- Cell tag field can now be modified in the Grid Editor
- [Fix] Fixed LoadConfiguration issue with cell colors
- [Fix] Fixed bug in CellMerge function which could affect other neighbour cells

Version 4.2 2017-JUL-31
  - Added "Even Layout" option for hexagonal topology
  
Version 4.1 2017-JUN-1
  - New demo scenes 15 & 16
  - Improved path finding algorithm to support max search cost and max steps
  - API: new effects CellBlink, CellFlash, CellFadeOut, TerritoryBlink, TerritoryFlash, TerritoryFadeOut

Version 4.0 2017-MAY-15
  - Path Finding: added per cell edge crossing cost
  - Update scene 14 with example to clear matching cells
  - API: Added CellSetSideCrossCost / CellSetAllSidesCrossCost
  - API: Added DrawLine for quick line drawing on hexagonal cells
  - [Fix] Fixed input issue with WebGL builds
  - [Fix] Removed shader warning on console

Version 3.4 2017-MAY-1
  - API: added CellGetNeighbours(cell, distance) which gets the nearest cells given a distance in cells steps
  - [Fix] Fixed path finding missing some cells with heavy usage
  - [Fix] Fixed highlight color issue
  
Version 3.3 2017-MAR-20
  - New demo scene #14 "Vertical Grid"
  - Improved highlighting - the highlight effect now preserves underline cell texture
  - API: new paremeter in CellToggleSurfaceRegion for rotating cell textures in local space
  
Version 3.2 2017-MAR-13
  - Added OnCellHighlight / OnTerritoryHighlight events
  - Faster hexagonal and box cells colorization
  - [Fix] Fixed highlight issue with some merged cells
  - [Fix] Fixed RespectOtherUI on mobile
  
Version 3.1 2016-DEC-16
  - Added RespectOtherUI to prevent interaction when pointer is over an UI element (button, image, ...)
  - [Fix] Fixed compatibility issues with Unity 5.5

Version 3.0 2016-SEP-26
  - A* Pathfinding support. New demo scene and inspector options.
  - Ability to define territories based on a texture. New option in inspector to assign territories texture.
  - Territories can now surround other territories. New inspector property.
  - New API: CellGetIsBorder - returns whether a cell is on the edge of the grid
  - Territories can be marked invisible
  
Version 2.2 2016-SEP-1
  - [Fix] Rotated orientation of box type grid to match hexagonal type orientation

Version 2.1 2016-AUG-26
  - [Fix] Changed hexagonal topology so all rows contains same number of cells (WARNING: this change can break previous hexagonal grid configurations!)  

Version 2.0 2016-AUG-1
 - New grid editor section with option to load configurations
 - New demo scene #9 showing how to transfer cells to another territory
 - Ability to hide territories outer borders
 - Added mask property to define cells visibility.
 - New API: CellSetTerritory.
 - Cells will be visible if at least one vertex if visible when applying mask.
 - CellGetAtPosition now can accept world space coordinates.
 - Option to prevent highlighting of invisible cells
 - [Fix] Fixed lower boundary of territory in hexagonal grid
 - [Fix] Fixed bug in territory frontiers line shader
  
Version 1.4.1 - Fixed cell selection when it's surrounded by another greater cell

Version 1.4 - New checkers board demo

Version 1.3 - Initial Launch







