﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ModelicaParser.Config;
using ModelicaParser.Changes;

namespace ModelicaParser.Datamodel
{
    public class Connector : ICloneable
    {
        // Backtracking
        private Element parentElement = null;
        private Element source = null;
        private Element target = null;

        // Attributes
        private String type = "";
        private String sourceCardinality = "";
        private String targetCardinality = "";
        private String uid = "";
        private String note = "";

        // Changes
        private int numOfChanges;
        private List<MMChange> changes = new List<MMChange>();

        #region Loading

        public Connector(string type, string sourceCardinality, string targetCardinality, string uid)
        {
            this.type = type;
            this.sourceCardinality = sourceCardinality;
            this.targetCardinality = targetCardinality;
            this.uid = uid;
        }

        public Connector(string type, string sourceCardinality, string targetCardinality, string uid, string note)
        {
            this.type = type;
            this.sourceCardinality = sourceCardinality;
            this.targetCardinality = targetCardinality;
            this.uid = uid;
            this.note = note;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        public override string ToString()
        {
            return "Connector " + source.Name + " -> " + target.Name + "\n";
        }

        #endregion

        #region Calculate number of

        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            return 1;
        }

        #endregion

        #region Retrieve object

        public List<MMChange> GetChanges()
        {
            return new List<MMChange>(changes);
        }

        public string GetPath()
        {
            if(target == null)
                return parentElement.GetPath() + " -> null";
            return parentElement.GetPath() + " -> " + target.GetPath();
        }

        #endregion

        #region Calculation

        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
        }

        public bool IgnoreConector()
        {
            foreach (string str in ConfigReader.ExcludedConnectorTypes)
                if (type.Equals(str))
                    return true;

            return false;
        }

        public int CompareConnectors(Connector oldConnector, bool RelevantOnly)
        {
            if (RelevantOnly && IgnoreConector())
                return 0;

            if (!Equals(SourceCardinality, oldConnector.SourceCardinality))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Source Cardinality: " + oldConnector.SourceCardinality + " -> " + SourceCardinality, false));
            }

            if (!Equals(TargetCardinality, oldConnector.TargetCardinality))
            {

                numOfChanges++;
                changes.Add(new MMChange("~ Target Cardinality (" + UID + "): " + oldConnector.TargetCardinality + " -> " + TargetCardinality, false));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldConnector.Note))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Note", false));
            }

            return numOfChanges;
        }

        #endregion

        #region Getters and setters

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string SourceCardinality
        {
            get { return sourceCardinality; }
            set { sourceCardinality = value; }
        }

        public string TargetCardinality
        {
            get { return targetCardinality; }
            set { targetCardinality = value; }
        }

        public string UID
        {
            get { return uid; }
            set { uid = value; }
        }

        public string Note
        {
            get { return note; }
            set { note = value; }
        }

        public Element ParentElement
        {
            get { return parentElement; }
            set { parentElement = value; }
        }

        public Element Source
        {
            get { return source; }
            set { source = value; }
        }

        public Element Target
        {
            get { return target; }
            set { target = value; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        #endregion
    }
}
