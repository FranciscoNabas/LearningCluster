using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;


namespace WorkItemTrackingSample
{
    class Program
    {
        public static void Main(string[] args)
        {
            string Title = "\"Teste título task\"";
            string Description = "\"Teste descrição task\"";

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript("if((Get - PSSnapIn - Name Microsoft.TeamFoundation.PowerShell - ErrorAction SilentlyContinue) -eq $null){Add - PSSnapin Microsoft.TeamFoundation.PowerShell} \n"
                    + " [string] $tfsServerCollection = \"https://tfs.tecnogroup.com.br/tfs/Eat\"\n"
                    + " [string] $workItemType = \"Saude\\Task\"\n"
                    + " [string] $taskTitle = " + Title + "\n"
                    + " [string] $description = " + Description + "\n"
                    + " [string] $assignedTo = \"Francisco Eugênio Romanini Nabas\"\n"
                    + " [string] $iterationPath = \"Saude\"\n"
                    + " [string] $taskFields = \"Title=$($taskTitle);Iteration Path=$($iterationPath);Assigned To=$($assignedTo);Description=$($description)\"\n"
                    + " $wiOutput = tfpt workitem /new $workItemType /collection:$tfsServerCollection /fields:$taskfields\n"
                    + " $wiSplitOutput = $wiOutput.split()\n"
                    + " $wiNumber = $wiSplitOutput[2]\n"
                    + " Write-Host \"The TFS work item number is: $wiNumber\"");
                Collection <PSObject> PSOutput = PowerShellInstance.Invoke();
                foreach (PSObject outputItem in PSOutput)
                {
                    Console.WriteLine(outputItem.BaseObject.GetType().FullName);
                    Console.WriteLine(outputItem.BaseObject.ToString() + "\n");
                }
            }
        }
    }
}
