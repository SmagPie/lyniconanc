﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lynicon.Repositories;
using Lynicon.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Lynicon.Services;

namespace Lynicon.Extensibility
{
    /// <summary>
    /// Base class with shared functionality for all modules
    /// </summary>
    public abstract class Module
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Module));

        /// <summary>
        /// Name of the module, must be unique
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Names of modules which must be registered for this one to function
        /// </summary>
        public List<string> DependentOn { get; set; }
        /// <summary>
        /// Names of modules which must be registered before this one (if they are registered at all)
        /// </summary>
        public List<string> MustFollow { get; set; }
        /// <summary>
        /// Names of modules which must be registered after this one (if they are registered at all)
        /// </summary>
        public List<string> MustPrecede { get; set; }
        /// <summary>
        /// Names of modules which cannot operate at the same time as this one
        /// </summary>
        public List<string> IncompatibleWith { get; set; }
        /// <summary>
        /// If true, the module was blocked from starting up and is not operational
        /// </summary>
        public bool Blocked { get; set; }
        /// <summary>
        /// View name of management panel view for this module
        /// </summary>
        public string ManagerView { get; set; }
        /// <summary>
        /// Error which stopped the module initialising
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// Whether the module applies to a given content or container type
        /// </summary>
        public Func<Type, bool> AppliesToType { get; set; }
        /// <summary>
        /// List of type the module never applies to (overrides AppliesToType)
        /// </summary>
        public List<Type> NeverAppliesTo { get; set; }
        /// <summary>
        /// The event hub manager which controls events for the module
        /// </summary>
        public LyniconSystem System { get; set; }

        /// <summary>
        /// Create a module supplying its name and the names of any modules on which it is dependent.
        /// </summary>
        /// <param name="name">Name of the module</param>
        /// <param name="dependentOn">Names (if any) of modules on which it is dependent</param>
        public Module(LyniconSystem system, string name, params string[] dependentOn)
        {
            Name = name;
            System = system;
            DependentOn = dependentOn == null ? new List<string>() : dependentOn.ToList();
            MustFollow = DependentOn;
            MustPrecede = new List<string>();
            IncompatibleWith = new List<string>();
            Blocked = false;
            ManagerView = null;
            Error = null;
            AppliesToType = t => true;
            NeverAppliesTo = new List<Type>();
        }

        /// <summary>
        /// Called to allow the module to register any routes it needs
        /// </summary>
        public virtual void MapRoutes(IRouteBuilder builder)
        { }

        /// <summary>
        /// Called to initialise the module
        /// </summary>
        /// <returns>True if the module initialised successfully, false if not (in which case the module will be blocked)</returns>
        public abstract bool Initialise();

        /// <summary>
        /// Get a ModuleAdminViewModel to pass into the view for rendering the module's status and operations in the Admin page
        /// </summary>
        /// <returns>A ModuleAdminViewModel describing the module</returns>
        public virtual ModuleAdminViewModel GetViewModel()
        {
            return new ModuleAdminViewModel { Title = this.Name };
        }

        /// <summary>
        /// Called to allow the module to perform any shutdown actions when the site is shutting down
        /// </summary>
        public virtual void Shutdown()
        { }

        /// <summary>
        /// Whether the module should process a given content or container type
        /// </summary>
        /// <param name="t">The content or container type</param>
        /// <returns>Whether the module processes it</returns>
        public bool CheckType(Type t)
        {
            return AppliesToType(t) && !NeverAppliesTo.Contains(t);
        }
    }
}
