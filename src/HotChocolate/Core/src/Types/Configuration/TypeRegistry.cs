using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Utilities;
using static HotChocolate.Utilities.ThrowHelper;

#nullable  enable

namespace HotChocolate.Configuration;

internal sealed class TypeRegistry
{
    private readonly Dictionary<ITypeReference, RegisteredType> _typeRegister = new();
    private readonly Dictionary<ExtendedTypeReference, ITypeReference> _runtimeTypeRefs =
        new(new ExtendedTypeReferenceEqualityComparer());
    private readonly Dictionary<string, ITypeReference> _nameRefs = new(StringComparer.Ordinal);
    private readonly List<RegisteredType> _types = new();
    private readonly ITypeRegistryInterceptor _typeRegistryInterceptor;

    public TypeRegistry(ITypeRegistryInterceptor typeRegistryInterceptor)
    {
        _typeRegistryInterceptor = typeRegistryInterceptor ??
            throw new ArgumentNullException(nameof(typeRegistryInterceptor));
    }

    public int Count => _typeRegister.Count;

    public IReadOnlyList<RegisteredType> Types => _types;

    public IReadOnlyDictionary<ExtendedTypeReference, ITypeReference> RuntimeTypeRefs =>
        _runtimeTypeRefs;

    public IReadOnlyDictionary<string, ITypeReference> NameRefs => _nameRefs;

    public bool IsRegistered(ITypeReference typeReference)
    {
        if (typeReference is null)
        {
            throw new ArgumentNullException(nameof(typeReference));
        }

        if (_typeRegister.ContainsKey(typeReference))
        {
            return true;
        }

        if (typeReference is ExtendedTypeReference clrTypeReference)
        {
            return _runtimeTypeRefs.ContainsKey(clrTypeReference);
        }

        return false;
    }

    public bool TryGetType(
        ITypeReference typeRef,
        [NotNullWhen(true)] out RegisteredType? registeredType)
    {
        if (typeRef is null)
        {
            throw new ArgumentNullException(nameof(typeRef));
        }

        if (typeRef is ExtendedTypeReference clrTypeRef &&
            _runtimeTypeRefs.TryGetValue(clrTypeRef, out var internalRef))
        {
            typeRef = internalRef;
        }

        return _typeRegister.TryGetValue(typeRef, out registeredType);
    }

    public bool TryGetTypeRef(
        ExtendedTypeReference runtimeTypeRef,
        [NotNullWhen(true)] out ITypeReference? typeRef)
    {
        if (runtimeTypeRef is null)
        {
            throw new ArgumentNullException(nameof(runtimeTypeRef));
        }

        return _runtimeTypeRefs.TryGetValue(runtimeTypeRef, out typeRef);
    }

    public bool TryGetTypeRef(
        string typeName,
        [NotNullWhen(true)] out ITypeReference? typeRef)
    {
        typeName.EnsureGraphQLName();

        if (!_nameRefs.TryGetValue(typeName, out typeRef))
        {
            typeRef = Types
                .FirstOrDefault(t => !t.IsExtension && t.Type.Name.EqualsOrdinal(typeName))
                ?.References[0];
        }
        return typeRef is not null;
    }

    public IEnumerable<ITypeReference> GetTypeRefs() => _runtimeTypeRefs.Values;

    public void TryRegister(ExtendedTypeReference runtimeTypeRef, ITypeReference typeRef)
    {
        if (runtimeTypeRef is null)
        {
            throw new ArgumentNullException(nameof(runtimeTypeRef));
        }

        if (typeRef is null)
        {
            throw new ArgumentNullException(nameof(typeRef));
        }

        if (!_runtimeTypeRefs.ContainsKey(runtimeTypeRef))
        {
            _runtimeTypeRefs.Add(runtimeTypeRef, typeRef);
        }
    }

    public void Register(RegisteredType registeredType)
    {
        if (registeredType is null)
        {
            throw new ArgumentNullException(nameof(registeredType));
        }

        var addToTypes = !_typeRegister.ContainsValue(registeredType);

        foreach (var typeReference in registeredType.References)
        {
            if (_typeRegister.TryGetValue(typeReference, out var current) &&
                !ReferenceEquals(current, registeredType))
            {
                if (current.IsInferred && !registeredType.IsInferred)
                {
                    _typeRegister[typeReference] = registeredType;
                    if (!_typeRegister.ContainsValue(current))
                    {
                        _types.Remove(current);
                    }
                }
            }
            else
            {
                _typeRegister[typeReference] = registeredType;
            }
        }

        if (addToTypes)
        {
            _types.Add(registeredType);
            _typeRegistryInterceptor.OnTypeRegistered(registeredType);
        }

        if (!registeredType.IsExtension)
        {
            if (registeredType.IsNamedType &&
                registeredType.Type is IHasTypeDefinition { Definition: { } typeDef } &&
                !_nameRefs.ContainsKey(typeDef.Name))
            {
                _nameRefs.Add(typeDef.Name, registeredType.References[0]);
            }
            else if (registeredType.Kind == TypeKind.Scalar &&
                registeredType.Type is ScalarType scalar)
            {
                _nameRefs.Add(scalar.Name, registeredType.References[0]);
            }
            else if (registeredType.Kind == TypeKind.Directive &&
                registeredType.Type is DirectiveType directive &&
                !_nameRefs.ContainsKey(directive.Definition!.Name))
            {
                _nameRefs.Add(directive.Definition.Name, registeredType.References[0]);
            }
        }
    }

    public void Register(string typeName, ExtendedTypeReference typeReference)
    {
        if (typeReference is null)
        {
            throw new ArgumentNullException(nameof(typeReference));
        }

        typeName.EnsureGraphQLName();

        _nameRefs[typeName] = typeReference;
    }

    public void Register(string typeName, RegisteredType registeredType)
    {
        if (registeredType is null)
        {
            throw new ArgumentNullException(nameof(registeredType));
        }

        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        if (registeredType.IsExtension)
        {
            return;
        }

        if (!registeredType.IsNamedType && !registeredType.IsDirectiveType)
        {
            return;
        }

        if (TryGetTypeRef(typeName, out var typeRef) &&
            TryGetType(typeRef, out var type) &&
            !ReferenceEquals(type, registeredType))
        {
            throw TypeInitializer_DuplicateTypeName(registeredType.Type, type.Type);
        }

        _nameRefs[typeName] = registeredType.References[0];
    }

    public void CompleteDiscovery()
    {
        foreach (var registeredType in _types)
        {
            ITypeReference reference = TypeReference.Create(registeredType.Type);
            registeredType.References.TryAdd(reference);

            _typeRegister[reference] = registeredType;

            if (registeredType.Type.Scope is { } s)
            {
                reference = TypeReference.Create(registeredType.Type, s);
                registeredType.References.TryAdd(reference);

                _typeRegister[reference] = registeredType;
            }
        }
    }
}
