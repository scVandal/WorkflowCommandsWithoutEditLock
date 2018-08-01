using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Gutters;
using Sitecore.Workflows;

/// <summary>
///     This class is a straight extract-and-copy of the WorflowState class in Sitecore.Kernel under the namespace  
///     Sitecore.Shell.Applications.ContentEditor.Gutters.WorkflowState.
///     There is one single change between this file and the base workflowState file,  at line 61 , which calls a custom extendedShowWorkflowState
///     command that contains additional logic to check if user can execute workflow state without locking.
/// </summary>
/// <remarks>Don't forget to change namespace to your environment</remarks>
namespace SS.BaseConfig.Extensions.Gutters
{
    public class ExtendedWorkflowState : GutterRenderer
    {
        /// <summary>
        /// Determines whether this instance is visible and should be rendered in context menu.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsVisible()
        {
            if (!Settings.Workflows.Enabled)
                return false;
            return base.IsVisible();
        }

        /// <summary>Gets the icon.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The icon.</returns>
        protected override GutterIconDescriptor GetIconDescriptor(Item item)
        {
            Assert.ArgumentNotNull((object)item, nameof(item));
            string str1 = item[FieldIDs.Workflow];
            string str2 = item[FieldIDs.WorkflowState];
            if (!Settings.Workflows.Enabled || !item.Access.CanWrite())
                return (GutterIconDescriptor)null;
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return (GutterIconDescriptor)null;
            IWorkflowProvider workflowProvider = item.Database.WorkflowProvider;
            if (workflowProvider == null)
                return (GutterIconDescriptor)null;
            IWorkflow workflow = workflowProvider.GetWorkflow(item);
            if (workflow == null)
                return (GutterIconDescriptor)null;
            Sitecore.Workflows.WorkflowState state = workflow.GetState(item);
            if (state == null)
                return (GutterIconDescriptor)null;
            if (state.FinalState)
                return (GutterIconDescriptor)null;
            GutterIconDescriptor gutterIconDescriptor = new GutterIconDescriptor();
            gutterIconDescriptor.Icon = state.Icon;
            gutterIconDescriptor.Tooltip = state.DisplayName;
            WorkflowCommand[] workflowCommandArray = WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(item), item);
            if (workflowCommandArray != null && workflowCommandArray.Length != 0)
                //Modify the event subscribed to the gutterIconDescriptor to call custom command, found at ExtendedShowWorkflowCommands.cs
                gutterIconDescriptor.Click = "ss:extendedshowworkflowcommands(id=" + (object)item.ID + ",language=" + (object)item.Language + ",version=" + (object)item.Version + ",database=" + item.Database.Name + ")";
            return gutterIconDescriptor;
        }
    }
}