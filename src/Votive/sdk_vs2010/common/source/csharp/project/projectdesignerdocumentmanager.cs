//-------------------------------------------------------------------------------------------------
// <copyright file="projectdesignerdocumentmanager.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Net;
using MSBuild = Microsoft.Build.Evaluation;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.Package
{
	class ProjectDesignerDocumentManager : DocumentManager
	{
		#region ctors
		public ProjectDesignerDocumentManager(ProjectNode node)
			: base(node)
		{
		}
		#endregion

		#region overriden methods

		public override int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
		{
			Guid editorGuid = VSConstants.GUID_ProjectDesignerEditor;
			return this.OpenWithSpecific(0, ref editorGuid, String.Empty, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
		}

		public override int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
		{
			frame = null;
			Debug.Assert(editorType == VSConstants.GUID_ProjectDesignerEditor, "Cannot open project designer with guid " + editorType.ToString());


			if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			IVsUIShellOpenDocument uiShellOpenDocument = this.Node.ProjectMgr.Site.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
			IOleServiceProvider serviceProvider = this.Node.ProjectMgr.Site.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;

			if (serviceProvider != null && uiShellOpenDocument != null)
			{
				string fullPath = this.GetFullPathForDocument();
				string caption = this.GetOwnerCaption();

				IVsUIHierarchy parentHierarchy = this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchy) as IVsUIHierarchy;

				IntPtr parentHierarchyItemId = (IntPtr)this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchyItemid);

				ErrorHandler.ThrowOnFailure(uiShellOpenDocument.OpenSpecificEditor(editorFlags, fullPath, ref editorType, physicalView, ref logicalView, caption, parentHierarchy, (uint)(parentHierarchyItemId.ToInt32()), docDataExisting, serviceProvider, out frame));

				if (frame != null)
				{
					if (windowFrameAction == WindowFrameShowAction.Show)
					{
						ErrorHandler.ThrowOnFailure(frame.Show());
					}
				}
			}

			return VSConstants.S_OK;
		}
		#endregion

	}
}
