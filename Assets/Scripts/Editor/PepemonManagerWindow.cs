using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public class PepemonManagerWindow : OdinMenuEditorWindow
{
    [OnValueChanged("OnStateChanged")]
    [LabelText("Manager View")]
    [LabelWidth(100f)]
    [EnumToggleButtons]
    [ShowInInspector]
    private ManagerState managerState;
    private int enumIndex = 0;
    private bool treeRebuild = false;

    private DrawSelected<Card> drawCardData = new DrawSelected<Card>();
    private DrawSelected<Pepemon> drawPepemonData = new DrawSelected<Pepemon>();

    private string cardDataPath = "Assets/ScriptableObjects/Cards";
    private string pepemonDataPath = "Assets/ScriptableObjects/Pepemons";

    public enum ManagerState
    {
        CardData,
        PepemonData
    }

    [MenuItem("Pepemon/Game Manager")]
    private static void Open()
    {
        var window = GetWindow<PepemonManagerWindow>();
        window.position = Sirenix.Utilities.Editor.GUIHelper.GetEditorWindowRect().AlignCenter(900, 700);
        window.titleContent.text = "Game Manager";
    }

    private void OnStateChanged()
    {
        treeRebuild = true;
    }

    protected override void Initialize()
    {
        drawCardData.SetPath(cardDataPath);
        drawPepemonData.SetPath(pepemonDataPath);
    }

    protected override void OnGUI()
    {
        if (treeRebuild && Event.current.type == EventType.Layout)
        {
            ForceMenuTreeRebuild();
            treeRebuild = false;
        }

        SirenixEditorGUI.Title("Pepemon Game Manager", "Pepemon Finance", TextAlignment.Center, true);
        EditorGUILayout.Space();

        switch (managerState)
        {
            case ManagerState.CardData:
            case ManagerState.PepemonData:
                base.DrawEditor(enumIndex);
                break;
            default:
                break;
        }

        EditorGUILayout.Space();

        base.OnGUI();
    }

    protected override void DrawEditors()
    {
        switch (managerState)
        {
            case ManagerState.CardData:
                drawCardData.SetSelected(this.MenuTree.Selection.SelectedValue);
                break;
            case ManagerState.PepemonData:
                drawPepemonData.SetSelected(this.MenuTree.Selection.SelectedValue);
                break;
            default:
                break;
        }

        DrawEditor((int)managerState);
    }

    protected override IEnumerable<object> GetTargets()
    {
        List<object> targets = new List<object>();
        targets.Add(drawCardData);
        targets.Add(drawPepemonData);
        targets.Add(base.GetTarget());

        enumIndex = targets.Count - 1;
        return targets;
    }

    protected override void DrawMenu()
    {
        switch (managerState)
        {
            case ManagerState.CardData:
            case ManagerState.PepemonData:
                base.DrawMenu();
                break;
            default:
                break;
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree();
        tree.DefaultMenuStyle.IconSize = 26.00f;
        tree.Config.DrawSearchToolbar = true;

        switch (managerState)
        {
            case ManagerState.CardData:
                tree.AddAllAssetsAtPath("Cards", cardDataPath, typeof(Card), true);
                tree.EnumerateTree().Where(x => x.Value as Card)
                .ForEach(x => x.Name = (x.Value as Card).DisplayName.IsNullOrWhitespace() ? (x.Value as Card).name : (x.Value as Card).DisplayName);
                tree.EnumerateTree().AddIcons<Card>(x => x.Icon);
                tree.EnumerateTree().ForEach(x => x.OnRightClick += AddContextMenu);

                break;
            case ManagerState.PepemonData:
                tree.AddAllAssetsAtPath("Pepemons", pepemonDataPath, typeof(Pepemon), true);
                tree.EnumerateTree().ForEach(x => x.OnRightClick += AddContextMenu);
                break;
            default:
                break;
        }

        return tree;
    }

    private void AddContextMenu(Sirenix.OdinInspector.Editor.OdinMenuItem item)
    {
        var contextMenu = new UnityEditor.GenericMenu();
        contextMenu.AddItem(new UnityEngine.GUIContent("Ping"), false, DoPing, item.Value);
        contextMenu.ShowAsContext();
    }

    private void DoPing(object selected)
    {
        ScriptableObject newItem = selected as ScriptableObject;
        string tPath = AssetDatabase.GetAssetPath(newItem);
        Selection.activeObject = newItem;
    }
}

public class DrawSelected<T> where T : ScriptableObject
{
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    public T selected;

    [LabelWidth(100)]
    [PropertyOrder(-2)]
    [ColorfoldoutGroup("CreateNew", 0.7f, 0.7f, 1f, 1f)]
    [HorizontalGroup("CreateNew/Horizontal")]
    public string newItemName;

    private string path;

    [ColorfoldoutGroup("CreateNew", 0.7f, 0.7f, 1f, 1f)]
    [HorizontalGroup("CreateNew/Horizontal")]
    [GUIColor(0.7f, 1f, 0.7f)]
    [Button()]
    public void CreateNew()
    {
        if (newItemName.IsNullOrWhitespace())
            return;

        T newItem = ScriptableObject.CreateInstance<T>();
        newItem.name = "NEW + " + typeof(T).ToString();

        string tPath = AssetDatabase.GetAssetPath(selected);
        tPath = tPath.Remove(tPath.LastIndexOf('/'));
        string compiled = tPath + "/" + newItemName + ".asset";

        AssetDatabase.CreateAsset(newItem, compiled);
        AssetDatabase.SaveAssets();

        newItemName = "";
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(compiled, typeof(UnityEngine.Object));
        EditorGUIUtility.PingObject(obj);
    }


    [ColorfoldoutGroup("CreateNew", 0.7f, 0.7f, 1f, 1f)]
    [HorizontalGroup("CreateNew/Horizontal")]
    [GUIColor(1f, 0.7f, 0.7f)]
    [Button("Delete Selected")]
    public void DeleteSelected()
    {
        if (selected != null)
        {
            string _path = AssetDatabase.GetAssetPath(selected);
            if (EditorUtility.DisplayDialog("Ermmm matey?", "Are you sure you wanna delete:  " + selected.name, "Yes! send it to the Gulag", "No! big whoopsie"))
            {
                AssetDatabase.DeleteAsset(_path);
                AssetDatabase.SaveAssets();
            }
        }
    }

    public void SetSelected(object item)
    {
        var attempt = item as T;
        if (attempt != null) this.selected = attempt;
    }

    public void SetPath(string path)
    {
        this.path = path;
    }

}

public class ColorfoldoutGroupAttribute : PropertyGroupAttribute
{
    public float R, G, B, A;

    public ColorfoldoutGroupAttribute(string path, float r, float g, float b, float a = 1f) : base(path)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    protected override void CombineValuesWith(PropertyGroupAttribute other)
    {
        var otherAtt = (ColorfoldoutGroupAttribute)other;
        this.R = Mathf.Max(otherAtt.R, this.R);
        this.G = Mathf.Max(otherAtt.G, this.G);
        this.B = Mathf.Max(otherAtt.B, this.B);
        this.A = Mathf.Max(otherAtt.A, this.A);
    }
}

public class ColorFoldGroupAttributeDrawer : OdinGroupDrawer<ColorfoldoutGroupAttribute>
{
    private bool isOpen;

    protected override void Initialize()
    {
        this.isOpen = true;
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        GUIHelper.PushColor(new Color(this.Attribute.R, this.Attribute.G, this.Attribute.B, this.Attribute.A));
        SirenixEditorGUI.BeginBox();
        GUIHelper.PopColor();

        if (SirenixEditorGUI.BeginFadeGroup(this, this.isOpen))
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                this.Property.Children[i].Draw();
            }
        }
        SirenixEditorGUI.EndFadeGroup();
        SirenixEditorGUI.EndBox();
    }
}