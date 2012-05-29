using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActionRecognitionEngine;
using EventGenerationEngine;
using DataAdapter;
using ARInterface;

namespace MiddleEngine
{
    public struct SEVENT
    {
        public string Name;
        public int Priority;
    }
    public class MiddleEngine
    {
        ActionRecognitionEngine.ActionRecognitionEngine _ActionRecoginizer;

        EventGenerationEngine.EventGenerationEngine _EventGenerator;

        Dictionary<string, List<SEVENT>> _ActionEventRelation = new Dictionary<string, List<SEVENT>>();

        /// <summary>
        /// initialize actions and events
        /// </summary>
        /// <param name="ActionRecoginizer">action recognition engine</param>
        /// <param name="EventGenerator">event generator engine</param>
        public void initialize(ActionRecognitionEngine.ActionRecognitionEngine ActionRecoginizer,
            EventGenerationEngine.EventGenerationEngine EventGenerator)
        {
            _ActionRecoginizer = ActionRecoginizer;
            _EventGenerator = EventGenerator;

            string[] Actions = GetActionsName();
            for (int i = 0; i < Actions.Length; i++)
            {
                _ActionEventRelation[Actions[i]] = new List<SEVENT>();
            }
        }

        /// <summary>
        /// Get list of actions
        /// </summary>
        /// <returns></returns>
        public string[] GetActionsName()
        {
            return _ActionRecoginizer.GetActionsName();
        }

        /// <summary>
        /// Get list of events
        /// </summary>
        /// <returns></returns>
        public string[] GetEventsName()
        {
            return _EventGenerator.GetEventsName();
        }

        /// <summary>
        /// bind an action to an event with priority
        /// when action happens, event's raised
        /// </summary>
        /// <param name="sAction">name of action</param>
        /// <param name="sEvent">name of event</param>
        /// <param name="priority">priority</param>
        /// <returns>succeeded or not</returns>
        public bool Bind(string sAction, string sEvent, int priority)
        {
            // check if this action exists
            bool rsl = false;
            int idxA = -1, idxE = -1;
            string[] actions = GetActionsName();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].ToLower().Trim().CompareTo(sAction.ToLower().Trim()) == 0)
                {
                    rsl = true;
                    idxA = i;
                    break;
                }
            }
            if (!rsl)
                return false;

            rsl = false;
            string[] events = GetEventsName();
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].ToLower().Trim().CompareTo(sEvent.ToLower().Trim()) == 0)
                {
                    rsl = true;
                    idxE = i;
                    break;
                }
            }

            if (!rsl)
                return false;

            SEVENT evnt = new SEVENT();
            evnt.Name = sEvent;
            evnt.Priority = priority;
            int cnt = 0;
            bool bExisted = false;
            foreach (SEVENT temp in _ActionEventRelation[sAction])
            {
                cnt ++;
                if (temp.Name.ToLower().Trim().CompareTo(sEvent.ToLower().Trim()) == 0)
                {
                    _ActionEventRelation[sAction][cnt] = evnt;
                    bExisted = true;
                }
            }
            if (!bExisted)
                _ActionEventRelation[sAction].Add(evnt);

            return rsl;
        }

        public void Process(GroupOfFingers[] GOF)
        {
            _ActionRecoginizer.InsertGOF(GOF);
            ARResult[] rsl = _ActionRecoginizer.Recognize();

            string action = "";
            string evnt = "";
            int iAction = -1;
            int highestPri = 99999999;

            // duyet danh sach action
            evnt = "";
            for (int i = 0; i < rsl.Length; i++)
            {
                action = rsl[i].Name;
                //if (action == "LEFT CLICK ACTION")
                //{
                //    int t = 0;
                //}
                if (action.ToLower().Trim().CompareTo("null") != 0)
                {
                    // find event with highest priority
                    foreach (SEVENT sevnt in _ActionEventRelation[action])
                    {
                        if (highestPri > sevnt.Priority)
                        {
                            highestPri = sevnt.Priority;
                            evnt = sevnt.Name;
                            iAction = i;
                        }
                    }
                }
            }

            if (iAction >= 0)
            {
                // raise event
                _EventGenerator.RaiseEvent(evnt, rsl[iAction].Params);
            }
        }
    }
}