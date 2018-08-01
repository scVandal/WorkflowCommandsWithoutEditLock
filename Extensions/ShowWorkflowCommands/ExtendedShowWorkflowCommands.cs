using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Framework.CommandBuilders;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Workflows;
using System;
using System.Collections.Generic;

/// <summary>
///     This class is a extact-and-copy of the ShowWorkflowCommand in Sitecore.Kernel. This class contains a single modification to the menu.Add code 
///     at line 68 that makes a call to Utilities.canUserRunCommandsWithoutLocking() method to check if context user meets the criteria set out by this method
///     to allow execution of workflow commands without locking. If they pass this check, the gutter workflow command buttons become active even without
///     locking the item.
/// </summary>

///<remarks>Don't forget to change Namespace to suit your environment!</remarks>
namespace SS.BaseConfig.Extensions.ShowWorkflowCommands
{
    [Serializable]
    public class ExtendedShowWorkflowCommands : Command
    {
        /// <summary>Queries the state of the command.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
        public override CommandState QueryState(CommandContext context)
        {
            if (!Settings.Workflows.Enabled)
                return CommandState.Hidden;
            return base.QueryState(context);
        }

        /// <summary>Executes the command in the specified context.</summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull((object)context, nameof(context));
            string parameter1 = context.Parameters["database"];
            string parameter2 = context.Parameters["id"];
            string parameter3 = context.Parameters["language"];
            string parameter4 = context.Parameters["version"];
            Database database = Factory.GetDatabase(parameter1);
            if (database == null)
                return;
            Item obj = database.GetItem(parameter2, Language.Parse(parameter3), Sitecore.Data.Version.Parse(parameter4));
            if (obj == null)
                return;
            IWorkflow workflow = obj.Database.WorkflowProvider.GetWorkflow(obj);
            if (workflow == null)
                return;
            WorkflowCommand[] workflowCommandArray = WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(obj), obj);
            if (workflowCommandArray == null || workflowCommandArray.Length == 0)
                return;
            Menu menu = new Menu();
            SheerResponse.DisableOutput();
            foreach (WorkflowCommand command in workflowCommandArray)
            {
                string click = new WorkflowCommandBuilder(obj, workflow, command).ToString();
                //Add new logical condition to call canUserRunCommandsWithoutEdit() in Utilities class to check if user has permissions to execute
                //workflow commands without locking. The rest of the conditions are same as in default class
                menu.Add("C" + command.CommandID, command.DisplayName, command.Icon, string.Empty, click, false, string.Empty, MenuItemType.Normal).Disabled 
                    = !Utilities.canUserRunCommandsWithoutLocking() && !Context.User.IsAdministrator && !obj.Locking.HasLock() && Settings.RequireLockBeforeEditing;
            }
            SheerResponse.EnableOutput();
            SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, "right", (Control)menu);
        }        
    }
}