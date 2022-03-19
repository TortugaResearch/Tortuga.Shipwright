﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Tortuga.Shipwright;

/// <summary>
/// Class TraitGenerator.
/// Implements the <see cref="Microsoft.CodeAnalysis.ISourceGenerator" />
/// </summary>
/// <seealso cref="Microsoft.CodeAnalysis.ISourceGenerator" />
[Generator]
public class TraitGenerator : ISourceGenerator
{
    /// <summary>
    /// Called to perform source generation. A generator can use the <paramref name="context" />
    /// to add source files via the <see cref="M:Microsoft.CodeAnalysis.GeneratorExecutionContext.AddSource(System.String,Microsoft.CodeAnalysis.Text.SourceText)" />
    /// method.
    /// </summary>
    /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.GeneratorExecutionContext" /> to add source to</param>
    /// <remarks>This call represents the main generation step. It is called after a <see cref="T:Microsoft.CodeAnalysis.Compilation" /> is
    /// created that contains the user written code.
    /// A generator can use the <see cref="P:Microsoft.CodeAnalysis.GeneratorExecutionContext.Compilation" /> property to
    /// discover information about the users compilation and make decisions on what source to
    /// provide.</remarks>
    public void Execute(GeneratorExecutionContext context)
    {
        // retrieve the populated receiver 
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
            return;

        //Write the source code.
        foreach (var workItem in receiver.WorkItems.Where(wi => wi.TraitClasses.Any()))
        {
            try
            {
                var fileName = workItem.ContainerClass.FullMetadataName().Replace('`', '.') + ".cs";
                var code = new CodeWriter();

                code.AppendLine("//This file was generated by Tortuga Shipwright");


                //Determine if we need a Register Traits method.

                var partialProperties = workItem.TraitClasses.SelectMany(m => m.GetMembers()).OfType<IPropertySymbol>().Where(m => m.HasAttribute<PartialAttribute>()).ToList();


                var ownerProperties = workItem.TraitClasses.SelectMany(m => m.GetMembers()).OfType<IPropertySymbol>().Where(m => m.HasAttribute<ContainerAttribute>()).ToList();

                bool useRegisterTraits = partialProperties.Any() || ownerProperties.Any();

                //receiver.Log.Add($"Working on {workItem.HostingClass.Name}");
                //receiver.Log.Add($"Found {partialProperties.Count} partial properties.");
                //receiver.Log.Add($"Found {ownerProperties.Count} owner properties.");

                //Find the list of interfaces
                var interfacesNamesA = workItem.TraitClasses.SelectMany(wi => wi.AllInterfaces);
                var interfacesNamesB = workItem.TraitClasses.SelectMany(x => x.GetMembers()).OfType<IPropertySymbol>()
                    .Where(x => (bool)(x.GetAttribute<ContainerAttribute>()?.NamedArguments.SingleOrDefault(x => x.Key == "RegisterInterface").Value.Value ?? false)).Select(x => x.Type).OfType<INamedTypeSymbol>();

                var interfacesNames = interfacesNamesA.Concat(interfacesNamesB)
                    .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default).Select(i => i.FullName().NotNullType()).ToList();
                var interfaceString = interfacesNames.Any() ? ": " + string.Join(", ", interfacesNames) : "";


                var traitFieldNames = workItem.TraitClasses.Select((Trait, Index) => (Trait, Index))
                    .ToDictionary(item => item.Trait, item => "__Trait" + item.Index, (IEqualityComparer<INamedTypeSymbol>)SymbolEqualityComparer.Default);


                code.AppendLine();
                using (code.BeginScope($"namespace {workItem.ContainerClass.FullNamespace()}"))
                {
                    using (code.BeginScope($"partial class {workItem.ContainerClass.Name}{interfaceString}"))
                    {
                        code.AppendLine();
                        if (useRegisterTraits)
                        {
                            code.AppendLine("private bool __TraitsRegistered;");
                            code.AppendLine();
                        }

                        CreateFieldsForHoldingTraits(code, useRegisterTraits, traitFieldNames);

                        ExplicitlyImplementInterfaces(receiver, workItem, code, traitFieldNames);

                        ExposePublicMembers(receiver, workItem, code, traitFieldNames);

                        CreatePartialMethods(code, partialProperties);


                        if (useRegisterTraits)
                            CreateRegisterTraitsFunction(workItem, code, traitFieldNames);
                    }
                }

                context.AddSource(fileName, SourceText.From(code.ToString(), Encoding.UTF8));
            }
            catch (Exception ex)
            {
                receiver.Log.Add("Error executing generator: " + ex.ToString());
            }
        }

