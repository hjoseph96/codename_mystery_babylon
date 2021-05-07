/*
Permission is hereby granted, free of charge, to any person  obtaining a copy of this software and associated 
documentation  files (the "Software"), to deal in the Software without  restriction, including without limitation 
the rights to use,  copy, modify, merge, publish, distribute, sublicense, and/or sell  copies of the Software, and 
to permit persons to whom the  Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;


public static class NDDebug
{
	public static void Log(string msg)
	{
		//UnityEngine.Debug.Log(msg);
	}
}


/// <summary> 
/// <para>A small data structure class hold values for making NaturalDocs config files </para>
/// </summary>
public class NaturalDocsConfig
{
	// the project name used in the docs
	public string Project = PlayerSettings.productName;
	// an optional subtitle
	public string Subtitle = "";
	// an optional credits line
	public string Copyright = "Copyright (C) " + PlayerSettings.companyName;
	// an optional timestamp
	public string Timestamp = "Updated month day, year";
	// the default scripts path that should be documented
	// per default the whole /Assets folder is documented
	public string ScriptsDirectory = Application.dataPath;
	// the html output folder
	public string DocDirectory = Application.dataPath.Replace("Assets", "Docs");
	// the path to NaturalDocs.exe
	public string PathtoNaturalDocs = "Set the path to NaturalDocs.exe (required)";

	public string ConfigDirectory = Application.dataPath + "/NaturalDocs/Editor/Config";

	// override ToString for debugging purposes
	public override string ToString()
	{
		string result = "\n";
		result += "    Project: " + Project + "\n";
		result += "    Subtitle: " + Subtitle + "\n";
		result += "    Copyright: " + Copyright + "\n";
		result += "    Timestamp: " + Timestamp + "\n";
		result += "    ScriptsDirectory: " + ScriptsDirectory + "\n";
		result += "    DocDirectory: " + DocDirectory + "\n";
		result += "    PathtoNaturalDocs: " + PathtoNaturalDocs + "\n";
		result += "    ConfigDirectory: " + ConfigDirectory + "\n";
		return result;
	}
}


/***************************************************************************** */
/* Class: NaturalDocsWindow

	An Editor Plugin for automatic doc generation through Natural Docs.
	The EditorWindow for the Natural Docs Settings Window

	Author: René Aye (http://unitysquid.com)

	Version: 1.0
 */
public class NaturalDocsWindow : EditorWindow 
{
	// Property: Instance
	// The window instance
	public static NaturalDocsWindow Instance;

	/* Enum: WindowModes

		Generate 		- The Genrate Docs window pane
		Configuration 	- The settings pane
		About 			- The About pane
	*/
	public enum WindowModes{Generate,Configuration,About}
	
	// Property: UnityProjectID
	// A unique Project ID used for storeing EditorPrefs
	// gets set in OnEnable()
	public string UnityProjectID = null;

	// Property: AssetsFolder
	// The path to the AssetsFolder (used for Themeing of DoxyGen, may be obsolete?)
	// gets set in OnEnable()
	public string AssetsFolder = null;

	// Property: Themes
	// The theme names (used for Themeing of DoxyGen, may be obsolete?)
	//public string[] Themes = new string[3] {"Default", "Dark and Colorful", "Light and Clean"};

	// Property: SelectedTheme
	// The current selected theme (used for Themeing of DoxyGen, may be obsolete?)
	//public int SelectedTheme = 1;

	// Property: DisplayMode
	// The current selected window pane
	WindowModes DisplayMode = WindowModes.Generate;

	// Property: Config
	// The current NaturalDocsConfig object
	static NaturalDocsConfig Config;

	// Property: NDConfigFileExists
	// flag if the Project.txt does exist
	static bool NDConfigFileExists = false;
	static bool NDNaturlaDocsExeFileExists = false;
	static bool NDRunSHFileExists = false;

	// Property: NDfileCreateProgress
	// for the progress bar
	float NDfileCreateProgress = -1.0f;

	// Property: NDoutputProgress
	// for the progress bar
	float NDoutputProgress = -1.0f;

	// Property: CreateProgressString
	// for the progress bar messages
	string CreateProgressString = "Creating NDfile..";
	public string BaseFileString = null;
	public string NaturalDocsOutputString = null;
	public string CurentOutput = null;
	NDThreadSafeOutput NaturalDocsOutput = null; 
	List<string> NaturalDocsLog = null;
	bool ViewLog = false;
	Vector2 scroll;
	bool DocsGenerated = false;
	
	
	Texture2D logoTexture;
	Rect logoSection;


