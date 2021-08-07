using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DialogueActionParser
{
    public static IEnumerator TryRunAction(string strAction, Action onFinish = null)
    {
        string[] str = strAction.Split('$');

        var dialogueActions = new DialogueActions();
        for (int i = 0; i < str.Length; i++)
        {
            string currentAction = str[i];

            if (currentAction.Length == 0)
                continue;

            if (currentAction.Contains("="))
            {
                string methodName = currentAction.Split('=')[0].Trim();
                string methodArgs = currentAction.Split('=')[1].Trim();
                
                MethodInfo theMethod = dialogueActions.GetType().GetMethod(methodName);
                
                yield return DialogueManager.Instance.StartCoroutine((IEnumerator)theMethod
                                ?.Invoke(dialogueActions, ResolveParameters(theMethod.GetParameters(), methodArgs.Split(','))));
            } else
            {
                var methodName = currentAction;

                MethodInfo theMethod = dialogueActions.GetType().GetMethod(methodName);

                object[] blankParams = new object[0];

                yield return DialogueManager.Instance.StartCoroutine((IEnumerator)theMethod
                                ?.Invoke(dialogueActions, blankParams));
            }


        }

        onFinish?.Invoke();

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
            objValue = EntityManager.Instance.GetEntityRefNullable(value.Trim());
        else if (_param.ParameterType.Equals(typeof(int)))
            objValue = int.Parse(value);
        else if (_param.ParameterType.Equals(typeof(float)))
            objValue = float.Parse(value);
        else
            objValue = value.Trim();

        return objValue;
    }
}
