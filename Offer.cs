using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBayKleinanzeigenTracker
{
    class Offer
    {
        public long Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Url { get; }
        public string Thumbnail { get; }
        public string Price { get; }

        public Offer(long id, string title, string description, string url, string thumbnail, string price)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Url = url;
            this.Thumbnail = thumbnail;
            this.Price = price;
        }

    }
}
