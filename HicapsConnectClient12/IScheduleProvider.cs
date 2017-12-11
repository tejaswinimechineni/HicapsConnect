using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectClient12
{
    /*
     * IItemRepository
     * 
     * An interface to represent a repository of items. 
     * The idea for using repository pattern came from:
     * http://www.asp.net/web-api/overview/creating-web-apis/creating-a-web-api-that-supports-crud-operations
     */

    public interface IScheduleProvider
    {
        Schedule GetCurrentSchedule();
        string GetCurrentScheduleAsXml();
        void SetCurrentScheduleAsXml(string xml);
        Item Get(string number);
        Item Add(Item item);
        void Remove(string number);
        bool Update(Item item);
        DateTime Expiry { get; }
        void SetCurrentSchedule(Schedule s);
    }
}
