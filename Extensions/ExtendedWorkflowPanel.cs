/* <summary>
 *      This class is a standalone, extended copy of the WorkflowPanel class as defined in Sitecore.Client
 *      and makes minimal modifications to allow a user who meets certain role criteria (defined by you!) to
 *      execute Workflow Commands from the workflow panel without locking the item for editing.
 *      
 *      This is the way Sitecore worked prior to 8.1 and this class is a reversion to this functionality with the 
 *      added benefit of allowing you to dole out this 'inline workflow command execute' ability as you desire.
 *      
 *      The extensiosn to this class are minimal: a new canUserRunCommandsWithoutEdit class is created which returns a
 *      Boolean value. This boolean value is used in line 62 in the flag4 definition to allow the workflowpanel to enable
 *      user interaction with the Workflow Panel buttons.
 *      
 *      You can modify the canUserRunCommandsWithoutEdit() method to include additional roles, or expand it even further
 *      to suite your needs when it comes to limiting this inline workflow command execution ability.
 */

using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Security.AccessControl;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Framework.CommandBuilders;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;
using Sitecore.Workflows;
using System.Collections.Generic;
using System.Web.UI;

//CHANGE THE NAMESPACE TO SUIT YOUR ENVIRONMENT!
namespace BaseConfig.Extensions
{
    /// <summary>Represents a WorkflowPanel.</summary>
    public class ExtendedWorkflowPanel : RibbonPanel
    {
        /// <summary>The _check in item.</summary>
        private Item checkInItem;

