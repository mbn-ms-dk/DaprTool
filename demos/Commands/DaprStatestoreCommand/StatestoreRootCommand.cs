using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace demos.Commands.DaprStatestoreCommand
{
    public class StatestoreRootCommand : Command
    {
        public StatestoreRootCommand() : base("state", "Dapr file demo")
        {
            var deployOption = new Option<bool>(
                name: "--deployOnly", description: "Create Azure environment");
            AddOption(deployOption);

            this.SetHandler((deploy) =>
            { Demo(deploy); }, deployOption);
        }

        private void Demo(bool deploy)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"Path: {path}");

            path = path +  @"Infra\Demo.ps1";
            Console.WriteLine($"Path: {path}");
            //create runspace
            var runSpace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace();
            runSpace.Open();

            var pipe = runSpace.CreatePipeline();
            var cmd = new System.Management.Automation.Runspaces.Command(path, true);
            if(deploy)
            {
                var para = new System.Management.Automation.Runspaces.CommandParameter("deployOnly");
                cmd.Parameters.Add(para);
            }
            pipe.Commands.Add(cmd);
            var results = pipe.Invoke();
            runSpace.Close();

            foreach(var pso in results)
            {
                Console.WriteLine(pso.ToString());
            }
        }
    }
}
