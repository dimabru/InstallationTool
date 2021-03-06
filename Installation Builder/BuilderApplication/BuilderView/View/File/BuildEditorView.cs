﻿using HelperProject.HelperLibrary;
using HelperProject.HelperLibrary.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace BuilderApplication.View.File
{
    public partial class BuildEditorView : HelperProject.BaseView.BaseMainForm
    {
        /*
        * integers set so the borderless form could move
        */
        int mov;
        int movX;
        int movY;


        public BMainForm BmainForm;

        private List<Task> tasks { get; set; }
        public Build build { get; set; }
        string buildName = string.Empty;
        string buildDescription = String.Empty;
        private string packagePath = "";

        public BuildEditorView()
        {
            InitializeComponent();
            tasks = new List<Task>();
            populatePlugins();
        }

        public BuildEditorView(List<Task> tsks, string name, string desc, BMainForm BmainForm)
        {
            InitializeComponent();
            this.BmainForm = BmainForm;
            tasks = tsks;
            this.Name = name;
            buildDescription = desc;
            buildName = name;
            populatePlugins();
            populateTree();
            treeViewPlugins.ExpandAll();
        }

        private void populateTree()
        {
            treeViewPlugins.Nodes.Clear();

            foreach (Task task in tasks)
            {
                TreeNode taskNode = new TreeNode(task.name);

                foreach (Plugin plugin in task.plugins)
                {
                    taskNode.Nodes.Add(plugin.name);
                }
                treeViewPlugins.Nodes.Add(taskNode);
            }

            treeViewPlugins.ExpandAll();
            checkSaveButton();
        }

        private void buttonAddTask_Click(object sender, EventArgs e)
        {
            string taskName = textBoxTaskName.Text;

            if (taskExists(taskName))
            {
                Dialogs.ErrorMessage("Task already exist. Please enter a different name");
                return;
            }

            Task task = new Task(textBoxTaskName.Text);
            tasks.Add(task);

            treeViewPlugins.Nodes.Add(taskName);
            checkSaveButton();
        }

        private bool taskExists(string task)
        {
            foreach (TreeNode node in treeViewPlugins.Nodes)
            {
                if (node.Text == task)
                {
                    return true;
                }
            }
            return false;
        }

        private void populatePlugins()
        {
            comboBoxChoosePlugin.Items.Clear();

            Plugin.LoadPlugins();
            foreach (Plugin plugin in Plugin.PluginList)
            {
                comboBoxChoosePlugin.Items.Add(plugin.name);
            }
        }

        private void buttonAddPlugin_Click(object sender, EventArgs e)
        {
            string pluginName = comboBoxChoosePlugin.Text;

            if (string.IsNullOrEmpty(pluginName))
            {
                return;
            }
            if (treeViewPlugins.SelectedNode == null || treeViewPlugins.SelectedNode.Level != 0)
            {
                Dialogs.ErrorMessage($"Please select a task to assign {pluginName} to");
                return;
            }
            if (checkCorrectPluginControlInput())
            {
                treeViewPlugins.SelectedNode.Nodes.Add(pluginName);
                Plugin plugin = Plugin.LocatePlugin(pluginName);
                
                foreach(string label in pluginControl.inputDict.Keys)
                {
                    plugin.updateInsertion(label, pluginControl.inputDict[label].Text);
                }

                string taskName = treeViewPlugins.SelectedNode.Text;
                Task task = tasks.Find(t => t.name == taskName);
                task.addPlugin(plugin);

                pluginControl.resetInputs(plugin);
            }
            else
            {
                Dialogs.ErrorMessage("Please fill all fields");
            }

            treeViewPlugins.ExpandAll();
        }

        private bool checkCorrectPluginControlInput()
        {
            return pluginControl.filledInputs();
        }  

        private void updatePlugin(object sender, EventArgs e)
        {
            string selectedPlugin = comboBoxChoosePlugin.Text;

            if (string.IsNullOrEmpty(selectedPlugin))
            {
                pluginControl.setPluginStatus(visible: true);
                textBoxDescription.Text = String.Empty;
                return;
            }

            else
            {
                this.pluginControl.Controls.Clear();
                this.pluginControl.inputDict.Clear();
                pluginControl.Refresh();
                Plugin plugin = Plugin.LocatePlugin(selectedPlugin);
                textBoxDescription.Text = plugin.description;
                pluginControl.appendPluginInfo(plugin);
            }

        }

        private void buttonEditTask_Click(object sender, EventArgs e)
        {
            if (treeViewPlugins.SelectedNode == null)
            {
                return;
            }
            string name = treeViewPlugins.SelectedNode.Text;
            new EditNameView(name, this).ShowDialog();
        }

        private void checkSaveButton()
        {
            if (this.treeViewPlugins.Nodes.Count != 0)
            {
                buttonSave.Enabled = true;
            }
            else
            {
                buttonSave.Enabled = false;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            new BuildDetailsView(tasks, buildName, buildDescription, packagePath).ShowDialog();
            //BmainForm.Refresh();
        }

        private void validTreeButtons(object sender, TreeViewEventArgs e)
        {
            TreeNode selected = treeViewPlugins.SelectedNode;
            if (selected.Level == 0)
            {
                buttonEditTask.Visible = true;
            }
            else
            {
                buttonEditTask.Visible = false;
            }
            buttonMoveDown.Visible = true;
            buttonMoveUp.Visible = true;
            buttonDelete.Visible = true;
        }

        public void updateTaskName(string oldName, string newName)
        {
            Task locate = tasks.Find(t => t.name == oldName);
            locate.name = newName;
            populateTree();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            TreeNode selected = treeViewPlugins.SelectedNode;
            if (selected == null)
            {
                return;
            }
            // Selected node is a task
            if (selected.Level == 0)
            {
                tasks.RemoveAll(t => t.name == selected.Text);
            }
            // Selected node is a plugin
            else
            {
                int taskIndex = selected.Parent.Index;
                Task editTask = tasks.ElementAt(taskIndex);
                int pluginIndex = selected.Index;
                Plugin toRemove = editTask.plugins.ElementAt(pluginIndex);
                editTask.plugins.Remove(toRemove);
            }

            populateTree();
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            if (treeViewPlugins.SelectedNode == null)
            {
                return;
            }
            if (treeViewPlugins.SelectedNode.Level == 0)
            {
                moveTasks(moveUp: true);
            }
            else
            {
                movePlugins(moveUp: true);
            }

            populateTree();
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            if (treeViewPlugins.SelectedNode == null)
            {
                return;
            }
            if (treeViewPlugins.SelectedNode.Level == 0)
            {
                moveTasks(moveUp: false);
            }
            else
            {
                movePlugins(moveUp: false);
            }

            populateTree();
        }

        private void moveTasks(bool moveUp)
        {
            TreeNode selected = treeViewPlugins.SelectedNode;
            int taskIndex = selected.Index;
            Task toMove = tasks.Find(t => t.name == selected.Text);

            if (taskIndex == 0 && moveUp)
            {
                return;
            }
            if (taskIndex == treeViewPlugins.Nodes.Count - 1 && !moveUp)
            {
                return;
            }

            tasks.Remove(toMove);

            if (moveUp)
            {
                tasks.Insert(taskIndex - 1, toMove);
            }
            else
            {
                tasks.Insert(taskIndex + 1, toMove);
            }
        }

        private void movePlugins(bool moveUp)
        {
            TreeNode selected = treeViewPlugins.SelectedNode;
            int pluginIndex = selected.Index;
            int taskIndex = selected.Parent.Index;
            List<Plugin> pluginList = tasks.ElementAt(taskIndex).plugins;
            Plugin toMove = pluginList.ElementAt(pluginIndex);

            // Move up
            if (moveUp)
            {
                pluginList.Remove(toMove);
                // First item in the list
                if (pluginIndex == 0)
                {
                    // No task to move to
                    if (taskIndex == 0)
                    {
                        return;
                    }
                    Task task = tasks.ElementAt(taskIndex - 1);
                    task.addPlugin(toMove);
                }
                // Not first item in the list
                else
                {
                    pluginList.Insert(pluginIndex - 1, toMove);
                }
            }
            // Move down
            else
            {
                if (pluginIndex == pluginList.Count - 1)
                {
                    if (taskIndex == tasks.Count - 1)
                    {
                        return;
                    }
                    pluginList.Remove(toMove);
                    
                    Task task = tasks.ElementAt(taskIndex + 1);
                    task.plugins.Insert(0, toMove);
                }
                else
                {
                    pluginList.Remove(toMove);
                    pluginList.Insert(pluginIndex + 1, toMove);
                }
            }
        }

        /*
         * the functions that give the borderless form, moving capabilities
         */

        private void panelBuilderMainForm_MouseDown(object sender, MouseEventArgs e)
        {
            mov = 1;
            movX = e.X;
            movY = e.Y;
        }

        private void panelBuilderMainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == 1)
            {
                this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
            }
        }

        private void panelBuilderMainForm_MouseUp(object sender, MouseEventArgs e)
        {
            mov = 0;
        }

        private void ExitPictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ResumePictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void buttonLoadPackage_Click(object sender, EventArgs e)
        {
            string folderPath = Dialogs.OpenFolder();
            if (String.IsNullOrEmpty(folderPath))
            {
                this.labelPackagePath.Text = "No package is selected";
                this.packagePath = "";
                return;
            }

            this.labelPackagePath.Text = $"Selected package:\n{folderPath}";
            this.packagePath = folderPath;
        }
    }
}
