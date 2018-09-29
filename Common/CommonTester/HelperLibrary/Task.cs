﻿using HelperLibrary.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelperLibrary
{
    public class Task
    {
        public List<Plugin> plugins { get; set; }

        public Task(List<Plugin> plugs)
        {
            plugins = plugs;
        }

        public Task()
        {
            plugins = new List<Plugin>();
        }

        public void addPlugin(Plugin plugin)
        {
            plugins.Add(plugin);
        }
    }
}