        //Write the log entries
        context.AddSource("Logs", SourceText.From($@"/*{ Environment.NewLine + string.Join(Environment.NewLine, receiver.Log) + Environment.NewLine}*/", Encoding.UTF8));

    }

    private static void CreateRegisterTraitsFunction(WorkItem workItem, CodeWriter code, Dictionary<INamedTypeSymbol, string> traitFieldNames)
    {
        using (code.BeginScope("private void __RegisterTraits()"))
        {
            code.AppendLine("__TraitsRegistered = true;");
            foreach (var traitClass in workItem.TraitClasses)
            {
                foreach (var partialProperty in traitClass.GetMembers()
                        .OfType<IPropertySymbol>().Where(m => m.HasAttribute<PartialAttribute>()))
                    code.AppendLine($"{traitFieldNames[traitClass]}.{partialProperty.Name} = {partialProperty.Name};");


                foreach (var containerProperty in traitClass.GetMembers()
                        .OfType<IPropertySymbol>().Where(m => m.HasAttribute<ContainerAttribute>()))
                {
                    if (containerProperty.GetAttribute<ContainerAttribute>()!.GetNamedArgument("IsOptional", false))
                        code.AppendLine($"{traitFieldNames[traitClass]}.{containerProperty.Name} = this as {containerProperty.Type.TryFullName().NotNullType()};");
                    else
                        code.AppendLine($"{traitFieldNames[traitClass]}.{containerProperty.Name} = this;");
                }
            }
        }
        code.AppendLine();


    }

    private static void CreateFieldsForHoldingTraits(CodeWriter code, bool useRegisterTraits, Dictionary<INamedTypeSymbol, string> traitFieldNames)
    {
        //List the trait fields;
        code.AppendLine("// These fields and/or properties hold the traits. They should not be referenced directly.");
        foreach (var item in traitFieldNames)
        {
            if (useRegisterTraits)
            {
                code.AppendLine($"private {item.Key.FullName()} _{item.Value} = new();");
                using (code.BeginScope($"private {item.Key.FullName()} {item.Value}"))
                {
                    using (code.BeginScope("get"))
                    {
                        code.AppendLine("if (!__TraitsRegistered) __RegisterTraits();");
                        code.AppendLine($"return _{item.Value};");
                    }
                }
            }
            else
            {
                code.AppendLine($"private {item.Key.FullName()} {item.Value} = new();");
            }
        }
        code.AppendLine();
    }

    private static void CreatePartialMethods(CodeWriter code, List<IPropertySymbol> partialProperties)
    {
        // Declare the partial methods
        if (partialProperties.Any())
        {
            var usedPartialMethodNames = new Dictionary<string, string>();

            foreach (var propertySymbol in partialProperties.OrderBy(p => p.Name))
            {
                var propertyType = (INamedTypeSymbol)propertySymbol.Type;
                var propertyTypeName = propertyType.FullName();
                var isAction = propertyTypeName.StartsWith("System.Action<");
                var isFunc = propertyTypeName.StartsWith("System.Func<");
                if (!isAction && !isFunc)
                {
                    code.AppendLine($"// [Partial] property {propertySymbol.Name} must be a Action or Func in order to generate the matching partial method. Found {propertyTypeName} instead.");
                    code.AppendLine();
                    continue;
                }

                if (usedPartialMethodNames.ContainsKey(propertySymbol.Name))
                {
                    code.AppendLine($"// Reusing the previously declared partial method named {propertySymbol.Name} declared on trait {usedPartialMethodNames[propertySymbol.Name]}");
                    code.AppendLine();
                    continue;
                }

                usedPartialMethodNames.Add(propertySymbol.Name, propertySymbol.ContainingType.FullName());

                var propertyCount = isAction ? propertyType.Arity : propertyType.Arity - 1;

                var partialAttribute = propertySymbol.GetAttribute<PartialAttribute>()!;
                var parameterNameString = (string?)partialAttribute.ConstructorArguments[0].Value;

                var parameterNames = parameterNameString?.Split(',').Select(s => s.Trim())
                    .Where(s => s != "").ToList() ?? new();

                for (int i = 0; i < propertyCount; i++)
                {
                    if (parameterNames.Count <= i)
                        parameterNames.Add($"arg{i}");
                }

                var parameters = new List<string>();
                for (int i = 0; i < propertyCount; i++)
                {
                    parameters.Add($"{propertyType.TypeArguments[i].TryFullName()} {parameterNames[i]}");
                }

                var returnType = isAction ? "void" : propertyType.TypeArguments.Last().TryFullName();

                code.AppendLine($"private partial {returnType} {propertySymbol.Name}({string.Join(", ", parameters) } );");
                code.AppendLine();
            }
        }
    }

    private static void ExposePublicMembers(SyntaxReceiver receiver, WorkItem workItem, CodeWriter code, Dictionary<INamedTypeSymbol, string> traitFieldNames)
    {
        if (receiver == null)
            throw new ArgumentNullException(nameof(receiver), $"{nameof(receiver)} is null.");

        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem), $"{nameof(workItem)} is null.");

        if (code == null)
            throw new ArgumentNullException(nameof(code), $"{nameof(code)} is null.");

        if ((traitFieldNames == null) || (traitFieldNames.Count == 0))
            throw new ArgumentException($"{nameof(traitFieldNames)} is null or empty.", nameof(traitFieldNames));

        //Expose the trait methods, properties, and events.
        foreach (var traitClass in workItem.TraitClasses.OrderBy(wi => wi.Name))
        {
            var fieldReference = traitFieldNames[traitClass];

            code.AppendLine($"// Exposing trait {traitClass.FullName()}");
            code.AppendLine();

            foreach (var member in traitClass.GetMembers().Where(m => m.HasAttribute<ExposeAttribute>()).OrderBy(m => m.Name))
            {
                var exposeAttribute = member.GetAttribute<ExposeAttribute>()!;

                //receiver.Log.Add("Inheritance value type = " + exposeAttribute.NamedArguments.SingleOrDefault(x => x.Key == "Inheritance").Value.Value?.GetType().FullName);

                var accessibilityOption = exposeAttribute.GetNamedArgument("Accessibility", Accessibility.Public);
                var inheritanceOption = exposeAttribute.GetNamedArgument("Inheritance", Inheritance.None);

                var accessibility = accessibilityOption switch
                {
                    Accessibility.Public => "public ",
                    Accessibility.Protected => "protected ",
                    Accessibility.Internal => "internal ",
                    Accessibility.ProtectedOrInternal => "protected internal ",
                    Accessibility.Private => "private ",
                    _ => throw new NotImplementedException($"Unknown accessibility value {accessibilityOption}")
                };

                var inheritance = inheritanceOption switch
                {
                    Inheritance.None => "",
                    Inheritance.Abstract => "abstract ",
                    Inheritance.AbstractOverride => "abstract override ",
                    Inheritance.Override => "override ",
                    Inheritance.Virtual => "virtual ",
                    Inheritance.Sealed => "sealed ",
                    Inheritance.SealedOverride => "sealed override ",
                    _ => throw new NotImplementedException($"Unknown inheritance value {inheritanceOption}")
                };

                bool isAbstract = inheritanceOption.HasFlag(Inheritance.Abstract);

                switch (member)
                {
                    case IMethodSymbol methodSymbol:
                        {

                            var returnType = (methodSymbol.ReturnsVoid) ? "void" : methodSymbol.ReturnType.TryFullName();
                            var parameters = string.Join(", ",
                                methodSymbol.Parameters.Select(p => ParameterWithDefault(p)));

                            var signature = $"{accessibility}{inheritance}{returnType} {methodSymbol.FullName()}({parameters}){methodSymbol.TypeConstraintString()}";

                            var invokerPrefix = (methodSymbol.ReturnsVoid) ? "" : "return ";
                            var arguments = string.Join(", ", methodSymbol.Parameters.Select(p => p.Name));
                            var invoker = $"{invokerPrefix}{fieldReference}.{methodSymbol.FullName()}({arguments});";

                            code.AppendMultipleLines(member.GetXmlDocs());

                            if (isAbstract)
                            {
                                code.AppendLine(signature + " ;");
                            }
                            else
                            {
                                using (code.BeginScope(signature))
                                {
                                    code.AppendLine(invoker);
                                }
                            }

                            code.AppendLine();
                        }
                        break;

                    case IPropertySymbol propertySymbol:
                        {
                            var propertyType = (INamedTypeSymbol)propertySymbol.Type;
                            var propertyTypeName = propertyType.FullName();

                            var getterOption = (Getter)(exposeAttribute.NamedArguments.SingleOrDefault(x => x.Key == "Getter").Value.Value ?? Getter.None)
                                ;
                            var setterOption = (Setter)(exposeAttribute.NamedArguments.SingleOrDefault(x => x.Key == "Setter").Value.Value ?? Setter.None);


                            var signature = $"{accessibility} {inheritance} {propertyTypeName} {propertySymbol.Name}";

                            code.AppendMultipleLines(member.GetXmlDocs());
                            using (code.BeginScope(signature))
                            {
                                if (propertySymbol.CanRead())
                                {

                                    var getAccessibility = getterOption switch
                                    {
                                        Getter.Protected => "protected ",
                                        Getter.Internal => "internal ",
                                        Getter.ProtectedOrInternal => "protected internal ",
                                        Getter.Private => "private ",
                                        _ => ""
                                    };

                                    code.AppendLine($"{getAccessibility}get => {fieldReference}.{propertySymbol.Name};");
                                }
                                if (propertySymbol.CanWrite())
                                {
                                    if (propertySymbol.SetMethod!.IsInitOnly)
                                    {
                                        code.AppendLine("// Cannot expose an `init` property.");
                                        receiver.Log.Add("// Cannot expose an `init` property.");
                                        continue; //Move onto the next member.
                                    }

                                    var setType = (setterOption.HasFlag(Setter.Init)) ? "init" : "set";

                                    var setAccessibility = (setterOption & (Setter)7) switch
                                    {
                                        Setter.Protected => "protected ",
                                        Setter.Internal => "internal ",
                                        Setter.ProtectedOrInternal => "protected internal ",
                                        Setter.Private => "private ",
                                        _ => ""
                                    };

                                    code.AppendLine($"{setAccessibility}{setType} => {fieldReference}.{propertySymbol.Name} = value;");
                                }
                            }
                        }
                        break;

                    case IEventSymbol eventSymbol:

                        {
                            var eventType = (INamedTypeSymbol)eventSymbol.Type;
                            var eventTypeName = eventType.FullName();


                            var signature = $"{accessibility}{inheritance}event {eventTypeName} {eventSymbol.Name}";

                            if (isAbstract)
                                code.AppendLine(signature + ";");
                            else
                                using (code.BeginScope(signature))
                                {
                                    code.AppendLine($"add {{ {fieldReference}.{eventSymbol.Name} += value;}}");
                                    code.AppendLine($"remove {{ {fieldReference}.{eventSymbol.Name} -= value;}}");
                                }
                        }

                        break;

                    default:
                        receiver.Log.Add($"Unable to process exposed trait member of type {member.GetType().FullName} named {member.Name}");
                        break;
                }
            }



        }
    }

    static void ExplicitlyImplementInterfaces(SyntaxReceiver receiver, WorkItem workItem, CodeWriter code, Dictionary<INamedTypeSymbol, string> traitFieldNames)
    {
        //Map the interfaces to traits
        foreach (var interfaceType in workItem.TraitClasses.SelectMany(wi => wi.AllInterfaces).Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default).OrderBy(i => i.Name))
        {
            code.AppendLine("// Explicit interface implementation " + interfaceType.FullName());
            var matchingTrait = workItem.TraitClasses.First(t => t.AllInterfaces.Contains(interfaceType, SymbolEqualityComparer.Default));

            foreach (var member in interfaceType.GetMembers().OrderBy(m => m.Name))
            {
                var fieldReference = $"(({interfaceType.FullName()}){traitFieldNames[matchingTrait]})";

                switch (member)
                {
                    //We need to filter this so it doesn't pick up property and event methods.
                    case IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.Ordinary:
                        {
                            var returnType = (methodSymbol.ReturnsVoid) ? "void" : methodSymbol.ReturnType.TryFullName();
                            var parameters = string.Join(", ",
                                methodSymbol.Parameters.Select(p => $"{p.Type.TryFullName()} {p.Name}"));

                            var signature = $"{returnType} {interfaceType.FullName()}.{methodSymbol.FullName()}({parameters})";
                            var invokerPrefix = (methodSymbol.ReturnsVoid) ? "" : "return ";
                            var arguments = string.Join(", ", methodSymbol.Parameters.Select(p => p.Name));
                            //We cast to the interface type in case it is explicitly implemented.
                            var invoker = $"{invokerPrefix}{fieldReference}.{methodSymbol.FullName()}({arguments});";

                            using (code.BeginScope(signature))
                            {
                                code.AppendLine(invoker);
                            }
                            code.AppendLine();

                        }
                        break;

                    case IPropertySymbol propertySymbol:
                        //Task-2 Properties in exposed interfaces
                        {

                            var propertyType = (INamedTypeSymbol)propertySymbol.Type;
                            var propertyTypeName = propertyType.FullName();


                            var signature = $"{propertyTypeName} {interfaceType.FullName()}.{propertySymbol.Name}";

                            code.AppendMultipleLines(member.GetXmlDocs());
                            using (code.BeginScope(signature))
                            {
                                if (propertySymbol.CanRead())
                                {
                                    code.AppendLine($"get => {fieldReference}.{propertySymbol.Name};");
                                }
                                if (propertySymbol.CanWrite())
                                {
                                    //Init properties don't work for interfaces on traits. We can't create an init wrapper around them that will satisfy the compiler.
                                    if (propertySymbol.SetMethod!.IsInitOnly)
                                        continue; //Move onto the next member.

                                    code.AppendLine($"set => {fieldReference}.{propertySymbol.Name} = value;");
                                }
                            }

                        }
                        break;

                    case IEventSymbol eventSymbol:
                        {
                            var eventType = (INamedTypeSymbol)eventSymbol.Type;
                            var eventTypeName = eventType.FullName();


                            var signature = $"event {eventTypeName} {interfaceType.FullName()}.{eventSymbol.Name}";

                            using (code.BeginScope(signature))
                            {
                                code.AppendLine($"add {{ {fieldReference}.{eventSymbol.Name} += value;}}");
                                code.AppendLine($"remove {{ {fieldReference}.{eventSymbol.Name} -= value;}}");
                            }
                        }
                        break;

                    default:
                        receiver.Log.Add($"Unable to process interface member of type {member.GetType().FullName} named {interfaceType.FullName()}.{member.Name}");
                        break;
                }
            }
        }
    }

    static string ParameterWithDefault(IParameterSymbol parameter)
    {
        if (parameter.IsParams)
        {
            return $"params {parameter.Type.TryFullName()} {parameter.Name}";
        }

        if (parameter.HasExplicitDefaultValue)
        {
            switch (parameter.ExplicitDefaultValue)
            {
                case null:
                    return $"{parameter.Type.TryFullName()} {parameter.Name} = default";
                case string sValue:
                    var sEscaped = sValue.Replace("\"", "\"\"");
                    return $"{parameter.Type.TryFullName()} {parameter.Name} = @\"{sEscaped}\"";
                case true:
                    return $"{parameter.Type.TryFullName()} {parameter.Name} = true";
                case false:
                    return $"{parameter.Type.TryFullName()} {parameter.Name} = false";

                default: //probably a numeric type
                    return $"{parameter.Type.TryFullName()} {parameter.Name} = {parameter.ExplicitDefaultValue?.ToString() ?? "default"}";
            }
        }
        else
            return $"{parameter.Type.TryFullName()} {parameter.Name}";
    }

    /// <summary>
    /// Called before generation occurs. A generator can use the <paramref name="context" />
    /// to register callbacks required to perform generation.
    /// </summary>
    /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.GeneratorInitializationContext" /> to register callbacks on</param>
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
