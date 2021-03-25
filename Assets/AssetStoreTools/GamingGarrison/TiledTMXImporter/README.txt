Tiled TMX Importer
------------------

This tool is intended for easy loading of Tiled TMX tilemap and TSX tileset files into Unity's new Tilemap system.

The importer is controlled via an editor menu, which appears under:
Window/Tiled TMX Importer

Once open, to use:
1) Drop a TMX file from your operating system into the box to select it for importing
2) Choose a tilemapper Grid object as a target to load into, or leave this box blank to create a new Grid in the scene
3) Select a Target Tileset directory - this is where the Sprite and Tile assets will be created from the tilesets referenced in the TMX.

When ready, hit the 'Import' button.

Imported tilesets are put into the Target Tileset directory, which by default is Assets/TileSets.
Since a TMX tilemap file potentially references many tilesets, each tileset is given it's own sub-directory during import.

Tiled TMX Importer will load properties that have been set on layers or objects inside Tiled.
Out of the box, "unity:tag", "unity:layer", "unity:prefab" or "unity:prefabReplace" properties will be processed.
The logic for these can be seen inside GamingGarrison/TilemapImporter/Editor/ImportOperations

Any custom properties can be handled by writing your own classes that implement the ITilemapImportOperation interface.
This is useful for extending functionality.
*These classes MUST be put in an Editor folder for the functions to be called on import.*

Validation Mode will check the TMX to see if it fits the known schema.  If the TMX is using new features or is from an unsupported
version of Tiled, then the validation will complain.  Sometimes, turning Validation Mode off will allow an import to proceed - but will
potentially skip over some of your data.  If you feel that Validation Mode is throwing errors that it shouldn't, please email me.


If you have any feedback, bug reports, or want a chat, my email is:
duncan.robert.stead@gmail.com

Thanks for reading, now go make some games! :)
Duncan