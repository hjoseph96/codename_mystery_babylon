// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using UnityEditor;

namespace TenPN.DecisionFlex
{
    /**
       \brief 
       entries for the help menu
    */
    public static class Menu
    {
        public const string s_ver = "v1.4";
        
        [MenuItem("Help/DecisionFlex " + s_ver + "/Documentation")]
        public static void OpenDocs()
        {
            Debug.Log("Opening DecisionFlex docs. Find local docs in your DecisionFlex folder");
            Application.OpenURL(GetDocsPath("index.html"));
        }

        [MenuItem("Help/DecisionFlex " + s_ver + "/Changes")]
        public static void OpenChanges()
        {
            Debug.Log("Opening DecisionFlex changelog. Find local docs in your DecisionFlex folder");
            Application.OpenURL(GetDocsPath("changelog.html"));
        }

        [MenuItem("Help/DecisionFlex " + s_ver + "/Support")]
        public static void OpenContact()
        {
            Application.OpenURL("http://tenpn.uservoice.com");
        }

        static string GetDocsPath(string path)
        {
            return "http://www.tenpn.com/df-" + s_ver + "/" + path;
        }
    }
}
