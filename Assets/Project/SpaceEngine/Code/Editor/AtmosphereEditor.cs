﻿using SpaceEngine.Environment.Atmospheric;

using UnityEditor;

[CustomEditor(typeof(Atmosphere))]
public class AtmosphereEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Atmosphere cTarget = (Atmosphere)target;

        base.OnInspectorGUI();

        EditorGUILayout.Space();
        cTarget.AtmosphereBase = (AtmosphereBase)EditorGUILayout.EnumPopup("Based on (Preset):", cTarget.AtmosphereBase);
        EditorGUILayout.Space();
    }
}