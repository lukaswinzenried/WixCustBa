//-------------------------------------------------------------------------------------------------
// <copyright file="FlavoredProjectFactoryBase.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.Shell.Flavor
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.ProjectAggregator2.Interop;

    /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase"]/*' />
    /// <devdoc>
    /// The project factory for the project flavor.
    /// Note that this is also known as Project Subtype
    /// </devdoc>
    //[CLSCompliant(false)]
    public abstract class FlavoredProjectFactoryBase : IVsAggregatableProjectFactoryCorrected, IVsProjectFactory
    {
        private ServiceProvider _serviceProvider;
        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.ServiceProvider"]/*' />
        protected ServiceProvider serviceProvider
        {
            get { return _serviceProvider; }
        }

        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.FlavoredProjectFactoryBase"]/*' />
        public FlavoredProjectFactoryBase()
        {
        }

        #region IVsProjectFactory

        int IVsProjectFactory.CanCreateProject(string fileName, uint flags, out int canCreate)
        {
            canCreate = this.CanCreateProject(fileName, flags) ? 1 : 0;
            return VSConstants.S_OK;
        }
        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.CanCreateProject"]/*' />
        /// <devdoc>
        /// This is called to ask the factory if it can create a project based on the current parameters
        /// </devdoc>
        /// <returns>True if the project can be created</returns>
        protected virtual bool CanCreateProject(string fileName, uint flags)
        {
            // Validate the filename
            bool canCreate = !string.IsNullOrEmpty(fileName);
            canCreate |= !PackageUtilities.ContainsInvalidFileNameChars(fileName);
            return canCreate;
        }

        /// <devdoc>
        /// This is not expected to be called unless using an extension other then the base project
        /// </devdoc>
        int IVsProjectFactory.CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out System.IntPtr project, out int canceled)
        {
            this.CreateProject(fileName, location, name, flags, ref projectGuid, out project, out canceled);
            return VSConstants.S_OK;
        }
        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.CreateProject"]/*' />
        /// <devdoc>
        /// If you want to use your own extension, you will need to call IVsCreateAggregatedProject.CreateAggregatedProject()
        /// </devdoc>
        /// <returns>HRESULT</returns>
        protected virtual void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out System.IntPtr project, out int canceled)
        {
            // If the extension is that of the base project then we don't get called
            project = IntPtr.Zero;
            canceled = 0;
        }

        int IVsProjectFactory.Close()
        {
            this.Dispose(true);

            return VSConstants.S_OK;
        }

        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.Dispose"]/*' />
        protected virtual void Dispose(bool disposing)
        {
            if (_serviceProvider != null)
            {
                _serviceProvider.Dispose();
                _serviceProvider = null;
            }
        }

        int IVsProjectFactory.SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider provider)
        {
            // keep track of our service provider
            this._serviceProvider = new ServiceProvider(provider);

            this.Initialize();

            return VSConstants.S_OK;
        }
        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.Initialize"]/*' />
        /// <devdoc>
        /// Called by SetSite after setting our service provider
        /// </devdoc>
        protected virtual void Initialize()
        {
        }

        #endregion

        #region IVsAggregatableProjectFactory

        int IVsAggregatableProjectFactoryCorrected.GetAggregateProjectType(string fileName, out string projectTypeGuid)
        {
            projectTypeGuid = this.ProjectTypeGuids(fileName);
            return VSConstants.S_OK;
        }

        int IVsAggregatableProjectFactoryCorrected.PreCreateForOuter(IntPtr outerProjectIUnknown, out IntPtr projectIUnknown)
        {
            projectIUnknown = IntPtr.Zero;  // always initialize out parameters of COM interfaces!

            object newProject = PreCreateForOuter(outerProjectIUnknown);

            IntPtr newProjectIUnknown = IntPtr.Zero;
            ILocalRegistryCorrected localRegistry = (ILocalRegistryCorrected)Package.GetGlobalService(typeof(SLocalRegistry));
            Debug.Assert(localRegistry != null, "Could not get the ILocalRegistry object");
            if (localRegistry == null)
            {
                throw new InvalidOperationException();
            }
            string progid = "VisualStudio.ProjectAggregator2";
            Type type = System.Type.GetTypeFromProgID(progid);
            if (type == null)
                throw new COMException();
            Guid clsid = type.GUID; // {D2322F25-4CE9-4A06-9266-50C1C576772F}
            Guid riid = VSConstants.IID_IUnknown;
            uint dwClsCtx = (uint)CLSCTX.CLSCTX_INPROC_SERVER;
            IntPtr aggregateProjectIUnknown = IntPtr.Zero;
            IVsProjectAggregator2 vsProjectAggregator2 = null;

            try
            {
                ErrorHandler.ThrowOnFailure(localRegistry.CreateInstance(clsid, outerProjectIUnknown, ref riid, dwClsCtx, out aggregateProjectIUnknown));

                // If we have a non-NULL punkOuter then we need to create a COM aggregated object with that punkOuter,
                // if not then we are the top of the aggregation.
                if (outerProjectIUnknown != IntPtr.Zero)
                {
                    newProjectIUnknown = Marshal.CreateAggregatedObject(outerProjectIUnknown, newProject);
                }
                else
                {
                    newProjectIUnknown = Marshal.CreateAggregatedObject(aggregateProjectIUnknown, newProject); ;
                }

                vsProjectAggregator2 = (IVsProjectAggregator2)Marshal.GetObjectForIUnknown(aggregateProjectIUnknown);
                if (vsProjectAggregator2 != null)
                    vsProjectAggregator2.SetMyProject(newProjectIUnknown);

                // We return the native ProjectAggregator2 COM object as the project created by our project
                // factory. This ProjectAggregator2 main purpose is to manage the fact that the punkInner pointer
                // for the project aggregation is not known until after IVsAggregateProject::SetInnerProject is 
                // called. This native object has a special implementation of QueryInterface that can handle 
                // the SetInnerProject mechanism. The ProjectAggregator2 will first delegate QueryInterface 
                // calls to our managed project and then delegates to the inner Project.
                // Note: we need to return an AddRef'ed IUnknown (AddRef comes from CreateInstance call).
                projectIUnknown = aggregateProjectIUnknown;
                aggregateProjectIUnknown = IntPtr.Zero;
            }
            finally
            {
                if (newProjectIUnknown != IntPtr.Zero)
                    Marshal.Release(newProjectIUnknown);
                if (aggregateProjectIUnknown != IntPtr.Zero)
                    Marshal.Release(aggregateProjectIUnknown);
            }

            if (projectIUnknown == IntPtr.Zero)
                return VSConstants.E_FAIL;

            return VSConstants.S_OK;
        }

        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.PreCreateForOuter"]/*' />
        /// <devdoc>
        /// This function returns an instance of the project. This is just creating the object,
        /// VS will later call SetInner and InitializeForOuter to initialize it.
        /// </devdoc>
        /// <param name="outerProjectIUnknown"></param>
        /// <returns>The project subtype</returns>
        protected abstract object PreCreateForOuter(IntPtr outerProjectIUnknown);
        #endregion

        /// <include file='doc\FlavoredProjectFactoryBase.uex' path='docs/doc[@for="FlavoredProjectFactoryBase.ProjectTypeGuids"]/*' />
        protected virtual string ProjectTypeGuids(string file)
        {
            throw new NotImplementedException();
        }
    }
}
