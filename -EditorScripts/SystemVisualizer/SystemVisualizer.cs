/// 5argon - Exceed7 Experiments

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;

using UnityEditor;

public class SystemVisualizer : EditorWindow
{
    [MenuItem("Window/System Visualizer")]
    static void OpenWindow()
    {
        SystemVisualizer window = (SystemVisualizer)EditorWindow.GetWindow(typeof(SystemVisualizer));
        window.titleContent = new GUIContent("Systems");
        window.Init();
        window.Show();
    }

    Dictionary<string, SystemVisualizerNode> linkDict;
    List<SystemVisualizerNode> rootNodes;

    public void Init()
    {
        MakeLinkDictRootNodes(out linkDict, out rootNodes);
        //rects = new Rect[dependencies.Count];
        style = new GUIStyle(EditorStyles.toolbarTextField);
        style.alignment = TextAnchor.MiddleCenter;
        SystemVisualizerNode.depthMaxWidth.Clear();
        SystemVisualizerNode.depthCurrentOrder.Clear();
        foreach(var n in rootNodes)
        {
            n.Traverse(linkDict);
        }
        foreach(var n in linkDict.Values)
        {
            n.UpdateRect();
        }

    }

    public void OnEnable()
    {
        Init();
    }

    GUIStyle style;
    float scrollValue;

    void OnGUI()
    {
        scrollValue = GUI.HorizontalScrollbar(new Rect(0, position.height-20, position.width, 20), scrollValue, 500, 0, 2000);
        BeginWindows();
        foreach(var nodes in linkDict.Values)
        {
            GUI.color = Color.yellow;
            GUI.Window(nodes.id, new Rect( nodes.rect.x - scrollValue, nodes.rect.y, nodes.rect.width, nodes.rect.height), DrawNodeWindow, nodes.text, style);
            foreach (var n in rootNodes)
            {
                n.TraverseDrawLine(linkDict,scrollValue);
            }
        }
        EndWindows();
    }

    void DrawNodeWindow(int id)
    {
        GUI.DragWindow();
    }

    public static void DrawNodeCurve(Rect start, Rect end, float scroll)
    {
        Vector3 startPos = new Vector3(start.x + start.width - scroll, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x- scroll, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 0.5f);
    }

    private class SystemVisualizerNode
    {
        public ScriptBehaviourUpdateOrder.DependantBehavior dependant;
        public Rect rect;
        public int depth;
        public int orderInDepth;
        public int textWidth => WidthOf(dependant.Manager.ToString());
        public string text => this.dependant.Manager.ToString();
        public bool root;
        public int id;
        private static int runningId;

        public SystemVisualizerNode(ScriptBehaviourUpdateOrder.DependantBehavior dependant)
        {
            this.dependant = dependant;
            this.root = dependant.UpdateAfter.Count == 0;
            this.id = runningId;
            this.depth = 0;
            runningId++;
        }

        public void UpdateRect()
        {
            int xNow = 0;
            for (int i = 0; i < this.depth; i++)
            {
                xNow += depthMaxWidth[i];
            }

            this.rect = new Rect(xNow, orderInDepth * 20, textWidth, 20);
        }

        public static Dictionary<int, int> depthMaxWidth = new Dictionary<int, int>();
        public static Dictionary<int, int> depthCurrentOrder= new Dictionary<int, int>();

        public void TraverseDrawLine(Dictionary<string, SystemVisualizerNode> linkDict, float scroll)
        {
            foreach (var next in this.dependant.UpdateBefore)
            {
                var nextNode = linkDict[next.FullName];
                SystemVisualizer.DrawNodeCurve(this.rect, nextNode.rect,scroll);
                nextNode.TraverseDrawLine(linkDict,scroll);
            }
        }

