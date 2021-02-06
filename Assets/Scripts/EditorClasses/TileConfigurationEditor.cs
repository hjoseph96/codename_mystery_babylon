using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileConfiguration))]
public class TileConfigurationEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        var config = target as TileConfiguration;
        DrawDefaultInspector();

        var rect = EditorGUILayout.GetControlRect();
        var size = 120f;
        var borderSize = 12;
        var offset = 20;
        rect.x = rect.width / 2 - size / 2;
        rect.y += offset + borderSize * 2;
        rect.width = size;
        rect.height = size;

        SirenixEditorGUI.DrawSolidRect(rect.Expand(borderSize * 2), new Color(1.0f, 1.0f, 1.0f, 0.5f));
        SirenixEditorGUI.DrawSolidRect(rect.Expand(borderSize), new Color(0.3f, 1.0f, 0.55f));

        var style = new GUIStyle();
        style.fontSize = 52;
        style.normal.textColor = Color.black;
        style.alignment = TextAnchor.MiddleCenter;

        config.TravelCost.TryGetValueExt(config.UnitType, out var cost);
        GUI.Label(rect, cost.ToString(), style);

        if (config.UnitType == UnitType.None)
        {
            return;
        }

        var blockExitColor = Color.blue;
        if (config.BlockExit.TryGetValue(Direction.Left, out var value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y - borderSize, borderSize, rect.height + borderSize * 2), blockExitColor);
        }

        if (config.BlockExit.TryGetValue(Direction.Right, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x + rect.width, rect.y - borderSize, borderSize, rect.height + borderSize * 2), blockExitColor);
        }

        if (config.BlockExit.TryGetValue(Direction.Up, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y - borderSize, rect.width + borderSize * 2, borderSize), blockExitColor);
        }

        if (config.BlockExit.TryGetValue(Direction.Down, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y + rect.height, rect.width + borderSize * 2, borderSize), blockExitColor);
        }

        rect = rect.Expand(borderSize);
        var blockEntranceColor = Color.red;
        if (config.BlockEntrance.TryGetValue(Direction.Left, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y - borderSize, borderSize, rect.height + 2 * borderSize), blockEntranceColor);
        }

        if (config.BlockEntrance.TryGetValue(Direction.Right, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x + rect.width, rect.y - borderSize, borderSize, rect.height + 2 * borderSize), blockEntranceColor);
        }

        if (config.BlockEntrance.TryGetValue(Direction.Up, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y - borderSize, rect.width + 2 * borderSize, borderSize), blockEntranceColor);
        }

        if (config.BlockEntrance.TryGetValue(Direction.Down, out value) && (value & config.UnitType) == config.UnitType)
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(rect.x - borderSize, rect.y + rect.height, rect.width + 2 * borderSize, borderSize), blockEntranceColor);
        }
    }
}