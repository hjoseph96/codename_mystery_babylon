/*
The MIT License (MIT)

Copyright (c) 2014 Andrew Fray

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using UnityEngine;
using System.Collections.Generic;

// shows recent label next to soldier
class WeaponLog : MonoBehaviour
{
    public void LogAction(string actionName)
    {
        m_actions.Enqueue(actionName);
        while (m_actions.Count > m_history)
        {
            m_actions.Dequeue();
        }

        var actsArray = m_actions.ToArray();
        var logBuilder = new System.Text.StringBuilder();
        logBuilder.Append("History:\n");
        for(int actIndex = 0; actIndex < actsArray.Length; ++actIndex)
        {
            bool isLast = actIndex == actsArray.Length-1;
            string wrapper = isLast ? "---\nLast: {0}" : "{0}\n";
            logBuilder.AppendFormat(wrapper, actsArray[actIndex]);
        }
        m_logContents = logBuilder.ToString();
    }
    
    //////////////////////////////////////////////////

    [SerializeField] private int m_history = 4;
    [SerializeField] private float m_padding;

    Queue<string> m_actions = new Queue<string>();
    string m_logContents = "";
    
    //////////////////////////////////////////////////

    private void OnGUI()
    {
        var logSize = GUI.skin.box.CalcSize(new GUIContent(m_logContents));
        
        var blCorner = Camera.main.WorldToScreenPoint(transform.position);
        blCorner.y = Screen.height - blCorner.y;
        var labelRect = new Rect(blCorner.x + m_padding, blCorner.y - logSize.y,
                                 logSize.x, logSize.y);
        GUI.Box(labelRect, m_logContents);
    }
}