        /// <summary>Renders the panel.</summary>
        /// <param name="output">The output.</param>
        /// <param name="ribbon">The ribbon.</param>
        /// <param name="button">The button.</param>
        /// <param name="context">The context.</param>
        public override void Render(HtmlTextWriter output, Ribbon ribbon, Item button, CommandContext context)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));
            Assert.ArgumentNotNull((object)ribbon, nameof(ribbon));
            Assert.ArgumentNotNull((object)button, nameof(button));
            Assert.ArgumentNotNull((object)context, nameof(context));
            if (context.Items.Length < 1)
                return;
            Item obj = context.Items[0];
            if (!this.HasField(obj, FieldIDs.Workflow) || !Settings.Workflows.Enabled)
                return;
            IWorkflow workflow;
            WorkflowCommand[] commands;
            ExtendedWorkflowPanel.GetCommands(context.Items, out workflow, out commands);
            bool flag1 = this.IsCommandEnabled("item:checkout", obj);
            bool flag2 = ExtendedWorkflowPanel.CanShowCommands(obj, commands);
            bool flag3 = this.IsCommandEnabled("item:checkin", obj);
            bool flag4 = Context.User.IsAdministrator || obj.Locking.HasLock() || !Settings.RequireLockBeforeEditing ||
                         canUserRunCommandsWithoutEdit();
            this.RenderText(output, ExtendedWorkflowPanel.GetText(context.Items));
            if (!(workflow != null | flag1 | flag2 | flag3))
                return;
            Context.ClientPage.ClientResponse.DisableOutput();
            ribbon.BeginSmallButtons(output);
            if (flag1)
                this.RenderSmallButton(output, ribbon, string.Empty, Translate.Text("Edit"), "Office/24x24/edit_in_workflow.png", Translate.Text("Start editing this item."), "item:checkout", this.Enabled, false);
            if (flag3)
            {
                Item checkInItem = this.GetCheckInItem();
                if (checkInItem != null)
                    this.RenderSmallButton(output, ribbon, string.Empty, checkInItem["Header"], checkInItem["Icon"], Translate.Text("Check this item in."), "item:checkin", this.Enabled, false);
            }
            if (workflow != null)
                this.RenderSmallButton(output, ribbon, Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("B"), Translate.Text("History"), "Office/16x16/history.png", Translate.Text("Show the workflow history."), "item:workflowhistory", this.Enabled, false);
            if (flag2)
            {
                foreach (WorkflowCommand command in commands)
                    this.RenderSmallButton(output, ribbon, string.Empty, command.DisplayName, command.Icon, command.DisplayName, new WorkflowCommandBuilder(obj, workflow, command).ToString(), this.Enabled & flag4, false);
            }
            ribbon.EndSmallButtons(output);
            Context.ClientPage.ClientResponse.EnableOutput();
        }

        /// <summary>Determines whether this instance can show commands.</summary>
        /// <param name="item">The item to check.</param>
        /// <param name="commands">The commands.</param>
        /// <returns>
        /// <c>true</c> if this instance [can show commands] the specified item; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanShowCommands(Item item, WorkflowCommand[] commands)
        {
            Assert.ArgumentNotNull((object)item, nameof(item));
            return (item.Appearance.ReadOnly || commands == null ? 0 : ((uint)commands.Length > 0U ? 1 : 0)) != 0 && (Context.IsAdministrator || item.Access.CanWriteLanguage() && (item.Locking.CanLock() || item.Locking.HasLock()));
        }

        /// <summary>Gets the commands.</summary>
        /// <param name="items">The items to get commands for.</param>
        /// <param name="workflow">The workflow.</param>
        /// <param name="commands">The commands.</param>
        private static void GetCommands(Item[] items, out IWorkflow workflow, out WorkflowCommand[] commands)
        {
            Assert.ArgumentNotNull((object)items, nameof(items));
            Item obj = items[0];
            if (obj != null)
            {
                IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
                if (workflowProvider != null && workflowProvider.GetWorkflows().Length != 0)
                {
                    workflow = workflowProvider.GetWorkflow(obj);
                    if (workflow != null && workflow.GetState(obj) != null)
                    {
                        commands = WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(obj), obj);
                        return;
                    }
                }
            }
            workflow = (IWorkflow)null;
            commands = (WorkflowCommand[])null;
        }

        /// <summary>Gets the text.</summary>
        /// <param name="items">The items.</param>
        /// <returns>The get text.</returns>
        private static string GetText(Item[] items)
        {
            Assert.ArgumentNotNull((object)items, nameof(items));
            if (items.Length == 0 || items.Length != 1)
                return string.Empty;
            Item obj = items[0];
            if (obj.Appearance.ReadOnly)
                return string.Empty;
            if (AuthorizationManager.IsAllowed((ISecurable)obj, AccessRight.ItemWrite, (Account)Context.User))
            {
                if (obj.Locking.HasLock())
                    return Translate.Text("<b>You</b> have locked this item.");
                if (obj.Locking.IsLocked())
                    return Translate.Text("<b>\"{0}\"</b> has locked this item.", (object)StringUtil.GetString(new string[2]
                    {
            obj.Locking.GetOwnerWithoutDomain(),
            "?"
                    }));
                if (obj.Locking.CanLock())
                    return Translate.Text("Click Edit to lock and edit this item.");
                IWorkflow workflow = obj.State.GetWorkflow();
                WorkflowState workflowState = obj.State.GetWorkflowState();
                if (workflow == null || workflowState == null)
                    return Translate.Text("You do not have permission to<br/>edit the content of this item.");
                if (workflowState.FinalState)
                    return Translate.Text("This item has been approved.");
                return Translate.Text("The item is in the <b>{0}</b> state<br/>in the <b>{1}</b> workflow.", (object)StringUtil.GetString(new string[2]
                {
          workflowState.DisplayName,
          "?"
                }), (object)StringUtil.GetString(new string[2]
                {
          workflow.Appearance.DisplayName,
          "?"
                }));
            }
            if (obj.Access.CanWrite())
                return Translate.Text("Click Edit to lock and edit this item.");
            IWorkflow workflow1 = obj.State.GetWorkflow();
            WorkflowState workflowState1 = obj.State.GetWorkflowState();
            if (workflow1 == null || workflowState1 == null)
                return Translate.Text("You do not have permission to<br/>edit the content of this item.");
            if (workflowState1.FinalState)
                return Translate.Text("This item has been approved.");
            return Translate.Text("The item is in the <b>{0}</b> state<br/>in the <b>{1}</b> workflow.", (object)StringUtil.GetString(new string[2]
            {
        workflowState1.DisplayName,
        "?"
            }), (object)StringUtil.GetString(new string[2]
            {
        workflow1.Appearance.DisplayName,
        "?"
            }));
        }

        /// <summary>Gets the check in item.</summary>
        /// <returns>Check in workflow item</returns>
        private Item GetCheckInItem()
        {
            if (this.checkInItem == null)
                this.checkInItem = Context.Database.Items["/sitecore/system/Settings/Workflow/Check In"];
            return this.checkInItem;
        }

        /// <summary>
        /// Determines whether command enabled for the specified item.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// 	<c>true</c> if [is command enabled] [the specified command]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCommandEnabled(string command, Item item)
        {
            Assert.ArgumentNotNullOrEmpty(command, nameof(command));
            Assert.ArgumentNotNull((object)item, nameof(item));
            CommandState commandState = CommandManager.QueryState(command, item);
            if (commandState != CommandState.Down)
                return commandState == CommandState.Enabled;
            return true;
        }

        /// <summary>
        /// Determines whether the current context user has the ability to execute workflow commands without locking the item
        /// for editing. In its current implementation the class simply checks the current user against a list of roles that 
        /// shoudl have this permission. You can extend and expand this class to include all sorts of validation as desired.
        /// </summary>
        /// <returns>
        ///     true if user meets the cireteria set out by the class
        /// </returns>
        private bool canUserRunCommandsWithoutEdit()
        {
            User user = Context.User;
            //Define list of roles that are approved to have this access. Add the full name for your environment here 
            List<string> roleList = new List<string>
            {
                "sitecore\\Author",         //This is the standard Author role provided with Sitecore
                "sitecore\\anotherRoleHere" //This is a fake role just serving as an example!
            };
            //Iterate over each role in the list and check if user is a member of the role. If they are return true
            foreach(string s in roleList)
            {
                if(user.IsInRole(s)) { return true; }
            }
            //Otherwise return false, indicating user should not have the right to execute commands without locking item
            return false;
        }
    }
}