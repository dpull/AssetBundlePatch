using System;
using System.Collections.Generic;
using System.Text;

namespace dpull
{
    public static class EventHelper
    {
        //警告    8    CA1030 
        public static void Raise<TEventArgs>(EventHandler<TEventArgs> hander, object sender, TEventArgs args) 
            where TEventArgs : EventArgs
        {
            //.net设计规范 P128
            EventHandler<TEventArgs> eventHander = hander;
            if (eventHander != null)
            {
                eventHander(sender, args);
            }
        }       
    }
    
}