using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter
{
    public class LogItem
    {
        private Dictionary<string, object> logContent = new Dictionary<string, object>();

        public object GetField(string fieldName)
        {
            if (!HasField(fieldName))
                throw new KeyNotFoundException();
            return logContent[fieldName];
        }

        public object TryGetField(string fieldName, object defaultValue)
        {
            if (!HasField(fieldName))
                return defaultValue;

            return logContent[fieldName];
        }

        public void SetField(string fieldName, object fieldValue)
        {
            if (fieldName == null || fieldValue == null) return;

            logContent[fieldName] = fieldValue;
        }

        public void ClearField(string fieldName)
        {
            if (HasField(fieldName))
                logContent.Remove(fieldName);
        }

        public bool HasField(string fieldName)
        {
            if (fieldName == null)
                return false;
            return logContent.ContainsKey(fieldName);
        }

        public List<string> FieldNames
        {
            get
            {
                return logContent.Keys.ToList();
            }
        }

        public void Extends(LogItem logItem, bool Override = false)
        {
            if (logItem == null) return;

            foreach (string fieldName in logItem.FieldNames)
            {
                if (Override || !HasField(fieldName))
                    this.SetField(fieldName, logItem.GetField(fieldName));
            }
        }
    }
}