        public void Traverse(Dictionary<string, SystemVisualizerNode> linkDict)
        {
            int maxDepth;
            if (depthMaxWidth.TryGetValue(this.depth, out maxDepth))
            {
                if (textWidth > maxDepth)
                {
                    depthMaxWidth.Remove(this.depth);
                    depthMaxWidth.Add(this.depth, textWidth);
                }
            }
            else
            {
                depthMaxWidth.Add(this.depth, textWidth);
            }

            int currentOrder;
            if (depthCurrentOrder.TryGetValue(this.depth, out currentOrder))
            {
                this.orderInDepth = currentOrder + 1;
                depthCurrentOrder.Remove(this.depth);
                depthCurrentOrder.Add(this.depth, this.orderInDepth);
            }
            else
            {
                depthCurrentOrder.Add(this.depth, 0);
                this.orderInDepth = 0;
            }

            foreach (var next in this.dependant.UpdateBefore)
            {
                var nextNode = linkDict[next.FullName];
                nextNode.depth = this.depth + 1;
                nextNode.Traverse(linkDict);
            }
        }

        private int WidthOf(string text)
        {
            int maxX = 0;
            EditorStyles.toolbarTextField.font.RequestCharactersInTexture(text, EditorStyles.toolbarTextField.fontSize, FontStyle.Normal);
            foreach (char c in text)
            {
                CharacterInfo ci;
                if (EditorStyles.toolbarTextField.font.GetCharacterInfo(c, out ci))
                {
                    maxX += ci.maxX;
                }
            }
            return maxX;
        }
    }

    private static void MakeLinkDictRootNodes(out Dictionary<string, SystemVisualizerNode> linkDict, out List<SystemVisualizerNode> rootNodes)
    {
        var deps = GetAllDepsList();
        rootNodes = new List<SystemVisualizerNode>();
        linkDict = new Dictionary<string, SystemVisualizerNode>();
        foreach (var d in deps)
        {
            var newNode = new SystemVisualizerNode(d);
            if (newNode.root)
            {
                rootNodes.Add(newNode);
            }
            linkDict.Add(d.Manager.ToString(), newNode);
        }
    }

    static List<ScriptBehaviourUpdateOrder.DependantBehavior> GetAllDepsList()
    {
        var world = new World("Visualizer");
        IEnumerable<Type> allTypes;
        foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                allTypes = ass.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                allTypes = e.Types.Where(t => t != null);
                Debug.LogWarning("DefaultWorldInitialization failed loading assembly: " + ass.Location);
            }

            // Create all ComponentSystem
            CreateBehaviourManagersForMatchingTypes(false, allTypes, world);
        }

        var method = typeof(ScriptBehaviourUpdateOrder).GetMethod("CreateSystemDependencyList", BindingFlags.NonPublic | BindingFlags.Static);
        Type insertionBucketType = typeof(ScriptBehaviourUpdateOrder).GetNestedType("InsertionBucket", BindingFlags.NonPublic);
        var systems = insertionBucketType.GetField("Systems", BindingFlags.Public | BindingFlags.Instance);
        var insertPos = insertionBucketType.GetField("InsertPos", BindingFlags.Public | BindingFlags.Instance);
        var insertPos2 = insertionBucketType.GetField("InsertSubPos", BindingFlags.Public | BindingFlags.Instance);
        var bucket = (IEnumerable<object>)method.Invoke(null, new object[] { world.BehaviourManagers, PlayerLoop.GetDefaultPlayerLoop() });

        List<ScriptBehaviourUpdateOrder.DependantBehavior> all = new List<ScriptBehaviourUpdateOrder.DependantBehavior>();
        foreach (object obj in bucket)
        {
            var returnSystems = (List<ScriptBehaviourUpdateOrder.DependantBehavior>)systems.GetValue(obj);
            var returnInsertPos = (int)insertPos.GetValue(obj);
            var returnInsertSubPos = (int)insertPos2.GetValue(obj);
            all.AddRange(returnSystems);
        }
        return all;
    }

    static void CreateBehaviourManagersForMatchingTypes(bool editorWorld, IEnumerable<Type> allTypes, World world)
    {
        var systemTypes = allTypes.Where(t =>
            t.IsSubclassOf(typeof(ComponentSystemBase)) &&
            !t.IsAbstract &&
            !t.ContainsGenericParameters &&
            t.GetCustomAttributes(typeof(DisableAutoCreationAttribute), true).Length == 0);
        foreach (var type in systemTypes)
        {
            if (editorWorld && type.GetCustomAttributes(typeof(ExecuteInEditMode), true).Length == 0)
                continue;

            GetBehaviourManagerAndLogException(world, type);
        }
    }

    static void GetBehaviourManagerAndLogException(World world, Type type)
    {
        try
        {
            world.GetOrCreateManager(type);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}