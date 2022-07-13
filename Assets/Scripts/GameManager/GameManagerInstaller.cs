using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class GameManagerInstaller : MonoBehaviour
{
    private void Awake()
    {
        CursorController.HideCursor();
        Assembly assembly = Assembly.Load("Assembly-CSharp");

        Type[] objects = assembly.GetTypes().Where(item => item.GetCustomAttributes(typeof(GameManagerMemberAttribute), false).Length > 0).ToArray();
        List<PropertyInfo> gameplayManagerProperties = typeof(GameManager).GetProperties().ToList();

        foreach (Type t in objects)
        {
            object obj = null;

            foreach (var go in Resources.FindObjectsOfTypeAll(t))
            {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                obj = go;
                break;
            }

            if (obj == null)
            {
                Debug.LogError("There is no suitable class on the scene.");
            }
            else
            {
                PropertyInfo appropiateProperty = null;

                foreach (var property in gameplayManagerProperties)
                {
                    if (property.PropertyType == t)
                    {
                        appropiateProperty = property;
                        break;
                    }
                }

                if (appropiateProperty != null)
                {
                    appropiateProperty.SetValue(null, obj);
                }
            }
        }
    }
}
