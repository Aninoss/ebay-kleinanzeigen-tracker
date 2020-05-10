using ObservableCollection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBayKleinanzeigenTracker
{
    public class SearchRequest : ViewModelBase
    {

        private string searchKey = "", priceMin = "", priceMax = "", results = "";
        private List<int> newestIds = new List<int>();
        private DateTime lastUpdate = DateTime.MinValue;

        public SearchRequest() { }

        public SearchRequest(string from)
        {
            string[] parts = from.Split(';');
            if (parts.Length == 6 && parts[0].Length > 0)
            {
                searchKey = parts[0];
                if (parts[1].Length > 0 && StringTools.StringIsInt(parts[1])) priceMin = parts[1];
                if (parts[2].Length > 0 && StringTools.StringIsInt(parts[2])) priceMax = parts[2];
                if (parts[3].Length > 0) newestIds = ParseNewestIdsString(parts[3]);
                if (parts[4].Length > 0) lastUpdate = DateTime.Parse(parts[4]);
                if (parts[5].Length > 0 && StringTools.StringIsInt(parts[5])) results = parts[5];
            }
        }

        public string SearchKey
        {
            get { return searchKey; }
            set
            {
                searchKey = value;
                newestIds = new List<int>();
                ResetDateTime();
                ResetResults();
                Save(); 
            }
        }
        public string PriceMin
        {
            get { return priceMin; }
            set
            {
                priceMin = value;
                Save();
            }
        }
        public string PriceMax
        {
            get { return priceMax; }
            set
            {
                priceMax = value;
                Save();
            }
        }
        public List<int> NewestIds
        {
            get { return newestIds; }
            set
            {
                newestIds = value;
                Save();
            }
        }
        public string LastUpdateString
        {
            get { return lastUpdate.Equals(DateTime.MinValue) ? "" : lastUpdate.ToString(); }
        }
        public string Results
        {
            get { return results; }
            set
            {
                results = value;
                Save();
                OnPropertyChanged("Results");
            }
        }

        public bool IsEmpty() { return SearchKey.Length == 0; }

        private void Save() { SearchRequestContainer.GetInstance().Save(); }

        private string NewestIdsString()
        {
            if (newestIds.Count == 0) return "";

            StringBuilder sb = new StringBuilder();
            foreach (int id in newestIds)
            {
                sb.Append(",").Append(id);
            }

            return sb.ToString().Substring(1);
        }

        private List<int> ParseNewestIdsString(string str)
        {
            List<int> newestIds = new List<int>();

            if (str.Length > 0)
            {
                foreach (string id in str.Split(','))
                {
                    if (StringTools.StringIsInt(id))
                    {
                        newestIds.Add(Int32.Parse(id));
                    }
                }
            }

            return newestIds;
        }

        public void ResetDateTime()
        {
            lastUpdate = DateTime.MinValue;
            OnPropertyChanged("LastUpdateString");
        }

        public void ResetResults()
        {
            results = "";
            OnPropertyChanged("Results");
        }

        public void UpdateDateTime()
        {
            lastUpdate = DateTime.Now;
            OnPropertyChanged("LastUpdateString");
        }

        public override string ToString() { return SearchKey + ";" + PriceMin + ";" + PriceMax + ";" + NewestIdsString() + ";" + lastUpdate.ToUniversalTime().ToString("O") + ";" + results; }

    }
}
