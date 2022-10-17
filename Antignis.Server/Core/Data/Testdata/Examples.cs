
using System.Collections.Generic;
namespace Antignis.Server.Core.Data.Testdata
{
    internal sealed class Examples
    {
        /// <summary>
        /// Returns a list with generated test data
        /// </summary>
        /// <returns></returns>
        internal static List<Core.Models.Host> GetExampleData(string domainname = "")
        {
            // Generate a model
            Generator generator = string.IsNullOrEmpty(domainname) ? new Generator() : new Generator(domainname);
            List<Models.Host> hosts = new List<Models.Host>();

            do
            {
                Models.Host example = generator.GetExample();
                hosts.Add(example);

            } while (!generator.Depleted());

            return hosts;
        }
    }
}
