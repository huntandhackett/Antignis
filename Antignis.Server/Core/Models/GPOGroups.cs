namespace Antignis.Server.Core.Models
{
    public sealed class GPOGroups
    {
        /// <summary>
        /// Name of the group that contains all computer objects
        /// </summary>
        public string ScopeGroupName { get; set; }

        /// <summary>
        /// Name of the group that contains users that can bypass the block rules
        /// May be null 
        /// </summary>
        public string UserBypassGroupName { get; set; }

        /// <summary>
        /// Name of the group that contains the computer objects that can bypass the block rules
        /// May be null
        /// </summary>
        public string ComputerBypassGroupName { get; set; }
    }

}
