using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XrcPackageDocs;

/// <summary>
/// The components page, Documentation~/manual/components.md, from the C# under Runtime/: every component with its
/// summary, its Inspector image when one is committed, and a table of the fields the Inspector shows, each described
/// by the XML summary of the public property that exposes it.
/// </summary>
/// <remarks>
/// The sources are read with Roslyn, without Unity's assemblies, so Unity's own types stay unresolved. What the
/// Inspector shows follows from rules that need no editor: the serialized fields of the class and its base classes in
/// this package, base class first, in declaration order, labelled like Unity labels them. A base class outside the
/// package is recognised by name (<see cref="k_ComponentBases"/>); its own fields are documented with that class.
/// </remarks>
internal static class ComponentsPage
{
    // Base classes outside the package whose descendants are components.
    private static readonly HashSet<string> k_ComponentBases = new(StringComparer.Ordinal)
    {
        "MonoBehaviour", "NetworkBehaviour", "UIBehaviour", "Graphic", "MaskableGraphic", "Selectable",
        // XR Interaction Toolkit
        "XRBaseInteractable", "XRGrabInteractable", "XRSimpleInteractable", "XRBaseInteractor", "XRBaseInputInteractor",
        "XRRayInteractor", "XRDirectInteractor", "NearFarInteractor", "XRPokeInteractor", "XRGazeInteractor",
        "XRSocketInteractor", "LocomotionProvider", "TeleportationProvider", "ContinuousMoveProvider",
        "ContinuousTurnProvider", "SnapTurnProvider", "XRInteractionManager", "UIInputModule", "XRUIInputModule",
        "BaseTeleportationInteractable", "TeleportationAnchor", "TeleportationArea",
        // XRC Core
        "BaseEditToolLogic", "BaseEditToolInput", "BaseEditToolFeedback",
    };

    // Base classes outside the package whose descendants are not components.
    private static readonly HashSet<string> k_NonComponentBases = new(StringComparer.Ordinal)
    {
        "Object", "ScriptableObject", "ScriptableSingleton", "StateMachineBehaviour", "PlayableBehaviour", "PlayableAsset",
        "PropertyAttribute", "UnityEvent", "Editor", "EditorWindow", "PropertyDrawer", "DecoratorDrawer",
        "AssetPostprocessor", "AssetModificationProcessor", "ScriptedImporter", "SettingsProvider",
        "InputProcessor", "InputBindingComposite",
        // UI Toolkit
        "VisualElement", "BindableElement", "BaseField", "TextElement", "Label", "Button", "Toggle", "Slider", "SliderInt",
        "TextField", "Foldout", "ScrollView", "ListView", "Image", "Manipulator", "MouseManipulator", "PointerManipulator",
        "Clickable", "UxmlFactory", "UxmlTraits", "UxmlRootElementFactory",
    };

    private static readonly HashSet<string> k_UnityMessages = new(StringComparer.Ordinal)
    {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable", "OnDestroy", "OnValidate",
        "Reset", "OnDrawGizmos", "OnDrawGizmosSelected", "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
        "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay", "OnGUI", "OnApplicationQuit", "OnApplicationPause",
    };

    // Types Unity does not serialize, by name, so a [SerializeField] on them shows nothing in the Inspector.
    private static readonly HashSet<string> k_UnserializedTypes = new(StringComparer.Ordinal)
    {
        "Dictionary", "HashSet", "Queue", "Stack", "LinkedList", "SortedDictionary", "SortedList", "SortedSet",
        "Decimal", "DateTime", "DateTimeOffset", "TimeSpan", "Guid", "IntPtr", "UIntPtr", "Delegate", "MulticastDelegate",
    };

