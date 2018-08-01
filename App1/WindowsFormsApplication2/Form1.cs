using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Title = "\"" + textBox2.Text + "\"";
            string Description = "\"" + textBox1.Text + "\"";

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
                    + " Write-Output \"O número da task é: $wiNumber\"");
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                foreach (PSObject outputItem in PSOutput)
                {
                    MessageBox.Show(outputItem.BaseObject.ToString() + "\n");
                }
                textBox1.Clear();
                textBox2.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
