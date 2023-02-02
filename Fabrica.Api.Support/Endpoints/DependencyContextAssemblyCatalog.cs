/*

MIT License

Copyright (c) 2017 Jonathan Channon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 
*/


namespace Fabrica.Api.Support.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

public class DependencyContextAssemblyCatalog
{

    private static readonly string carterAssemblyName;

    private readonly DependencyContext dependencyContext;

    private readonly Assembly[] overriddenAssemblies = Array.Empty<Assembly>();

    static DependencyContextAssemblyCatalog()
    {
        carterAssemblyName = typeof(EndpointExtensions).Assembly.GetName().Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class,
    /// using <see cref="Assembly.GetEntryAssembly()"/>.
    /// </summary>
    public DependencyContextAssemblyCatalog()
        : this(Assembly.GetEntryAssembly())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class,
    /// using <paramref name="entryAssembly"/>.
    /// </summary>
    public DependencyContextAssemblyCatalog(Assembly entryAssembly)
    {
        dependencyContext = DependencyContext.Load(entryAssembly);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class,
    /// using assemblies passed in to disable dependency scanning.
    /// </summary>
    public DependencyContextAssemblyCatalog(params Assembly[] assemblies)
    {
        overriddenAssemblies = assemblies;
    }

    /// <summary>
    /// Gets all <see cref="Assembly"/> instances in the catalog.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
    public virtual IReadOnlyCollection<Assembly> GetAssemblies()
    {
        var results = new HashSet<Assembly>
        {
            typeof(DependencyContextAssemblyCatalog).Assembly
        };

        if (overriddenAssemblies.Any())
        {
            foreach (var overriddenAssembly in overriddenAssemblies)
            {
                results.Add(overriddenAssembly);
            }
        }
        else
        {
            foreach (var library in dependencyContext.RuntimeLibraries)
            {
                if (IsReferencingCarter(library) || IsReferencingFluentValidation(library))
                {
                    foreach (var assemblyName in library.GetDefaultAssemblyNames(dependencyContext))
                    {
                        results.Add(SafeLoadAssembly(assemblyName));
                    }
                }
            }
        }

        return results;
    }

    private static Assembly SafeLoadAssembly(AssemblyName assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsReferencingCarter(Library library)
    {
        return library.Dependencies.Any(dependency => dependency.Name.Equals(carterAssemblyName));
    }

    private static bool IsReferencingFluentValidation(Library library)
    {
        return false;
    }
}