    /// <returns>One entry per component for the overview page: <c>label</c>, <c>anchor</c> and <c>summary</c>, its first sentence.</returns>
    public static IReadOnlyList<IReadOnlyDictionary<string, object?>> Write(string packageRoot, string docsFolder, Report report)
    {
        var entries = new List<IReadOnlyDictionary<string, object?>>();
        var sources = RuntimeSources(packageRoot).ToList();
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse);
        var trees = sources.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parseOptions, path)).ToList();
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Package", trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var models = trees.ToDictionary(tree => tree, tree => compilation.GetSemanticModel(tree));

        var components = trees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .Select(declaration => models[declaration.SyntaxTree].GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .Where(type => IsComponent(type, report))
            .OrderBy(type => type.Name, StringComparer.InvariantCulture)
            .ToList();

        var page = new List<string>
        {
            "# Components",
            "",
            "This document lists all MonoBehaviour components in the package.",
            "",
        };

        if (components.Count == 0)
        {
            report.Warning("no components found under Runtime/");
            page.Add("This package has no MonoBehaviour components.");
            page.Add("");
        }

        var missing = 0;
        var described = 0;
        foreach (var type in components)
        {
            var displayName = Names.Nicify(type.Name);
            page.Add("## " + displayName);
            page.Add("");
            var summary = Summary(type, false, report);
            page.Add(summary ?? "*No summary provided*");
            page.Add("");
            entries.Add(new Dictionary<string, object?>
            {
                ["label"] = displayName,
                ["anchor"] = OverviewPage.Anchor(displayName),
                ["summary"] = FirstSentence(summary),
            });
            if (summary == null)
            {
                report.Missing(type.Name + ": class summary", FileOf(type), Line(type));
                missing++;
            }

            var externalBase = ExternalBase(type);
            if (externalBase != null && externalBase != "MonoBehaviour")
            {
                page.Add("Also has the properties of its base class `" + externalBase + "`.");
                page.Add("");
            }

            if (System.IO.File.Exists(Path.Combine(docsFolder, "images", "components", type.Name + ".png")))
            {
                // An img tag rather than Markdown, so the image can be told to fill the text column.
                page.Add("<img src=\"../images/components/" + type.Name + ".png\" alt=\"" + displayName + "\" style=\"width: 100%;\">");
                page.Add("");
            }

            var rows = new List<string>();
            foreach (var (label, field) in InspectorFields(type))
            {
                string? description = null;
                var property = ExposingProperty(type, field, models);
                if (property != null)
                    description = Summary(property, true, report);
                description ??= Summary(field, true, report);

                if (description == null)
                {
                    var fieldName = field.AssociatedSymbol is IPropertySymbol auto ? auto.Name : field.Name;
                    report.Missing(type.Name + "." + fieldName +
                                   (property != null ? " (property " + property.Name + " has no summary)" : " (no public property reads it)"),
                        FileOf(field), Line(field));
                    missing++;
                }
                else
                {
                    described++;
                }
                rows.Add("| **" + label + "** | " + (description ?? "*No description provided*") + " |");
            }

            if (rows.Count > 0)
            {
                page.Add("| Property | Description |");
                page.Add("|----------|-------------|");
                page.AddRange(rows);
                page.Add("");
            }
        }


        var manual = Path.Combine(docsFolder, "manual");
        Directory.CreateDirectory(manual);
        var pagePath = Path.Combine(manual, "components.md");
        System.IO.File.WriteAllText(pagePath, string.Join("\n", page), new UTF8Encoding(false));
        report.Wrote(pagePath);
        report.Info(components.Count + " components, " + described + " fields described, " + missing + " missing");
        return entries;
    }

    // The first sentence of a summary's first paragraph, for the one-line listing on the overview page.
    private static string FirstSentence(string? summary)
    {
        if (summary == null)
            return "";
        var paragraph = summary.Split("\n\n")[0].Replace('\n', ' ');
        var end = paragraph.IndexOf(". ", StringComparison.Ordinal);
        return end < 0 ? paragraph : paragraph[..(end + 1)];
    }

    // ==== Components =============================================================================================

    // The .cs files of the package's Runtime assembly: everything under Runtime/ except folders that hold their own
    // assembly definition, which Unity compiles separately.
    private static IEnumerable<string> RuntimeSources(string packageRoot)
    {
        var runtime = Path.Combine(packageRoot, "Runtime");
        if (!Directory.Exists(runtime))
            return [];
        return Directory.EnumerateFiles(runtime, "*.cs", SearchOption.AllDirectories)
            .Where(file => !InOwnAssembly(Path.GetDirectoryName(file)!, runtime))
            .OrderBy(file => file, StringComparer.Ordinal);
    }

    private static bool InOwnAssembly(string folder, string runtime)
    {
        for (var current = folder; current != null && current.Length > runtime.Length; current = Path.GetDirectoryName(current))
        {
            if (Directory.EnumerateFiles(current, "*.asmdef").Any() || Directory.EnumerateFiles(current, "*.asmref").Any())
                return true;
        }
        return false;
    }

    // A concrete class that derives from MonoBehaviour, through base classes in this package or a known base class
    // outside it. A base class the lists above do not know is decided by what the class itself looks like, and
    // reported, so the list can be extended.
    private static bool IsComponent(INamedTypeSymbol type, Report report)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic || type.IsGenericType)
            return false;

        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            if (current.TypeKind == TypeKind.Error)
            {
                var name = current.Name;
                if (k_ComponentBases.Contains(name))
                    return true;
                if (k_NonComponentBases.Contains(name))
                    return false;
                var looksLikeComponent = LooksLikeComponent(type);
                report.Warning(type.Name + ": base class " + name + " is not a known Unity type; " +
                               (looksLikeComponent
                                   ? "listed as a component because the class has serialized fields or Unity messages"
                                   : "not listed, since the class has neither serialized fields nor Unity messages"),
                    FileOf(type), Line(type));
                return looksLikeComponent;
            }
            if (current.DeclaringSyntaxReferences.Length == 0)
                return false; // a resolved .NET type such as Exception: not a component
        }
        return false;
    }

    private static bool LooksLikeComponent(INamedTypeSymbol type)
    {
        return SourceChain(type).Any(t =>
            t.GetMembers().OfType<IFieldSymbol>().Any(f => HasAttribute(f, "SerializeField")) ||
            t.GetMembers().OfType<IMethodSymbol>().Any(m => k_UnityMessages.Contains(m.Name)) ||
            HasAttribute(t, "RequireComponent") || HasAttribute(t, "AddComponentMenu") || HasAttribute(t, "DisallowMultipleComponent"));
    }

    // The nearest base class outside the package, by name, or null when the chain ends inside the package.
    private static string? ExternalBase(INamedTypeSymbol type)
    {
        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            if (current.DeclaringSyntaxReferences.Length == 0)
                return current.Name;
        }
        return null;
    }

    // The class and its base classes in this package, the root first: the order the Inspector shows their fields in.
    private static List<INamedTypeSymbol> SourceChain(INamedTypeSymbol type)
    {
        var chain = new List<INamedTypeSymbol>();
        for (var current = type; current != null && current.DeclaringSyntaxReferences.Length > 0; current = current.BaseType)
            chain.Insert(0, current);
        return chain;
    }

    // ==== Fields =================================================================================================

    // The fields the Inspector shows, in its order, with its labels. A property with [field: SerializeField] shows
    // as its backing field, labelled by the property's name.
    private static IEnumerable<(string label, IFieldSymbol field)> InspectorFields(INamedTypeSymbol type)
    {
        foreach (var owner in SourceChain(type))
        {
            var fields = owner.GetMembers().OfType<IFieldSymbol>()
                .Where(IsSerialized)
                .OrderBy(FileOf, StringComparer.Ordinal)
                .ThenBy(Position);
            foreach (var field in fields)
            {
                var name = field.AssociatedSymbol is IPropertySymbol auto ? auto.Name : field.Name;
                yield return (Names.Nicify(name), field);
            }
        }
    }

    private static bool IsSerialized(IFieldSymbol field)
    {
        if (field.IsStatic || field.IsConst || field.IsReadOnly)
            return false;
        if (field.IsImplicitlyDeclared && field.AssociatedSymbol is not IPropertySymbol)
            return false;
        if (HasAttribute(field, "NonSerialized") || HasAttribute(field, "HideInInspector"))
            return false;

        var serializeReference = HasAttribute(field, "SerializeReference");
        var serialized = HasAttribute(field, "SerializeField") || serializeReference
                         || (field.DeclaredAccessibility == Accessibility.Public && !field.IsImplicitlyDeclared);
        if (!serialized)
            return false;

        var type = field.Type;
        if (type is IArrayTypeSymbol array)
        {
            if (array.Rank > 1)
                return false;
            type = array.ElementType;
        }
        else if (type is INamedTypeSymbol { IsGenericType: true, Name: "List" } list && !serializeReference)
        {
            type = list.TypeArguments[0];
        }

        if (type.TypeKind is TypeKind.Delegate or TypeKind.Pointer or TypeKind.FunctionPointer)
            return false;
        if (type.TypeKind == TypeKind.Interface && !serializeReference)
            return false;
        if (type.SpecialType == SpecialType.System_Object && !serializeReference)
            return false;
        return !k_UnserializedTypes.Contains(type.Name);
    }

    // ==== Properties =============================================================================================

    // The public property, here or in a base class, whose getter reads exactly this field.
    private static IPropertySymbol? ExposingProperty(INamedTypeSymbol type, IFieldSymbol field, Dictionary<SyntaxTree, SemanticModel> models)
    {
        if (field.AssociatedSymbol is IPropertySymbol auto)
            return auto.DeclaredAccessibility == Accessibility.Public ? auto : null;

        var chain = SourceChain(type);
        for (var i = chain.Count - 1; i >= 0; i--)
        {
            var properties = chain[i].GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && !p.IsIndexer && p.GetMethod != null)
                .OrderBy(FileOf, StringComparer.Ordinal)
                .ThenBy(Position);
            foreach (var property in properties)
            {
                if (SymbolEqualityComparer.Default.Equals(ReadField(property, models), field))
                    return property;
            }
        }
        return null;
    }

    // The single instance field a getter reads, or null when it reads none or several.
    private static IFieldSymbol? ReadField(IPropertySymbol property, Dictionary<SyntaxTree, SemanticModel> models)
    {
        if (property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not PropertyDeclarationSyntax syntax)
            return null;

        SyntaxNode? body = syntax.ExpressionBody?.Expression;
        if (body == null)
        {
            var getter = syntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getter == null)
                return null;
            body = (SyntaxNode?)getter.Body ?? getter.ExpressionBody?.Expression;
            if (body == null)
                return property.ContainingType.GetMembers().OfType<IFieldSymbol>()
                    .FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));
        }

        var model = models[syntax.SyntaxTree];
        IFieldSymbol? found = null;
        foreach (var identifier in body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(identifier).Symbol is not IFieldSymbol { IsStatic: false, IsConst: false } read)
                continue;
            if (found != null && !SymbolEqualityComparer.Default.Equals(found, read))
                return null;
            found = read;
        }
        return found;
    }

    // ==== XML documentation ======================================================================================

    private static string? Summary(ISymbol symbol, bool inTable, Report report)
    {
        var xml = symbol.GetDocumentationCommentXml(expandIncludes: false);
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        var document = new XmlDocument();
        try
        {
            document.LoadXml(xml);
        }
        catch (XmlException e)
        {
            report.Warning(symbol.Name + ": the XML comment does not parse: " + e.Message, FileOf(symbol), Line(symbol));
            return null;
        }

        var node = document.SelectSingleNode("/member/summary");
        if (node == null)
            return null;
        var text = DocComments.ToMarkdown(node, inTable).Trim();
        return text.Length > 0 ? text : null;
    }

    // ==== Locations ==============================================================================================

    private static bool HasAttribute(ISymbol symbol, string name)
    {
        return symbol.GetAttributes().Any(a =>
        {
            var attributeName = a.AttributeClass?.Name ?? "";
            return attributeName == name || attributeName == name + "Attribute";
        });
    }

    private static string? FileOf(ISymbol symbol) => symbol.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

    private static int Line(ISymbol symbol)
    {
        var location = symbol.Locations.FirstOrDefault(l => l.IsInSource);
        return location == null ? 0 : location.GetLineSpan().StartLinePosition.Line + 1;
    }

    private static int Position(ISymbol symbol) => symbol.Locations.FirstOrDefault(l => l.IsInSource)?.SourceSpan.Start ?? 0;
}
