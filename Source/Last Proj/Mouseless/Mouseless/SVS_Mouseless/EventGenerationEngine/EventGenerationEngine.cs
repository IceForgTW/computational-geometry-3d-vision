using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using EGInterface;

namespace EventGenerationEngine
{
    public class EventGenerationEngine
    {
        private List<IEventGenerator> _EG_Plugins = new List<IEventGenerator>();        // list of event generators

        public List<IEventGenerator> EG_Plugins
        {
            get { return _EG_Plugins; }
            set { _EG_Plugins = value; }
        }

        private int _N_EG_Plugins = 0;                                                  // number of EG plugins

        public int N_EG_Plugins
        {
            get { return _N_EG_Plugins; }
            set { _N_EG_Plugins = value; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public EventGenerationEngine()
        {
        }

        /// <summary>
        /// Load plugins from folder
        /// </summary>
        /// <param name="Folderpath">folder's path</param>
        public void Load_EG_Plugins(string Folderpath)
        {
            N_EG_Plugins = 0;
            foreach (string Filename in Directory.GetFiles(Folderpath, "*.dll"))
            {
                Assembly Asm = Assembly.LoadFile(Filename);
                foreach (Type AsmType in Asm.GetTypes())
                {
                    if (AsmType.GetInterface("IEventGenerator") != null)
                    {
                        IEventGenerator Plugin = (IEventGenerator)Activator.CreateInstance(AsmType);
                        EG_Plugins.Add(Plugin);
                        N_EG_Plugins++;
                    }
                }
            }
        }


        public string[] GetEventsName()
        {
            string[] rsl = new string[N_EG_Plugins];

            int cnt = 0;
            foreach (IEventGenerator eGene in EG_Plugins)
            {
                rsl[cnt] = eGene.GetName();
                cnt++;
            }
            return rsl;
        }

        public void RaiseEvent(string eName, object[] Params)
        {
            foreach (IEventGenerator eGenerator in EG_Plugins)
            {
                if (eGenerator.GetName().ToLower().Trim().CompareTo(eName.ToLower().Trim()) == 0)
                {
                    eGenerator.SendEvent(Params);
                    break;
                }
            }
        }
    }
}