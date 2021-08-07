using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DialogueAttributesParser
{
    public static Dictionary<string,object> GetEntityInstanceSpeaker(string text, out string dialogueText)
    {
        var dialogueAttributes = new DialogueAttributes();
        Dictionary<string, object> result = new Dictionary<string, object>();


        var attributesToScanFor = dialogueAttributes.GetType().GetMethods();

        if (!attributesToScanFor.Any((method) => text.Contains(method.Name)))
        {
            dialogueText = text;
            return result;
        }

        string cacheText = text;
        int openingIndex = cacheText.IndexOf("<");
        int closingIndex = cacheText.IndexOf(">");


        string attributesStr = cacheText.Substring(openingIndex + 1, closingIndex - openingIndex - 1);

        string[] attributes = attributesStr.Split(',');

        for (int i = 0; i < attributes.Length; i++)
        {
            string attributeName = attributes[i].Split('=')[0];
            string attributeValue = attributes[i].Split('=')[1];

            MethodInfo theMethod = dialogueAttributes.GetType().GetMethod(attributeName);

            result.Add(attributeName, theMethod?.Invoke(dialogueAttributes, new object[] { ResolveParameter(theMethod.GetParameters()[0], attributeValue) }));
        }

        dialogueText = cacheText.Remove(openingIndex, closingIndex - openingIndex + 1);

        return result;
    }

    private static object[] ResolveParameters(ParameterInfo[] _params, string[] values)
    {
        object[] objValues = new object[_params.Length];

        for (int i = 0; i < _params.Length; i++)
        {
            objValues[i] = ResolveParameter(_params[i], values[i]);
        }

        return objValues;
    }

    private static object ResolveParameter(ParameterInfo _param, string value)
    {
        object objValue;
        if (_param.ParameterType.Equals(typeof(EntityReference)))
            objValue = EntityManager.Instance.GetEntityRefNullable(value);
        else if (_param.ParameterType.Equals(typeof(int)))
            objValue = int.Parse(value);
        else if (_param.ParameterType.Equals(typeof(float)))
            objValue = float.Parse(value);
        else
            objValue = value;

        return objValue;
    }
}
