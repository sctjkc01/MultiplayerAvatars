using ModestTree;
using SiraUtil.Logging;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

static class Utils
{
    public static String GetPath(Transform Transform)
    {
        String s = $"{Transform.name}";

        Transform t = Transform.parent;

        while (t != null)
        {
            s = t.name + "/" + s;
        }

        return s;
    }

    public static void ReadEntireScene<T>(SiraLog OutputLogger, Transform FlaggedTransform) where T : MonoBehaviour
    {
        ReadEntireScene(OutputLogger, FlaggedTransform, new Type[] { typeof(T) });
    }

    public static void ReadEntireScene(SiraLog OutputLogger, Transform FlaggedTransform)
    {
        ReadEntireScene(OutputLogger, FlaggedTransform, new Type[] { });
    }

    public static void ReadEntireScene(SiraLog OutputLogger, Transform FlaggedTransform, Type[] RequestedTypes)
    {
        var s = "\n";

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var Scene = SceneManager.GetSceneAt(i);

            var Roots = Scene.GetRootGameObjects();

            foreach (var Root in Roots)
            {
                s = s + RecursiveHeirarchyTrace(RequestedTypes, FlaggedTransform, Root.transform, 0) + "\n";
            }
        }

        OutputLogger.Debug(s);
    }

    private static string RecursiveHeirarchyTrace(Type[] RequestedTypes, Transform FlaggedTransform, Transform Reference, int Level)
    {
        string s = $"{new string(' ', Level * 2)}- {Reference.name}";
        if (Reference == FlaggedTransform)
        {
            s = s + " <<<<<< ";
        }

        var InnerLevel = Level + 1;

        foreach (var Type in RequestedTypes)
        {
            var FoundComponents = Reference.GetComponents(Type);

            if (FoundComponents.Length == 0)
                continue;
            s = s + ($"\n{new string(' ', InnerLevel * 2)}Comp- {Type.FullName}");
        }

        foreach (Transform Child in Reference)
        {
            s = s + "\n" + RecursiveHeirarchyTrace(RequestedTypes, FlaggedTransform, Child, InnerLevel);
        }

        return s;
    }
}