	[MenuItem( "Window/Documentation with Natural Docs" )]

	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: Init
	// Initialzies the Window. Get called if the window is not present and the menu 
	// Window/Documentation with NaturalDocs is selected to open the window
	public static void Init()
	{
		NDDebug.Log("#NaturalDocs# Init");

		Instance = (NaturalDocsWindow)EditorWindow.GetWindow( typeof( NaturalDocsWindow ), false, "Documentation" );
		Instance.minSize = new Vector2( 420, 245 );
		Instance.maxSize = new Vector2( 420, 720 );

	}

	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: OnEnable
	// Gets called when the Window gets opened/enabled
	void OnEnable()
	{
		NDDebug.Log("#NaturalDocs# OnEnable");

		// set the UnityProjectID that is used as a prefix for all EditorPrefs keys

		UnityProjectID = PlayerSettings.productName+":";		// Load the ND config file

		// get the Assets Folder path and store it in AssetsFolder variable

		AssetsFolder = Application.dataPath;

		// Load the Config data from EditorPrefs if available

		LoadConfig();

		// set the progress bar to not filled

		NDoutputProgress = 0;

		// Load the logo texture
		//this.logoTexture = Resources.Load<Texture2D>("NaturalDocs/logo");
		this.logoTexture = (Texture2D) EditorGUIUtility.Load("Assets/NaturalDocs/Editor/Resources/logo.png");
		this.logoSection = new Rect(0,0,0,0);
	}


	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: OnDisable
	// Gets called when the Window gets disabled
	void OnDisable()
	{
		NDoutputProgress = 0;
		NaturalDocsLog = null;
	}


	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: OnGUI()
	// The GUI display loop. Displays the heade tabs and the window panes
	void OnGUI()
	{
		// Check if setup is ok to start generating docs with NaturalDocs.
		CheckConfiguration();

		// Display the header tabs
		DisplayHeadingToolbar();

		switch( DisplayMode )
		{
			case WindowModes.Generate:
				GenerateGUI();
			break;

			case WindowModes.Configuration:
				ConfigGUI();
			break;

			case WindowModes.About:
				AboutGUI();
			break;
		}
	}

	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: CheckConfiguration
	// Checks if the configuration is set and ok so that the Generate Button can be displayed
	// or an error message should be displayed
	void CheckConfiguration()
	{
		// check if project.txt does exist and set a flag in NDConfigFileExists
		if ( System.IO.File.Exists( Config.PathtoNaturalDocs ) ) 
		{
			NDNaturlaDocsExeFileExists = true;
		} else {
			NDNaturlaDocsExeFileExists = false;
		}

		// check if Project.txt does exist and set a flag in NDConfigFileExists
		if ( System.IO.File.Exists( Config.ConfigDirectory + "/Project.txt" ) ) 
		{
			NDConfigFileExists = true;
		} else {
			NDConfigFileExists = false;
		}

		// on MacOS we also check if the run.sh shell script does exist
		if ( Application.platform == RuntimePlatform.OSXEditor )
		{
			if ( System.IO.File.Exists( Config.ConfigDirectory + "/run.sh" ) ) 
			{
				NDRunSHFileExists = true;
			}else{
				NDRunSHFileExists = false;
			}
		}

		// On Windows set this always to true, since we do not need the run.sh on Windows 
		else if ( Application.platform == RuntimePlatform.WindowsEditor ) 
		{
			NDRunSHFileExists = true;
		}
	}


	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: DisplayHeadingToolbar
	// Displays the three header toolbar button tabs
	void DisplayHeadingToolbar()
	{
		GUIStyle normalButton = new GUIStyle( EditorStyles.toolbarButton );
		normalButton.fixedWidth = 140;

		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
		{
			// if GENERATE WINDOW PANE IS ACTIVE
			if( GUILayout.Toggle( DisplayMode == WindowModes.Generate, "Generate Documentation", normalButton ) )
			{
				NDfileCreateProgress = -1;
				DisplayMode = WindowModes.Generate;
			}

			// if GENERATE SETTINGS PANE IS ACTIVE

			if( GUILayout.Toggle( DisplayMode == WindowModes.Configuration, "Settings/Configuration", normalButton ) )
			{
				DisplayMode = WindowModes.Configuration;
			}

			// if GENERATE ABOUT PANE IS ACTIVE

			if( GUILayout.Toggle( DisplayMode == WindowModes.About, "About", normalButton ) )
			{
				NDfileCreateProgress = -1;
				DisplayMode = WindowModes.About;
			}
		}
		EditorGUILayout.EndHorizontal();
	}	


	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: ConfigGUI
	// Draws the Config window pane
	void ConfigGUI()
	{
		GUILayout.Space (10);

		// disable the save button if no project title is set or the path to the NaturalDocs.exe is not set

		if ( Config.Project == "Enter your Project name (Required)" || Config.Project == "" || Config.PathtoNaturalDocs == "" ) 
			GUI.enabled = false;
		
		// create Save Config button

		if ( GUILayout.Button ("Save Configuration", GUILayout.Height(40)) )
		{
			// Call MakeNewNDConfigFileOSX when button is clicked to create a new config file like Project.txt

			if ( !System.IO.Directory.Exists( Config.ConfigDirectory ) )
			{
				NDDebug.Log("#NaturalDocs# ConfigDirectory does not exist: " + Config.ConfigDirectory);
				System.IO.Directory.CreateDirectory( Config.ConfigDirectory );
			}

			// MACOS
			if ( Application.platform == RuntimePlatform.OSXEditor ) 
				MakeNewNDConfigFileOSX( Config );
			// WINDOWS
			else if ( Application.platform == RuntimePlatform.WindowsEditor ) 
				MakeNewNDConfigFileWINDOWS( Config );
		}

		// create a progress bar if NDfileCreateProgress is > -1

		if ( NDfileCreateProgress >= 0 )
		{
			Rect r = EditorGUILayout.BeginVertical();
			EditorGUI.ProgressBar(r, NDfileCreateProgress, CreateProgressString);
			GUILayout.Space(16);
			EditorGUILayout.EndVertical();
		}

		// enable the GUI from here again

		GUI.enabled = true;

		// label and text field input field for path to NaturalDocs.exe

		GUILayout.Space (20);
		GUILayout.Label("Set Path to Natural Docs Install", EditorStyles.boldLabel);

		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
			Config.PathtoNaturalDocs = EditorGUILayout.TextField( "NaturalDocs.exe : ",Config.PathtoNaturalDocs );
			if ( GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)) )
				Config.PathtoNaturalDocs = EditorUtility.OpenFilePanel( "Where is NaturalDocs.exe installed?", "", "" );
		EditorGUILayout.EndHorizontal();

		// Project Details

		GUILayout.Space (20);
		GUILayout.Label("Provide some details about the project",EditorStyles.boldLabel);
		GUILayout.Space (5);
		Config.Project = EditorGUILayout.TextField("Project Title: ",Config.Project);
		Config.Subtitle = EditorGUILayout.TextField("Project Subtitle: ",Config.Subtitle);
		Config.Copyright = EditorGUILayout.TextField("Copyright: ",Config.Copyright);
		Config.Timestamp = EditorGUILayout.TextField("Timestamp: ",Config.Timestamp);

		// GUILayout.Space (15);
		// GUILayout.Label("Select Theme",EditorStyles.boldLabel);
		// GUILayout.Space (5);
		// SelectedTheme = EditorGUILayout.Popup(SelectedTheme,Themes) ;		

		GUILayout.Space (20);
		GUILayout.Label("Setup the Directories",EditorStyles.boldLabel);
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.ScriptsDirectory = EditorGUILayout.TextField("Scripts folder: ",Config.ScriptsDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.ScriptsDirectory = EditorUtility.OpenFolderPanel("Select your scripts folder", Config.ScriptsDirectory, "");
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.DocDirectory = EditorGUILayout.TextField("Output folder: ",Config.DocDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.DocDirectory = EditorUtility.OpenFolderPanel("Select your ouput Docs folder", Config.DocDirectory, "");
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (5);
		GUILayout.Space (30);
		GUILayout.Label("By default Natural Docs will search through your whole Assets folder for C# script files to document. Then it will output the documentation it generates into a folder called \"Docs\" that is placed in your project folder next to the Assets folder. If you would like to set a specific script or output folder you can do so above. ",EditorStyles.wordWrappedMiniLabel);
		GUILayout.Space (30);
		EditorGUILayout.EndHorizontal();
	}


	// //////////////////////////////////////////////////////////////////////////////////////
	// Function: AboutGUI
	// Draws the About window pane
	void AboutGUI()
	{
		this.logoSection.x = (420 - 150) / 2;
		this.logoSection.y = 65;
		this.logoSection.width = 150;
		this.logoSection.height = 28;

		//GUILayout.Box( this.logoTexture );
		GUI.DrawTexture( this.logoSection, this.logoTexture );

		GUIStyle CenterLable = new GUIStyle(EditorStyles.largeLabel);
		GUIStyle littletext = new GUIStyle(EditorStyles.miniLabel) ;
		CenterLable.alignment = TextAnchor.MiddleCenter;
		GUILayout.Space (100);
		GUILayout.Label( " C# Documentation Generation with Natural Docs",CenterLable );
		GUILayout.Label( "Version: 1.0", CenterLable );
		GUILayout.Label( "By: UnitySquid – René Aye", CenterLable );

		GUILayout.Space (20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Follow me for more Unity tips and tricks", littletext );
		GUILayout.Space (15);
		if(GUILayout.Button( "twitter", GUILayout.Width(150)))
			Application.OpenURL( "https://twitter.com/@unitysquid" );
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Visit my site for more plugins and tutorials", littletext );
		if(GUILayout.Button( "unitysquid.com", GUILayout.Width(150)))
			Application.OpenURL( "https://unitysquid.com" );
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Natural Docs is written by Greg Valure", littletext );
		if(GUILayout.Button( "naturaldocs.org", GUILayout.Width(150)))
			Application.OpenURL( "https://www.naturaldocs.org/" );
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Getting started documenting", littletext );
		if( GUILayout.Button( "online guide", GUILayout.Width(150) ) )
			Application.OpenURL( "https://www.naturaldocs.org/getting_started/documenting_your_code/" );
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
	}

	// Function: GenerateGUI
	// Draws the Generate docs window pane
	void GenerateGUI()
	{
		// if a Project.txt config file is found show the button to generate and browse the docs

		if ( NDConfigFileExists && NDNaturlaDocsExeFileExists && NDRunSHFileExists )
		{
			//NDDebug.Log(NDoutputProgress);
			GUILayout.Space (10);
			if ( !DocsGenerated )
				GUI.enabled = false;

			// Button to open the docs in browser
			if ( GUILayout.Button ("Browse Documentation", GUILayout.Height(40)) )
				Application.OpenURL( "File://" + Config.DocDirectory + "/index.html" );

			GUI.enabled = true;	

			if ( NaturalDocsOutput == null )
			{
				if ( GUILayout.Button ("Run Natural Docs", GUILayout.Height(40)) )
				{
					DocsGenerated = false;
					RunNaturalDocs();
				}
					
				if ( DocsGenerated && NaturalDocsLog != null )
				{
					if ( GUILayout.Button( "View Natural Docs Log", EditorStyles.toolbarDropDown) )
						ViewLog = !ViewLog;

					if ( ViewLog )
					{
						scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
						foreach(string logitem in NaturalDocsLog)
						{
							EditorGUILayout.SelectableLabel(logitem,EditorStyles.miniLabel,GUILayout.ExpandWidth(true));
						}
		            	EditorGUILayout.EndScrollView();
					}
				}
			}
			else
			{
				if ( NaturalDocsOutput.isStarted() && !NaturalDocsOutput.isFinished() )
				{
					string currentline = NaturalDocsOutput.ReadLine();
					NDoutputProgress = NDoutputProgress + 0.1f;
					if(NDoutputProgress >= 0.9f)
						NDoutputProgress = 0.75f;
					Rect r = EditorGUILayout.BeginVertical();
					EditorGUI.ProgressBar(r, NDoutputProgress,currentline );
					GUILayout.Space(40);
					EditorGUILayout.EndVertical();
				}

	        	if ( NaturalDocsOutput.isFinished() )
	        	{
	        		if ( Event.current.type == EventType.Repaint )
					{
						//SetTheme(SelectedTheme);

		        		NaturalDocsLog = NaturalDocsOutput.ReadFullLog();
		        		NDoutputProgress = -1.0f;
		        		NaturalDocsOutput = null;
	        			DocsGenerated = true;
	        			EditorPrefs.SetBool( UnityProjectID + "DocsGenerated", DocsGenerated );
					}
	        	}
			}
		}

		// if no Project.txt config is found display an error message
		else
		{
			GUIStyle ErrorLabel = new GUIStyle(EditorStyles.largeLabel);
			ErrorLabel.alignment = TextAnchor.MiddleCenter;
			GUILayout.Space(20);
			GUI.contentColor = Color.red;

			string errorMessage = "";

			if ( NDNaturlaDocsExeFileExists == false)
				errorMessage += "NaturalDocs.exe not found\nPlease goto Settings/Configuration\nSet the path to your NaturalDocs.exe.\nPress Save Configuration.\n\n";

			else if ( NDConfigFileExists == false || NDRunSHFileExists == false)
				errorMessage += "Config file not found\nPlease goto Settings/Configuration\nPress Save Configuration.\n\n";

			GUILayout.Label( errorMessage, ErrorLabel );
		}
	}


	// Function: ReadConfig
	// Reads the Project.txt and returns its content as a string
	// Is used to rewrite the Project.txt and append the config settings to it in MakeNewNDConfigFileOSX()
	// Returns: the contents of Project.txt as a string
	public static string ReadConfig()
	{
		NDDebug.Log("#NaturalDocs# ReadConfig");

		//string configPath = Application.dataPath + "/NaturalDocs/Editor/Config";
		string configFile = Config.ConfigDirectory + "/Project.txt";
		string result = "";

		using (StreamReader file = new StreamReader( configFile )) {
			string line;
			// Read and display lines from the file until the end of 
			// the file is reached.
			while ((line = file.ReadLine()) != null) 
			{
				result += line + "\n";
			}
		}

		return result;
	}


	// Function: MakeNewNDConfigFileWINDOWS
	// Creates a new NaturalDocsConfig instance on Windows machines
	// Parameters:
    // 		config - a NaturalDocsConfig object instance
    public static int MakeNewNDConfigFileWINDOWS( NaturalDocsConfig config )
    {
		NDDebug.Log( "#NDRunner# MakeNewNDConfigFileWINDOWS() config: " + config.ToString() );

		// pre-assemble the path strings we will need
		string configPath 		= Application.dataPath + "/NaturalDocs/Editor/Config";
		string configScript 	= configPath + "/Project.txt";
		string languagesScript 	= configPath + "/Languages.txt";
		string commentsScript 	= configPath + "/Comments.txt";

		// delete all the confoguration files
		System.IO.File.Delete( configScript );
		System.IO.File.Delete( languagesScript );
		System.IO.File.Delete( commentsScript );

		NDDebug.Log( "#NDRunner# MakeNewNDConfigFileWINDOWS() Create config files");

		// 1. call the NaturalDocs.exe the first time create the default config files
		System.Diagnostics.Process process = new System.Diagnostics.Process {
			StartInfo = {
				FileName = config.PathtoNaturalDocs,
				Arguments =  configPath,
				UseShellExecute = false,
				CreateNoWindow = false
			}
		};
		process.Start();
		process.WaitForExit();
		NDDebug.Log( "#NDRunner# MakeNewNDConfigFileWINDOWS() DONE");


		// 2. update the Project.txt config file by appending the project settings
		NDDebug.Log( "#NDRunner# MakeNewNDConfigFileWINDOWS() Read new config file");

		// read the current default Project.txt which has not settings at all, only comments in it
		string baseConfig = ReadConfig();
		NDDebug.Log("#NDRunner# MakeNewNDConfigFileOSX() DONE baseConfig: " + baseConfig);

		// append the settings strings to the file
		baseConfig += "\n";
		baseConfig += "Title: " + config.Project + "\n";
		if ( config.Subtitle != "" ) baseConfig += "Subtitle: " + config.Subtitle + "\n";
		if ( config.Copyright != "" ) baseConfig += "Copyright: " + config.Copyright + "\n";
		if ( config.Timestamp != "" ) baseConfig += "Timestamp: " + config.Timestamp + "\n";
		baseConfig += "Source Folder: \"" + config.ScriptsDirectory + "\"\n";
		baseConfig += "HTML Output Folder: \"" + config.DocDirectory + "\"\n";

		//
		// HERE WE CAN ADD THE SELECTED THEME STYLE
		//		

		// delete the old Project.txt
		System.IO.File.Delete( configScript );

		// save the new Project.txt
		using (StreamWriter file = new StreamWriter(configScript, false, Encoding.UTF8)) {
			file.Write(baseConfig);
        	file.Close();
		}

		return 0;
	}


	// Function: MakeNewNDConfigFileOSX
	// Creates a new NaturalDocsConfig instance on MacOS machines
	// Parameters:
    // 		config - a NaturalDocsConfig object instance
    public static int MakeNewNDConfigFileOSX( NaturalDocsConfig config )
    {
		NDDebug.Log( "#NDRunner# MakeNewNDConfigFileOSX() config: " + config.ToString() );

		string configPath = Application.dataPath + "/NaturalDocs/Editor/Config";

		// 1. create a shell script
		// 2. chmod it as executable
		// 3. call that shell script
		// 4. add project settings to Project.txt

		string shellScript = configPath + "/run.sh";
		string configScript = configPath + "/Project.txt";
		string languagesScript = configPath + "/Languages.txt";
		string commentsScript = configPath + "/Comments.txt";

		NDDebug.Log("#NDRunner#" + shellScript);
		NDDebug.Log("#NDRunner#" + configScript);
		NDDebug.Log("#NDRunner#" + languagesScript);
		NDDebug.Log("#NDRunner#" + commentsScript);

		if ( System.IO.File.Exists( shellScript ) ) System.IO.File.Delete( shellScript );
		if ( System.IO.File.Exists( configScript ) ) System.IO.File.Delete( configScript );
		if ( System.IO.File.Exists( languagesScript ) ) System.IO.File.Delete( languagesScript );
		if ( System.IO.File.Exists( commentsScript ) ) System.IO.File.Delete( commentsScript );

		// 1. create a shell script
		using (StreamWriter file = new StreamWriter(shellScript, false, Encoding.UTF8)) {
			file.WriteLine("#!/bin/sh");
			//file.WriteLine("printf \"\033c\"");
			file.WriteLine( string.Format( "mono \"{0}\" \"{1}\" -r", config.PathtoNaturalDocs, configPath ) );
			file.WriteLine("kill -9 $(ps -p $(ps -p $PPID -o ppid=) -o ppid=)");
        	file.Close();
		}

		// 2. chmod it as executable
		System.Diagnostics.Process chmod = new System.Diagnostics.Process {
			StartInfo = {
				FileName = @"/bin/bash",
				Arguments = string.Format("-c \"chmod 777 {0}\"", shellScript),
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		chmod.Start();
		chmod.WaitForExit();
	
		// 3. call that shell script
		NDDebug.Log("#NDRunner#" + string.Format("{0}", shellScript));
		System.Diagnostics.Process process = new System.Diagnostics.Process {
			StartInfo = {
				FileName = @"/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
				Arguments = shellScript,
				UseShellExecute = false,
				CreateNoWindow = false
			}
		};

		process.Start();
		process.WaitForExit();

		// 4. update the Project.txt config file by appending the project settings

		// read the current default Project.txt which has not settings at all, only comments in it
		string baseConfig = ReadConfig();
		NDDebug.Log("#NDRunner# MakeNewNDConfigFileOSX() baseConfig: " + baseConfig);

		// append the settings strings to the file
		baseConfig += "\n";
		baseConfig += "Title: " + config.Project + "\n";
		if ( config.Subtitle != "" ) baseConfig += "Subtitle: " + config.Subtitle + "\n";
		if ( config.Copyright != "" ) baseConfig += "Copyright: " + config.Copyright + "\n";
		if ( config.Timestamp != "" ) baseConfig += "Timestamp: " + config.Timestamp + "\n";
		baseConfig += "Source Folder: \"" + config.ScriptsDirectory + "\"\n";
		baseConfig += "HTML Output Folder: \"" + config.DocDirectory + "\"\n";

		//
		// HERE WE CAN ADD THE SELECTED THEME STYLE
		//

		// delete the old Project.txt
		if ( System.IO.File.Exists( configScript ) ) System.IO.File.Delete( configScript );

		// save the new Project.txt
		using (StreamWriter file = new StreamWriter(configScript, false, Encoding.UTF8)) {
			file.Write(baseConfig);
        	file.Close();
		}

		return 0;
    }


	void SaveConfigtoEditor(NaturalDocsConfig config)
	{
		EditorPrefs.SetString( UnityProjectID+"NDProjectName", config.Project );
		EditorPrefs.SetString( UnityProjectID+"NDProjectSubtitle", config.Subtitle );
		EditorPrefs.SetString( UnityProjectID+"NDProjectCopyright", config.Copyright );
		EditorPrefs.SetString( UnityProjectID+"NDProjectTimestamp", config.Timestamp );
		EditorPrefs.SetString( UnityProjectID+"NDProjectFolder", config.ScriptsDirectory );
		EditorPrefs.SetString( UnityProjectID+"NDProjectOutput", config.DocDirectory );
		EditorPrefs.SetString( "NDEXE", config.PathtoNaturalDocs);
		//EditorPrefs.SetInt( UnityProjectID+"NDTheme", SelectedTheme);
	}

	// Function: LoadConfig
	// Loads config data from EditorPrefs
	void LoadConfig()
	{
		NDDebug.Log("#NaturalDocs# LoadConfig");
		
		// if Config is null we need to create a new one
		
		if ( Config == null )
		{
			// check if we find config data in the EditorPrefs
			// if not create a new empty NaturalDocsConfig instance

			if ( !LoadSavedConfig() ) 
			{
				Config = new NaturalDocsConfig();
				NDDebug.Log("#NaturalDocs# LoadConfig - No EditorPrefs found -> create a new one with default values.");

				SetDefaultNaturalDocsPath( Config );
			}
		}	


		if(EditorPrefs.HasKey( UnityProjectID+"DocsGenerated") )
			DocsGenerated = EditorPrefs.GetBool( UnityProjectID + "DocsGenerated" );

		if(EditorPrefs.HasKey( UnityProjectID+"NDTheme") )
			//SelectedTheme = EditorPrefs.GetInt( UnityProjectID + "NDTheme" );

		if(EditorPrefs.HasKey( "NDEXE") )
			Config.PathtoNaturalDocs = EditorPrefs.GetString( "NDEXE" );

		NDDebug.Log("#NaturalDocs# Config: " + Config.ToString() );
		NDDebug.Log("#NaturalDocs# DocsGenerated: " + DocsGenerated.ToString() );
		//NDDebug.Log("#NaturalDocs# SelectedTheme: " + SelectedTheme.ToString() );
	}


	// Function: LoadSavedConfig
	// Create a Config instance of the values stores in EditorPrefs
	// returns true if config in EditorPrefs exists, false if not
	bool LoadSavedConfig()
	{
		if( EditorPrefs.HasKey( UnityProjectID + "NDProjectName") )
		{
			Config 					= new NaturalDocsConfig();
			Config.Project 			= EditorPrefs.GetString(UnityProjectID+"NDProjectName");
			Config.Subtitle			= EditorPrefs.GetString(UnityProjectID+"NDProjectSubtitle");
			Config.Copyright 		= EditorPrefs.GetString(UnityProjectID+"NDProjectCopyright");
			Config.Timestamp 		= EditorPrefs.GetString(UnityProjectID+"NDProjectTimestamp");
			Config.DocDirectory 	= EditorPrefs.GetString(UnityProjectID+"NDProjectOutput");
			Config.ScriptsDirectory = EditorPrefs.GetString(UnityProjectID+"NDProjectFolder");	

			NDDebug.Log("#NaturalDocs# LoadSavedConfig - EditorPrefs found");
			return true;
		}

		return false;
	}


	// Function: SetDefaultNaturalDocsPath
	// Tries to set the NatrualDocs.exe path depending on OS
	void SetDefaultNaturalDocsPath(NaturalDocsConfig config)
	{
		NDDebug.Log("#NaturalDocs# SetDefaultNaturalDocsPath()");

		// MACOS
		if ( Application.platform == RuntimePlatform.OSXEditor ) 
		{
			string defaultPath = "/Applications/Natural Docs/NaturalDocs.exe";

			if ( System.IO.File.Exists( defaultPath ) )
				config.PathtoNaturalDocs = defaultPath;
			else
				NDDebug.Log("#NaturalDocs# default path not found: " + defaultPath );
		}

		// WINDOWS
		if ( Application.platform == RuntimePlatform.WindowsEditor ) 
		{
			string defaultPath = "C:/Program Files (x86)/Natural Docs/NaturalDocs.exe";

			if ( System.IO.File.Exists( defaultPath ) )
				config.PathtoNaturalDocs = defaultPath;
			else
				NDDebug.Log("#NaturalDocs# default path not found: " + defaultPath );
		}
		
	}


	public static void OnNaturalDocsFinished(int code)
	{
		if(code != 0)
		{
		}
	}

	void SetTheme(int theme)
	{
	}


	// Function: RunNaturalDocs
	// Starts the documentation generating process with console command
	public void RunNaturalDocs()
	{
		NDDebug.Log("#NaturalDocs# RunNaturalDocs()");

		// check if the output directory does exist, if not create it
		if ( !System.IO.Directory.Exists( Config.DocDirectory ) )
		{
			NDDebug.Log("#NaturalDocs# DocDirectory does not exist: " + Config.DocDirectory);
			System.IO.Directory.CreateDirectory( Config.DocDirectory );
		}

		string[] Args = new string[2];
		Args[0] = Config.ConfigDirectory;
		Args[1] = "-r"; 		// add the -r parameter so that NaturalDocs are always rebuild completely FOR WINDOWS ONLY
								// since Mac uses a shell script the whole Args array gets overwritten in NDRunner.RunMacOS()
		
      	NaturalDocsOutput = new NDThreadSafeOutput();
      	NaturalDocsOutput.SetStarted();

      	Action<int> setcallback = (int returnCode) => OnNaturalDocsFinished(returnCode);

      	NDRunner NaturalDocs = new NDRunner( Config.PathtoNaturalDocs, Args, NaturalDocsOutput, setcallback);

      	Thread NaturalDocsThread = new Thread(new ThreadStart(NaturalDocs.RunThreadedND));
      	NaturalDocsThread.Start();
	}

}

/// <summary>
///  This class spawns and runs NaturalDocs in a separate thread, and could serve as an example of how to create 
///  plugins for unity that call a command line application and then get the data back into Unity safely.	 
/// </summary>
public class NDRunner
{
	NDThreadSafeOutput SafeOutput;
	public Action<int> onCompleteCallBack;
	List<string> NDLog = new List<string>();
	public string EXE = null;
	public string[] Args;
	static string WorkingFolder;

	public NDRunner(string exepath, string[] args, NDThreadSafeOutput safeoutput, Action<int> callback)
	{
		NDDebug.Log("#NDRunner# new NDRunner");

		EXE = exepath;
		Args = args;
		SafeOutput = safeoutput;
		onCompleteCallBack = callback;
		WorkingFolder = FileUtil.GetUniqueTempPathInProject();
		System.IO.Directory.CreateDirectory(WorkingFolder);

		NDDebug.Log("#NDRunner# EXE: " + EXE);
		NDDebug.Log("#NDRunner# Args: " + Args);
		NDDebug.Log("#NDRunner# WorkingFolder: " + WorkingFolder);
	}

	public void updateOutputString(string output)
	{
		NDDebug.Log("#NDRunner# updateOutputString() output: " + output);
		SafeOutput.WriteLine(output);
		NDLog.Add(output);
	}

	public void RunThreadedND()
	{
		NDDebug.Log("#NDRunner# RunThreadedND()");

		Action<string> GetOutput = (string output) => updateOutputString(output);
		int ReturnCode = 0;

		// WINDOWS
		if ( Application.platform == RuntimePlatform.WindowsEditor ) 
			ReturnCode = RunWindows( GetOutput, null, EXE, Args);

		// MAC OS
		else if ( Application.platform == RuntimePlatform.OSXEditor ) 
			ReturnCode = RunMacOS( GetOutput, null, EXE, Args);
		

		SafeOutput.WriteFullLog(NDLog);
		SafeOutput.SetFinished();
		onCompleteCallBack(ReturnCode);
	}

    /// <summary>
    /// Runs the specified executable with the provided arguments and returns the process' exit code.
    /// </summary>
    /// <param name="output">Recieves the output of either std/err or std/out</param>
    /// <param name="input">Provides the line-by-line input that will be written to std/in, null for empty</param>
    /// <param name="exe">The executable to run, may be unqualified or contain environment variables</param>
    /// <param name="args">The list of unescaped arguments to provide to the executable</param>
    /// <returns>Returns process' exit code after the program exits</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'
    public static int RunWindows(Action<string> output, TextReader input, string exe, params string[] args)
    {
		NDDebug.Log("#NDRunner# RunWindows()");

        if (String.IsNullOrEmpty(exe))
            throw new FileNotFoundException();
        if (output == null)
            throw new ArgumentNullException("output");

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute 		= false;
        psi.RedirectStandardError 	= true;
        psi.RedirectStandardOutput 	= true;
        psi.RedirectStandardInput 	= true;
        psi.WindowStyle 			= ProcessWindowStyle.Hidden;
        psi.CreateNoWindow 			= true;
        psi.ErrorDialog 			= false;
        psi.WorkingDirectory 		= WorkingFolder;
        psi.FileName 				= FindExePath(exe); 
        psi.Arguments 				= EscapeArguments(args); 

		NDDebug.Log("#NDRunner# psi.Arguments: " + psi.Arguments);


		using (Process process = Process.Start(psi))
		using (ManualResetEvent mreOut = new ManualResetEvent(false), mreErr = new ManualResetEvent(false))
		{
			NDDebug.Log("#NDRunner# Process ...");

			process.OutputDataReceived += (o, e) => { if (e.Data == null) mreOut.Set(); else output(e.Data); };
			process.BeginOutputReadLine();
			process.ErrorDataReceived += (o, e) => { if (e.Data == null) mreErr.Set(); else output(e.Data); };
			process.BeginErrorReadLine();

			string line;
			while (input != null && null != (line = input.ReadLine()))
				process.StandardInput.WriteLine(line);

			process.StandardInput.Close();
			process.WaitForExit();

			mreOut.WaitOne();
			mreErr.WaitOne();
			return process.ExitCode;
		}
    }


   /// <summary>
    /// Runs the specified executable with the provided arguments and returns the process' exit code.
    /// </summary>
    /// <param name="output">Recieves the output of either std/err or std/out</param>
    /// <param name="input">Provides the line-by-line input that will be written to std/in, null for empty</param>
    /// <param name="exe">The executable to run, may be unqualified or contain environment variables</param>
    /// <param name="args">The list of unescaped arguments to provide to the executable</param>
    /// <returns>Returns process' exit code after the program exits</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'
    public static int RunMacOS(Action<string> output, TextReader input, string exe, params string[] args)
    {
		NDDebug.Log("#NDRunner# RunMacOS()");

        if (output == null)
            throw new ArgumentNullException("output");

		output("Start generating docs ...");

		string shellScript = args[0] + "/run.sh";
	
		/*
		// call that shell script
		System.Diagnostics.Process process = new System.Diagnostics.Process {
			StartInfo = {
				FileName = @"/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
				Arguments = shellScript,
				UseShellExecute = false,
				CreateNoWindow = false,
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
			}
		};

		process.Start();
		process.WaitForExit();

		return 0;
		*/

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute 		= false;
        psi.RedirectStandardError 	= true;
        psi.RedirectStandardOutput 	= true;
        psi.RedirectStandardInput 	= true;
        psi.WindowStyle 			= ProcessWindowStyle.Hidden;
        psi.CreateNoWindow 			= true;
        psi.ErrorDialog 			= false;
        psi.WorkingDirectory 		= WorkingFolder;
        psi.FileName 				= @"/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal"; 
        psi.Arguments 				= shellScript; 

		NDDebug.Log("#NDRunner# psi.Arguments: " + psi.Arguments);

		using (Process process = Process.Start(psi))
		using (ManualResetEvent mreOut = new ManualResetEvent(false), mreErr = new ManualResetEvent(false))
		{
			NDDebug.Log("#NDRunner# Process ...");

			process.OutputDataReceived += (o, e) => { if (e.Data == null) mreOut.Set(); else output(e.Data); };
			process.BeginOutputReadLine();
			process.ErrorDataReceived += (o, e) => { if (e.Data == null) mreErr.Set(); else output(e.Data); };
			process.BeginErrorReadLine();

			string line;
			while (input != null && null != (line = input.ReadLine()))
			{
				process.StandardInput.WriteLine(line);
				NDDebug.Log("#NDRunner# " + line);
				output(line);
			}

			process.StandardInput.Close();
			process.WaitForExit();

			mreOut.WaitOne();
			mreErr.WaitOne();

			output("Finished");

			return process.ExitCode;
		}

    }


    /// <summary>
    /// Quotes all arguments that contain whitespace, or begin with a quote and returns a single
    /// argument string for use with Process.Start().
    /// </summary>
    /// <param name="args">A list of strings for arguments, may not contain null, '\0', '\r', or '\n'</param>
    /// <returns>The combined list of escaped/quoted strings</returns>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'</exception>
    public static string EscapeArguments(params string[] args)
    {
        StringBuilder arguments = new StringBuilder();
        Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
        Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
        Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
        for (int carg = 0; args != null && carg < args.Length; carg++)
        {
            if (args[carg] == null)
            {
                throw new ArgumentNullException("args[" + carg + "]");
            }
            if (invalidChar.IsMatch(args[carg]))
            {
                throw new ArgumentOutOfRangeException("args[" + carg + "]");
            }
            if (args[carg] == String.Empty)
            {
                arguments.Append("\"\"");
            }
            else if (!needsQuotes.IsMatch(args[carg]))
            {
                arguments.Append(args[carg]);
            }
            else
            {
                arguments.Append('"');
                arguments.Append(escapeQuote.Replace(args[carg], m =>
                                                     m.Groups[1].Value + m.Groups[1].Value +
                                                     (m.Groups[2].Value == "\"" ? "\\\"" : "")
                                                    ));
                arguments.Append('"');
            }
            if (carg + 1 < args.Length)
                arguments.Append(' ');
        }

        return arguments.ToString();
    }


    /// <summary>
    /// Expands environment variables and, if unqualified, locates the exe in the working directory
    /// or the evironment's path.
    /// </summary>
    /// <param name="exe">The name of the executable file</param>
    /// <returns>The fully-qualified path to the file</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    public static string FindExePath(string exe)
    {
		NDDebug.Log("#NDRunner# FindExePath exe: " + exe);

        exe = Environment.ExpandEnvironmentVariables(exe);
        if (!File.Exists(exe))
        {
            if (Path.GetDirectoryName(exe) == String.Empty)
            {
                foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                {
                    string path = test.Trim();
                    if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                        return Path.GetFullPath(path);
                }
            }
            throw new FileNotFoundException(new FileNotFoundException().Message, exe);
        }

		NDDebug.Log("#NDRunner# found: " + Path.GetFullPath(exe));

        return Path.GetFullPath(exe);
    }	
}	


/// <summary>
///  This class encapsulates the data output by NaturalDocs so it can be shared with Unity in a thread share way.	 
/// </summary>
public class NDThreadSafeOutput
{
   private ReaderWriterLockSlim outputLock = new ReaderWriterLockSlim();
   private string CurrentOutput = "";  
   private List<string> FullLog = new List<string>();
   private bool Finished = false;
   private bool Started = false;

   public string ReadLine( )
   {
        NDDebug.Log("#NDThreadSafeOutput# ReadLine()");

        outputLock.EnterReadLock();
        try
        {
            return CurrentOutput;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public void SetFinished( )
   {
        NDDebug.Log("#NDThreadSafeOutput# SetFinished()");

        outputLock.EnterWriteLock();
        try
        {
            Finished = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public void SetStarted( )
   {
        NDDebug.Log("#NDThreadSafeOutput# SetStarted()");

        outputLock.EnterWriteLock();
        try
        {
            Started = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public bool isStarted( )
   {
        NDDebug.Log("#NDThreadSafeOutput# isStarted()");

        outputLock.EnterReadLock();
        try
        {
            return Started;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public bool isFinished( )
   {
        NDDebug.Log("#NDThreadSafeOutput# isFinished()");

        outputLock.EnterReadLock();
        try
        {
            return Finished;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }
   
   public List<string> ReadFullLog()
   {
        NDDebug.Log("#NDThreadSafeOutput# ReadFullLog()");

        outputLock.EnterReadLock();
        try
        {
            return FullLog;
        }
        finally
        {
            outputLock.ExitReadLock();
        } 
   }

   public void WriteFullLog(List<string> newLog)
   {
        NDDebug.Log("#NDThreadSafeOutput# WriteFullLog()");

        outputLock.EnterWriteLock();
        try
        {
           FullLog = newLog;
        }
        finally
        {
            outputLock.ExitWriteLock();
        } 
   }

   public void WriteLine(string newOutput)
    {
        NDDebug.Log("#NDThreadSafeOutput# WriteLine() newOutput: " + newOutput);

        outputLock.EnterWriteLock();
        try
        {
            CurrentOutput = newOutput;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }
}


