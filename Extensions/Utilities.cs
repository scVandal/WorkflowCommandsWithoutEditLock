using Sitecore;
using Sitecore.Security.Accounts;
using System.Collections.Generic;

/// <remarks>Don't forget to change the Namespace to suit your environment!</remarks>
namespace SS.BaseConfig.Extensions
{
    /// <summary>
    /// Contains various generic and/or abstract utility methods to acomplish tasks in specific Extension code files
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Determines whether the current context user has the ability to execute workflow commands without locking the item
        /// for editing. In its current implementation the class simply checks the current user against a list of roles that 
        /// shoudl have this permission. You can extend and expand this class to include all sorts of validation as desired.
        /// </summary>
        /// <returns>
        ///     true if user meets the cireteria set out by the class
        /// </returns>
        public static bool canUserRunCommandsWithoutLocking()
        {
            User user = Context.User;
            //Define list of roles that are approved to have this access. Add the full name for your environment here 
            List<string> roleList = new List<string>
            {
                "sitecore\\Author",         //This is the standard Author role provided with Sitecore
                "sitecore\\anotherRoleHere" //This is a fake role just serving as an example!
            };
            //Iterate over each role in the list and check if user is a member of the role. If they are return true
            foreach (string s in roleList)
            {
                if (user.IsInRole(s)) { return true; }
            }
            /// <remarks>
            /// Add your own validation here. Maybe if a user is of a certain account. Or if they have edit rights to the item. Or if
            /// their full name contains every vowel in the english langauge! Any validation goes!!!
            /// </remarks>

            //Return false if conditions aren't met, indicating user should not have the right to execute commands without locking item
            return false;
        }
    }
}