# Summary
The ExtendedWorkflowPanel class provided in this repository is used to revert a frustrating change in Sitecore 8.1+ where any user other than an administrator needs to lock an item before executing a workflow command on the item. While the intent by Sitecore (limit number of potential collisions due to multiple people executing workflow commands on a single item) is admirable, the reality is for a great many smaller instances this change caused more harm than good.

This class provides a medium-ground between the old and new way by modifying the WorkflowPanel class in Sitecore.Client to include a custom validation function for the Context User which can be used to enable the old way of executing workflow commands on an item without locking it to certain roles, users, or any number of other criteria that one desires!

# How It Works
The ExtendedWorkflowPanel.cs class is a direct extract-and-copy of the WorkflowPanel.cs class from Sitecore.Client in the namespace Sitecore. The class makes only two modifications to the class.

Firstly, the class contains a new method, canUserRunCommandsWithoutEdit(). This method is the main authenticator method used to determine if the current context user should be allowed to execute workflow commands without locking the item. As provided, this class defines a list of roles and checks if the user is a member of any of those roles. If they are, a true boolean value is returned, meaning they should have the ability to execute commands without locking items. If they are a member of none of the defined roles, a false boolean value is returned indicating they should not have this access.

This boolean value is then plugged in to the new piece of code introduced in 8.1: the flag4 boolean value in the Render method. With 8.1, flag4, which is used by the WorkflowPanel to 'grey out' the workflow command buttons, is set to 'true' (don't grey out') only if the user is an administrator or the item is in a locked edit state (or if your environment doesn't require lock for editing...but you shouldn't be doing that anyways because versioning!).

The ExtendedWorkflowPanel makes a minor change to this flag4 to include an additional OR condition based on the returned boolean of the canUserRunCommandsWithoutEdit() method. If this method returns true, flag4 is set to true and thus the workflow commands are not greyed out.

This is the extent of modifications to the base WorkflowPanel class; no other changes were made.

#Further Extending
The canUserRunCommandsWithoutEdit() is bare-bones in the file to show the basic functionality, but can be further extended to meet your environment and business needs. Additional roles can be added. Role hardening can be implemented by changing the List to be List<Role>, access can be dolled out based on individual users, or any other number of validations you can think of.

Extending beyond the canUserRunCommandsWithoutEdit() method should not be necessary and is not recommended, as modifying any other code can change the way the WorkflowPanel operates.

# Implementation
Implementing the ExtendedWorkflowPanel class is a simple process with the following general steps:
1. Merge the class into your Sitecore Visual Studio solution and modify the Namespace of the class to suit your environment. Make note of this namespace - you'll need it in the next steps.
2. Make changes to the canUserRunCommandsWithoutEdit() method, either adding your own roles, or building out further validation rules as your environment and business needs.
3. Build and deploy the project containing the ExtendedWorkflowPanel class to your Sitecore environment.
4. Once deployed, you will need to make one configuration change. Log into your Sitecore environment as an administrator and open the Core database in the Content Editor. Navigate to the WorkflowPanel Sitecore item at the item path _/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Workflow/WorkflowPanel_
5. In the Data fieldset, you should see a single _Type_ field with a reference to the base WorkflowPanel class. Modify this field to match the Namespace for your ExtendedWorkflowPanel.cs class. Use the format <Namespace>.ExtendedWorkflowPanel,<Assembly>. IE: BaseConfig.Extensions.ExtendedWorkflowPanel,BaseConfig
6. Save the item and test it out. Log into your Sitecore environment with a user who you know matches the conditions outlined in the canUserRunCommandsWithoutEdit() method and you should see the Workflow Commands for an item the user has access to and is in a workflow be available to execute.
