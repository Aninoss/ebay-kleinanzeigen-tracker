using ObservableCollection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;

namespace eBayKleinanzeigenTracker
{
    class SearchRequestContainer
    {

        private static SearchRequestContainer OurInstance = new SearchRequestContainer();
        public static SearchRequestContainer GetInstance() { return OurInstance; }
        public TrulyObservableCollection<SearchRequest> SearchRequests { get; }

        private SearchRequestContainer()
        {
            SearchRequests = new TrulyObservableCollection<SearchRequest>();
            Load();
            SearchRequests.CollectionChanged += Collection_Changed;
            BindingOperations.EnableCollectionSynchronization(SearchRequests, SearchRequests);
        }

        private void Collection_Changed(object sender, NotifyCollectionChangedEventArgs e) { Save(); }

        private bool HasEmptyRequest()
        {
            foreach (SearchRequest searchRequest in SearchRequests)
            {
                if (searchRequest.IsEmpty()) return true;
            }
            return false;
        }

        private void Load()
        {
            string from = Properties.Settings.Default.SearchRequests;
            SearchRequests.Clear();
            if (from != null && from.Length > 0) {
                foreach (string part in from.Split('_'))
                {
                    SearchRequest searchRequest = new SearchRequest(part);
                    if (!searchRequest.IsEmpty()) SearchRequests.Add(searchRequest);
                }
            }
        }

        public void Save()
        {
            Properties.Settings.Default.SearchRequests = ToString();
            Properties.Settings.Default.Save();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(SearchRequest sr in SearchRequests)
            {
                if (!sr.IsEmpty()) {
                    sb.Append("_");
                    sb.Append(sr.ToString());
                }
            }
            if (sb.Length == 0) return "";
            return sb.ToString().Substring(1);
        }

        public void AddEmpty()
        {
            if (!HasEmptyRequest()) 
                SearchRequests.Add(new SearchRequest());
        }

    }

}
