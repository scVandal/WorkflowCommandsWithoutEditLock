# Summary
The Extended classes provided in this repository are used to revert a frustrating change in Sitecore 8.1+ where any user other than an administrator needs to lock an item before executing a workflow command on the item using either the Workflow Panel (in the ribbon) or the gutter. While the intent by Sitecore (limit number of potential collisions due to multiple people executing workflow commands on a single item) is admirable, the reality is for a great many smaller instances this change caused more harm than good.

The repository contains files that provide a medium-ground between the old and new way by extending and making minor modificatiosn to the base WorkflowPanel and Gutter workflow classes to use a custom validation function for the Context User which can be used to enable the old way of executing workflow commands on an item without locking it to certain roles, users, or any number of other criteria that one desires!

# How It Works: Workflow Panel
The Utilities class provides a single canUserRunCommandsWithoutLocking() method which allows the programmer to set any number of validation rules and conditions to define if the current context user has the ability to execute workflow commands without locking the item.

The ExtendedWorkflowPanel.cs class is a direct extract-and-copy of the WorkflowPanel.cs class from Sitecore.Client in the Sitecore.Shell.Applications.ContentManager.Panels namespace. Sitecore 8.1 introduced a new 'feature' to this code: the flag4 boolean value in the Render method. With 8.1, flag4, which is used by the WorkflowPanel to 'grey out' the workflow command buttons, is set to 'true' (don't grey out') only if the user is an administrator or the item is in a locked edit state (or if your environment doesn't require lock for editing...but you shouldn't be doing that anyways because versioning!).

The ExtendedWorkflowPanel.cs class modifies a single line of code in the flag4 decleration to make a call out to the Utilities.canUserRunCommandsWithoutLocking() method, and if this method returns true, flag4 is set to true, meaning workflow commands can be executed by the user in the workflow panel without item locking.

# How It Works: Gutters
Gutters are slightly more complicated. The gutter icons for workflows are generated in Sitecore.Kernel under the Sitecore.Shell.Applications.ContentEditor.Gutters.WorkflowState namepsace. This WorkflowState class makes a call to a base Sitecore command to check if the workflow command icon should be greyed out. In order to overwrite this behavior, we do an extract-and-copy of two files.

Firstly, we extract the WorkflowState class from Sitecore.Shell.Applications.ContentEditor.Gutters.WorkflowState and extend it into the ExtendedWorkflowState.cs class in this repository. In this class we make a single line modification to call a new command, "ss:extendedShowWorkflowCommands".

This new command is defined in ExtendedShowWorkflowCommands.cs, which is itself a extract-and-copy of the base showWorkflowCommand class in Sitecore.Kernel. In our extended command, we make a single-line code modification to add a call to Utilities.canUserRunCommandsWithoutLocking(). The command takes the return of this Utilities method and sets the .disabled state of the menu item to true or false based on its response.

Last step is we have to patch the new command using a quick config patch, located in App_Config/Includes/SitecoreSolutions.Commands.Config. Nothing special here; just a basic Sitecore patch file that adds the new command to Sitecore.config and allows ExtendedWorkflowState class to make a call to this new command.

# Result
The end result is if the context user passes whatever authentication is defined in Utilities.canUserRunCommandsWithoutLocking(), they will be able to execute workflow commands for the item in question without locking the item. These extension classes make no futher modifications, so all the same basic Sitecore logic and rules with workflow apply (eg. if item doesn't have workflow or context user doesn't have permissions to workflow, no commands will show, etc).

# Implementation
1. Merge the classes into your Sitecore Visual Studio solution and modify the Namespace of the classes to suit your environment. Make note of the namespaces - you'll need it in the next steps.
2. Make changes to the canUserRunCommandsWithoutEdit() method, either adding your own roles, or building out further validation rules as your environment and business needs.
3. Build and deploy the project to your Sitecore instance, making sure the compiled dll file is deployed to the \bin folder and make sure the custom command config patch file - SitecoreSolutions.Commands.config - is deployed to the \App_Config\Includes so it is patched into the base Sitecore.config file.
4. You will need to make two configuration changes. Log into your Sitecore environment as an administrator and open the Core database in the Content Editor. 
5. Navigate to the WorkflowPanel Sitecore item at the item path _/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Workflow/WorkflowPanel_ In the Data fieldset, you should see a single _Type_ field with a reference to the base WorkflowPanel class. Modify this field to match the Namespace for your ExtendedWorkflowPanel.cs class. Use the format <Namespace>.ExtendedWorkflowPanel,<Assembly>. IE: BaseConfig.Extensions.ExtendedWorkflowPanel,BaseConfig. Save the item.
6. You will also need to point the WorkflowState gutter item to the extended class. Still in the Core database, navigate to _/sitecore/content/Applications/Content Editor/Gutters/WorkflowState_ and in the _Type_ field enter the namespace of the ExtendedWorkflowState class using the <Namespace>.ExtendedWorkflowState,<Assembly> pattern as with step 5. Save the item.
7. Save the item and test it out. Log into your Sitecore environment with a user who you know matches the conditions outlined in the canUserRunCommandsWithoutEdit() method and you should see the Workflow Commands for an item the user has access to and is in a workflow be available to execute in both the panel as well as the gutter.
  
# Further Extending
The canUserRunCommandsWithoutEdit() is bare-bones in the file to show the basic functionality, but can be further extended to meet your environment and business needs. Additional roles can be added. Role hardening can be implemented by changing the List to be List<Role>, access can be dolled out based on individual users, or any other number of validations you can think of.

Extending beyond the canUserRunCommandsWithoutEdit() method should not be necessary and is not recommended, as modifying any other code can change the way the WorkflowPanel or Gutters operate.


